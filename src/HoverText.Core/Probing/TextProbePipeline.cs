using HoverText.Core.Overlay;
using HoverText.Core.Settings;

namespace HoverText.Core.Probing;

public sealed class TextProbePipeline(
    ITextProbeSource uiAutomation,
    ITextProbeSource ocr,
    IMagnifierFallback magnifier)
{
    public async Task<ProbeResult> ProbeAsync(
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

        MagnifierResult magnifierResult = await magnifier.CaptureAsync(point, cancellationToken);
        return magnifierResult.HasImage ? ProbeResult.FromMagnifier(magnifierResult) : ProbeResult.Empty();
    }
}
