using HoverText.Core.Overlay;
using HoverText.Core.Settings;

namespace HoverText.Core.Probing;

public sealed class TextProbePipeline(
    ITextProbeSource uiAutomation,
    ITextProbeSource ocr,
    IMagnifierFallback magnifier,
    Func<Task>? beforeMagnifierCaptureAsync = null)
{
    private IMagnifierFallback magnifier = magnifier;

    public void SetMagnifier(IMagnifierFallback updatedMagnifier)
    {
        magnifier = updatedMagnifier;
    }

    public async Task<ProbeResult> ProbeAsync(
        PointerPoint point,
        HoverTextSettings settings,
        CancellationToken cancellationToken = default)
    {
        ProbeResult textResult = await ProbeTextAsync(point, settings, cancellationToken);
        if (textResult.DisplayKind != ProbeDisplayKind.Empty)
        {
            return textResult;
        }

        return await CaptureMagnifierAsync(point, settings, cancellationToken);
    }

    public async Task<ProbeResult> ProbeTextAsync(
        PointerPoint point,
        HoverTextSettings settings,
        CancellationToken cancellationToken = default)
    {
        TextProbeResult automationResult = await uiAutomation.TryReadAsync(point, cancellationToken);
        if (automationResult.HasText)
        {
            return ProbeResult.FromText(automationResult);
        }

        if (settings.IsOcrEnabled)
        {
            TextProbeResult ocrResult = await ocr.TryReadAsync(point, cancellationToken);
            if (ocrResult.HasText)
            {
                return ProbeResult.FromText(ocrResult);
            }
        }

        return ProbeResult.Empty();
    }

    public async Task<ProbeResult> CaptureMagnifierAsync(
        PointerPoint point,
        HoverTextSettings settings,
        CancellationToken cancellationToken = default)
    {
        if (!settings.IsMagnifierEnabled)
        {
            return ProbeResult.Empty();
        }

        if (beforeMagnifierCaptureAsync is not null)
        {
            await beforeMagnifierCaptureAsync();
        }

        MagnifierResult magnifierResult = await magnifier.CaptureAsync(point, cancellationToken);
        return magnifierResult.HasImage ? ProbeResult.FromMagnifier(magnifierResult) : ProbeResult.Empty();
    }
}
