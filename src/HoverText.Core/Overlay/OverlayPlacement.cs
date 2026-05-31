namespace HoverText.Core.Overlay;

public static class OverlayPlacement
{
    public static PixelRect PlaceNearCursor(
        PointerPoint cursor,
        PixelSize overlaySize,
        PixelRect workArea,
        int margin,
        int offset)
    {
        int x = cursor.X + offset;
        int y = cursor.Y + offset;

        int minX = workArea.Left + margin;
        int minY = workArea.Top + margin;
        int maxX = workArea.Right - margin - overlaySize.Width;
        int maxY = workArea.Bottom - margin - overlaySize.Height;

        if (x > maxX)
        {
            x = cursor.X - offset - overlaySize.Width;
        }

        if (y > maxY)
        {
            y = cursor.Y - offset - overlaySize.Height;
        }

        x = Math.Clamp(x, minX, Math.Max(minX, maxX));
        y = Math.Clamp(y, minY, Math.Max(minY, maxY));

        return new PixelRect(x, y, overlaySize.Width, overlaySize.Height);
    }
}
