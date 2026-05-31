namespace HoverText.Core.Overlay;

public readonly record struct PointerPoint(int X, int Y);

public readonly record struct PixelSize(int Width, int Height);

public readonly record struct PixelRect(int Left, int Top, int Width, int Height)
{
    public int Right => Left + Width;

    public int Bottom => Top + Height;
}
