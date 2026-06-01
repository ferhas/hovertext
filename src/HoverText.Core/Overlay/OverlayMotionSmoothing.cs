namespace HoverText.Core.Overlay;

public static class OverlayMotionSmoothing
{
    public const int DefaultMaxStepPixels = 32;

    public static PixelRect MoveToward(PixelRect? current, PixelRect target, int maxStepPixels = DefaultMaxStepPixels)
    {
        if (current is null || maxStepPixels <= 0)
        {
            return target;
        }

        int dx = target.Left - current.Value.Left;
        int dy = target.Top - current.Value.Top;
        double distance = Math.Sqrt((dx * dx) + (dy * dy));
        if (distance <= maxStepPixels)
        {
            return target;
        }

        double ratio = maxStepPixels / distance;
        int left = current.Value.Left + (int)Math.Round(dx * ratio);
        int top = current.Value.Top + (int)Math.Round(dy * ratio);
        return new PixelRect(left, top, target.Width, target.Height);
    }
}
