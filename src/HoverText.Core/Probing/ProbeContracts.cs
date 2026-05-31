using HoverText.Core.Overlay;

namespace HoverText.Core.Probing;

public interface ITextProbeSource
{
    Task<TextProbeResult> TryReadAsync(PointerPoint point, CancellationToken cancellationToken = default);
}

public interface IFocusedInputProbeSource
{
    Task<TextProbeResult> TryReadFocusedInputAsync(CancellationToken cancellationToken = default);

    Task<bool> TryWriteFocusedInputAsync(string text, CancellationToken cancellationToken = default);
}

public interface IMagnifierFallback
{
    Task<MagnifierResult> CaptureAsync(PointerPoint point, CancellationToken cancellationToken = default);
}
