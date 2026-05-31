using HoverText.Core.Overlay;

namespace HoverText.Core.Probing;

public interface ITextProbeSource
{
    Task<TextProbeResult> TryReadAsync(PointerPoint point, CancellationToken cancellationToken = default);
}

public interface IMagnifierFallback
{
    Task<MagnifierResult> CaptureAsync(PointerPoint point, CancellationToken cancellationToken = default);
}
