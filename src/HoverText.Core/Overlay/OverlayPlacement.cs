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

    public static PixelRect PlaceNearAnchor(
        PixelRect anchor,
        PixelSize overlaySize,
        PixelRect workArea,
        int margin,
        int offset)
    {
        int minX = workArea.Left + margin;
        int minY = workArea.Top + margin;
        int maxX = workArea.Right - margin - overlaySize.Width;
        int maxY = workArea.Bottom - margin - overlaySize.Height;

        int anchorCenterX = anchor.Left + anchor.Width / 2;
        int x = anchorCenterX - overlaySize.Width / 2;
        int y = anchor.Top - overlaySize.Height - offset;

        if (y < minY)
        {
            y = anchor.Bottom + offset;
        }

        x = Math.Clamp(x, minX, Math.Max(minX, maxX));
        y = Math.Clamp(y, minY, Math.Max(minY, maxY));

        return new PixelRect(x, y, overlaySize.Width, overlaySize.Height);
    }
}
