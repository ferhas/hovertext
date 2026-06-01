using HoverText.Core.Overlay;
using HoverText.Core.Probing;

namespace HoverText.Tests;

[TestClass]
public sealed class MagnifierMotionResponsivenessTests
{
    [TestMethod]
    public void Active_magnifier_uses_small_motion_step_for_smoother_tracking()
    {
        var current = new PixelRect(0, 0, 120, 80);
        var target = new PixelRect(200, 0, 120, 80);

        PixelRect placement = OverlayMotionSmoothing.MoveToward(current, target);

        Assert.AreEqual(32, placement.Left);
    }

    [TestMethod]
    public void Active_magnifier_refreshes_before_motion_can_jump_by_a_large_chunk()
    {
        var previous = new MagnifierFrameState(new PointerPoint(100, 100), TimeSpan.FromMilliseconds(100));

        bool shouldCapture = MagnifierRefreshPolicy.ShouldCapture(
            new PointerPoint(110, 100),
            previous,
            TimeSpan.FromMilliseconds(116));

        Assert.IsTrue(shouldCapture);
    }
}
