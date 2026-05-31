using System.Windows.Threading;
using HoverText.App.Input;
using HoverText.App.Probing;
using HoverText.App.Ui;
using HoverText.Core.Overlay;
using HoverText.Core.Probing;
using HoverText.Core.Settings;

namespace HoverText.App;

public sealed class HoverTextController : IDisposable
{
    private readonly OverlayWindow overlayWindow;
    private readonly TextProbePipeline pipeline;
    private readonly ScreenMagnifierFallback magnifier;
    private readonly KeyboardTriggerReader triggerReader;
    private readonly CursorPositionProvider cursorProvider;
    private readonly DispatcherTimer timer;
    private readonly DateTime startedAt = DateTime.UtcNow;
    private bool isTicking;
    private HoverTextSettings settings;
    private ProbeResult? cachedMagnifierResult;
    private MagnifierFrameState? cachedMagnifierFrame;

    public HoverTextController(
        OverlayWindow overlayWindow,
        TextProbePipeline pipeline,
        ScreenMagnifierFallback magnifier,
        KeyboardTriggerReader triggerReader,
        CursorPositionProvider cursorProvider,
        HoverTextSettings settings)
    {
        this.overlayWindow = overlayWindow;
        this.pipeline = pipeline;
        this.magnifier = magnifier;
        this.triggerReader = triggerReader;
        this.cursorProvider = cursorProvider;
        this.settings = settings;
        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(settings.PollIntervalMilliseconds)
        };
        timer.Tick += OnTick;
    }

    public void Start()
    {
        timer.Start();
    }

    public void UpdateSettings(HoverTextSettings updatedSettings)
    {
        settings = updatedSettings;
        timer.Interval = TimeSpan.FromMilliseconds(settings.PollIntervalMilliseconds);
    }

    public void Dispose()
    {
        timer.Stop();
        timer.Tick -= OnTick;
        overlayWindow.Close();
    }

    private async void OnTick(object? sender, EventArgs e)
    {
        if (isTicking)
        {
            return;
        }

        if (!triggerReader.IsPressed(settings.TriggerKey))
        {
            overlayWindow.Hide();
            return;
        }

        isTicking = true;
        try
        {
            PointerPoint point = cursorProvider.GetCursorPosition();
            if (cachedMagnifierResult is not null
                && cachedMagnifierFrame is not null
                && !MagnifierRefreshPolicy.ShouldCapture(point, cachedMagnifierFrame, ElapsedSinceStart()))
            {
                overlayWindow.ShowProbeResult(cachedMagnifierResult, settings, point, magnifier.LastCapture);
                return;
            }

            ProbeResult result = await pipeline.ProbeAsync(point, settings);
            if (result.DisplayKind == ProbeDisplayKind.Magnifier)
            {
                cachedMagnifierResult = result;
                cachedMagnifierFrame = new MagnifierFrameState(point, ElapsedSinceStart());
            }
            else
            {
                cachedMagnifierResult = null;
                cachedMagnifierFrame = null;
            }

            overlayWindow.ShowProbeResult(result, settings, point, magnifier.LastCapture);
        }
        catch
        {
            overlayWindow.Hide();
        }
        finally
        {
            isTicking = false;
        }
    }

    private TimeSpan ElapsedSinceStart()
    {
        return DateTime.UtcNow - startedAt;
    }
}
