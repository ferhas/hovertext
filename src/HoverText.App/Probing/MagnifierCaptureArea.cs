using HoverText.Core.Overlay;

namespace HoverText.App.Probing;

public static class MagnifierCaptureArea
{
    public const int Width = 240;
    public const int Height = 140;

    public static PixelRect FromPointer(PointerPoint point)
    {
        int x = Math.Max(0, point.X - Width / 2);
        int y = Math.Max(0, point.Y - Height / 2);
        return new PixelRect(x, y, Width, Height);
    }
}
