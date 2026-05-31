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
    private readonly IFocusedInputProbeSource focusedInputProbe;
    private readonly ScreenMagnifierFallback magnifier;
    private readonly KeyboardTriggerReader triggerReader;
    private readonly CursorPositionProvider cursorProvider;
    private readonly DispatcherTimer timer;
    private readonly DateTime startedAt = DateTime.UtcNow;
    private readonly HoverTypingActivityGate hoverTypingActivityGate = HoverTypingActivityGate.CreateDefault();
    private bool isTicking;
    private HoverTextSettings settings;
    private ProbeResult? cachedMagnifierResult;
    private MagnifierFrameState? cachedMagnifierFrame;

    public HoverTextController(
        OverlayWindow overlayWindow,
        TextProbePipeline pipeline,
        IFocusedInputProbeSource focusedInputProbe,
        ScreenMagnifierFallback magnifier,
        KeyboardTriggerReader triggerReader,
        CursorPositionProvider cursorProvider,
        HoverTextSettings settings)
    {
        this.overlayWindow = overlayWindow;
        this.pipeline = pipeline;
        this.focusedInputProbe = focusedInputProbe;
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
        if (!settings.IsMagnifierEnabled || !settings.IsHoverTypingEnabled)
        {
            ClearMagnifierCache();
            overlayWindow.Hide();
        }
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

        isTicking = true;
        try
        {
            PointerPoint point = cursorProvider.GetCursorPosition();
            if (!triggerReader.IsPressed(settings.TriggerKey))
            {
                TimeSpan now = ElapsedSinceStart();
                if (triggerReader.HasTypingActivity())
                {
                    hoverTypingActivityGate.RecordTypingActivity(now);
                }

                await ShowFocusedInputWithoutTrigger(point, hoverTypingActivityGate.HasRecentActivity(now));
                return;
            }

            if (settings.IsMagnifierEnabled
                && cachedMagnifierResult is not null
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
                ClearMagnifierCache();
            }

            if (result.DisplayKind == ProbeDisplayKind.Empty || result.Source == ProbeSource.None)
            {
                overlayWindow.Hide();
                return;
            }

            overlayWindow.ShowProbeResult(result, settings, point, magnifier.LastCapture);
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

    private void ClearMagnifierCache()
    {
        cachedMagnifierResult = null;
        cachedMagnifierFrame = null;
    }

    private TimeSpan ElapsedSinceStart()
    {
        return DateTime.UtcNow - startedAt;
    }

    private async Task ShowFocusedInputWithoutTrigger(PointerPoint point, bool hasRecentTypingActivity)
    {
        ClearMagnifierCache();
        if (!settings.IsHoverTypingEnabled || !hasRecentTypingActivity)
        {
            overlayWindow.Hide();
            return;
        }

        TextProbeResult textResult = await focusedInputProbe.TryReadFocusedInputAsync();
        ProbeResult result = ProbeResult.FromText(textResult);
        if (!HoverTypingPolicy.ShouldShowWithoutTrigger(settings, result, hasRecentTypingActivity))
        {
            overlayWindow.Hide();
            return;
        }

        overlayWindow.ShowProbeResult(result, settings, point, magnifier.LastCapture);
    }
}
