using HoverText.Core.Overlay;

namespace HoverText.Tests;

[TestClass]
public sealed class OverlayMotionSmoothingTests
{
    [TestMethod]
    public void MoveToward_returns_target_without_previous_position()
    {
        var target = new PixelRect(300, 200, 120, 80);

        PixelRect placement = OverlayMotionSmoothing.MoveToward(null, target, maxStepPixels: 40);

        Assert.AreEqual(target, placement);
    }

    [TestMethod]
    public void MoveToward_limits_large_position_jumps()
    {
        var current = new PixelRect(0, 0, 120, 80);
        var target = new PixelRect(300, 0, 120, 80);

        PixelRect placement = OverlayMotionSmoothing.MoveToward(current, target, maxStepPixels: 96);

        Assert.AreEqual(96, placement.Left);
        Assert.AreEqual(0, placement.Top);
        Assert.AreEqual(target.Width, placement.Width);
        Assert.AreEqual(target.Height, placement.Height);
    }

    [TestMethod]
    public void MoveToward_snaps_to_target_for_small_position_changes()
    {
        var current = new PixelRect(100, 80, 120, 80);
        var target = new PixelRect(130, 100, 120, 80);

        PixelRect placement = OverlayMotionSmoothing.MoveToward(current, target, maxStepPixels: 96);

        Assert.AreEqual(target, placement);
    }
}
