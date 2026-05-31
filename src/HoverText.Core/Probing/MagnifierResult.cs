using HoverText.Core.Overlay;

namespace HoverText.Core.Probing;

public sealed record MagnifierResult(bool HasImage, PixelRect CaptureArea, double Scale)
{
    public static MagnifierResult Captured(PixelRect captureArea, double scale)
    {
        return new MagnifierResult(true, captureArea, scale);
    }

    public static MagnifierResult None()
    {
        return new MagnifierResult(false, new PixelRect(0, 0, 0, 0), 1.0);
    }
}
