using HoverText.Core.Overlay;

namespace HoverText.Core.Probing;

public sealed record MagnifierFrameState(PointerPoint Point, TimeSpan CapturedAt);

public static class MagnifierRefreshPolicy
{
    public static readonly TimeSpan MaxReuseAge = TimeSpan.FromMilliseconds(650);
    public const int MaxReuseDistancePixels = 8;

    public static bool ShouldCapture(
        PointerPoint point,
        MagnifierFrameState? previous,
        TimeSpan now,
        bool canReuseCapturedFrame = true)
    {
        if (!canReuseCapturedFrame)
        {
            return true;
        }

        if (previous is null)
        {
            return true;
        }

        if (now - previous.CapturedAt > MaxReuseAge)
        {
            return true;
        }

        int dx = point.X - previous.Point.X;
        int dy = point.Y - previous.Point.Y;
        return (dx * dx) + (dy * dy) > MaxReuseDistancePixels * MaxReuseDistancePixels;
    }
}
