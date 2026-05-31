using System.Windows;
using HoverText.App.Input;
using HoverText.App.Platform;
using HoverText.App.Probing;
using HoverText.App.Ui;
using HoverText.Core.Platform;
using HoverText.Core.Probing;
using HoverText.Core.Settings;

namespace HoverText.App;

public partial class App : System.Windows.Application
{
    private HoverTextController? controller;
    private TrayIconService? trayIcon;
    private JsonSettingsStore? settingsStore;
    private HoverTextSettings settings = HoverTextSettings.CreateDefault();
    private StartupManager? startupManager;
    private SingleInstanceLock? singleInstanceLock;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        singleInstanceLock = SingleInstanceLock.TryAcquire(@"Local\HoverText.App");
        if (!singleInstanceLock.IsPrimaryInstance)
        {
            Shutdown();
            return;
        }

        settingsStore = new JsonSettingsStore(AppPaths.SettingsFilePath);
        settings = await settingsStore.LoadAsync();

        startupManager = new StartupManager("HoverText");
        startupManager.SetEnabled(settings.StartWithWindows);

        var overlay = new OverlayWindow();
        var automation = new UiAutomationTextProbeSource();
        var ocr = new TesseractCliOcrTextProbeSource();
        var magnifier = new ScreenMagnifierFallback();
        var pipeline = new TextProbePipeline(automation, ocr, magnifier, () =>
        {
            overlay.Hide();
            return Task.CompletedTask;
        });

        controller = new HoverTextController(
            overlay,
            pipeline,
            magnifier,
            new KeyboardTriggerReader(),
            new CursorPositionProvider(),
            settings);
        controller.Start();

        trayIcon = new TrayIconService(
            settings,
            OpenSettingsWindow,
            ToggleOcr,
            ToggleStartup,
            () => Shutdown());

        StartupUiPolicy uiPolicy = StartupUiPolicy.CreateDefault();
        if (uiPolicy.ShowTaskbarControlWindow || e.Args.Contains("--show-window", StringComparer.OrdinalIgnoreCase))
        {
            OpenSettingsWindow();
        }

        if (e.Args.Contains("--show-settings", StringComparer.OrdinalIgnoreCase))
        {
            OpenSettingsWindow();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        controller?.Dispose();
        trayIcon?.Dispose();
        singleInstanceLock?.Dispose();
        base.OnExit(e);
    }

    private void OpenSettingsWindow()
    {
        if (settingsStore is null)
        {
            return;
        }

        var window = new SettingsWindow(settings, SaveSettingsAsync);
        window.Show();
        window.Activate();
    }

    private async Task SaveSettingsAsync(HoverTextSettings updated)
    {
        if (settingsStore is null || startupManager is null)
        {
            return;
        }

        settings = updated;
        await settingsStore.SaveAsync(settings);
        startupManager.SetEnabled(settings.StartWithWindows);
        controller?.UpdateSettings(settings);
        trayIcon?.UpdateSettings(settings);
    }

    private async void ToggleOcr()
    {
        await SaveSettingsAsync(settings with { IsOcrEnabled = !settings.IsOcrEnabled });
    }

    private async void ToggleStartup()
    {
        await SaveSettingsAsync(settings with { StartWithWindows = !settings.StartWithWindows });
    }
}
