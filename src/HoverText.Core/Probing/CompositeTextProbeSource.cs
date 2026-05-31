using HoverText.Core.Overlay;

namespace HoverText.Core.Probing;

public sealed class CompositeTextProbeSource(params ITextProbeSource[] sources) : ITextProbeSource
{
    public async Task<TextProbeResult> TryReadAsync(PointerPoint point, CancellationToken cancellationToken = default)
    {
        foreach (ITextProbeSource source in sources)
        {
            TextProbeResult result = await source.TryReadAsync(point, cancellationToken);
            if (result.HasText)
            {
                return result;
            }
        }

        return TextProbeResult.None(ProbeSource.Ocr);
    }
}
