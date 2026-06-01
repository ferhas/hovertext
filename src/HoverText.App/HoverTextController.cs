using System.Windows.Threading;
using HoverText.App.Input;
using HoverText.App.Probing;
using HoverText.App.Ui;
using HoverText.Core.Overlay;
using HoverText.Core.Probing;
using HoverText.Core.Settings;
using HoverText.Core.Typing;

namespace HoverText.App;

public sealed class HoverTextController : IDisposable
{
    private readonly OverlayWindow overlayWindow;
    private readonly TextProbePipeline pipeline;
    private readonly IHoverTypingProbeSource hoverTypingProbe;
    private readonly HoverTypingSession hoverTypingSession = new();
    private IMagnifierBackend magnifier;
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
        IHoverTypingProbeSource hoverTypingProbe,
        IMagnifierBackend magnifier,
        KeyboardTriggerReader triggerReader,
        CursorPositionProvider cursorProvider,
        HoverTextSettings settings)
    {
        this.overlayWindow = overlayWindow;
        this.pipeline = pipeline;
        this.hoverTypingProbe = hoverTypingProbe;
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
        if (magnifier.Kind != settings.MagnifierBackend)
        {
            magnifier.Dispose();
            magnifier = MagnifierBackendFactory.Create(settings);
            pipeline.SetMagnifier(magnifier);
            ClearMagnifierCache();
        }

        if (!settings.IsMagnifierEnabled)
        {
            ClearMagnifierCache();
            overlayWindow.Hide();
        }
    }

    public void Dispose()
    {
        timer.Stop();
        timer.Tick -= OnTick;
        magnifier.Dispose();
        overlayWindow.Close();
    }

    private async void OnTick(object? sender, EventArgs e)
    {
        if (isTicking)
        {
            return;
        }

        isTicking = true;
        try
        {
            PointerPoint point = cursorProvider.GetCursorPosition();
            bool magnifierPressed = triggerReader.IsMagnifierPressed();
            UpdateTimerInterval(magnifierPressed);
            bool hoverTextPressed = triggerReader.IsPressed(settings.TriggerKey);
            if (!magnifierPressed && !hoverTextPressed)
            {
                ClearMagnifierCache();
                if (settings.IsHoverTypingEnabled)
                {
                    HoverTypingSnapshot snapshot = await hoverTypingProbe.ReadAsync();
                    HoverTypingDisplay display = hoverTypingSession.Evaluate(
                        settings,
                        snapshot,
                        triggerReader.IsEscapePressed());
                    if (display.IsVisible)
                    {
                        overlayWindow.ShowHoverTypingDisplay(display, settings, point);
                        return;
                    }
                }

                overlayWindow.Hide();
                return;
            }

            if (magnifierPressed)
            {
                await ShowMagnifierAsync(point);
                return;
            }

            ClearMagnifierCache();
            ProbeResult result = await pipeline.ProbeTextAsync(point, settings);
            if (result.DisplayKind == ProbeDisplayKind.Empty || result.Source == ProbeSource.None)
            {
                overlayWindow.Hide();
                return;
            }

            overlayWindow.ShowProbeResult(result, settings, point, null);
        }
        catch
        {
            overlayWindow.Hide();
            ClearMagnifierCache();
        }
        finally
        {
            isTicking = false;
        }
    }

    private async Task ShowMagnifierAsync(PointerPoint point)
    {
        if (settings.IsMagnifierEnabled
                && cachedMagnifierResult is not null
                && cachedMagnifierFrame is not null
                && !MagnifierRefreshPolicy.ShouldCapture(
                    point,
                    cachedMagnifierFrame,
                    ElapsedSinceStart(),
                    magnifier.CanReuseCapture))
        {
            if (magnifier.UsesExternalWindow && cachedMagnifierResult.Magnifier is not null)
            {
                overlayWindow.Hide();
                magnifier.ShowExternal(point, cachedMagnifierResult.Magnifier, settings);
                return;
            }

            magnifier.HideExternal();
            overlayWindow.ShowProbeResult(cachedMagnifierResult, settings, point, magnifier.LastCapture);
            return;
        }

        ProbeResult result = await pipeline.CaptureMagnifierAsync(point, settings);
        if (result.DisplayKind == ProbeDisplayKind.Magnifier)
        {
            cachedMagnifierResult = result;
            cachedMagnifierFrame = new MagnifierFrameState(point, ElapsedSinceStart());
        }
        else
        {
            ClearMagnifierCache();
        }

        if (result.DisplayKind == ProbeDisplayKind.Empty || result.Source == ProbeSource.None)
        {
            overlayWindow.Hide();
            magnifier.HideExternal();
            return;
        }

        if (magnifier.UsesExternalWindow && result.Magnifier is not null)
        {
            overlayWindow.Hide();
            magnifier.ShowExternal(point, result.Magnifier, settings);
            return;
        }

        magnifier.HideExternal();
        overlayWindow.ShowProbeResult(result, settings, point, magnifier.LastCapture);
    }

    private void ClearMagnifierCache()
    {
        cachedMagnifierResult = null;
        cachedMagnifierFrame = null;
        magnifier.HideExternal();
    }

    private void UpdateTimerInterval(bool magnifierPressed)
    {
        TimeSpan configuredInterval = TimeSpan.FromMilliseconds(settings.PollIntervalMilliseconds);
        TimeSpan interval = MagnifierPollingPolicy.GetInterval(configuredInterval, magnifierPressed);
        if (timer.Interval != interval)
        {
            timer.Interval = interval;
        }
    }

    private TimeSpan ElapsedSinceStart()
    {
        return DateTime.UtcNow - startedAt;
    }

}
