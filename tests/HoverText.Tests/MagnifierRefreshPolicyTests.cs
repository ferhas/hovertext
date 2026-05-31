using HoverText.Core.Overlay;
using HoverText.Core.Probing;

namespace HoverText.Tests;

[TestClass]
public sealed class MagnifierRefreshPolicyTests
{
    [TestMethod]
    public void ShouldCapture_returns_false_when_pointer_is_still_and_cache_is_fresh()
    {
        var previous = new MagnifierFrameState(new PointerPoint(100, 100), TimeSpan.FromMilliseconds(100));

        bool shouldCapture = MagnifierRefreshPolicy.ShouldCapture(
            new PointerPoint(112, 112),
            previous,
            TimeSpan.FromMilliseconds(300));

        Assert.IsFalse(shouldCapture);
    }

    [TestMethod]
    public void ShouldCapture_returns_true_when_pointer_moves_far()
    {
        var previous = new MagnifierFrameState(new PointerPoint(100, 100), TimeSpan.FromMilliseconds(100));

        bool shouldCapture = MagnifierRefreshPolicy.ShouldCapture(
            new PointerPoint(220, 100),
            previous,
            TimeSpan.FromMilliseconds(300));

        Assert.IsTrue(shouldCapture);
    }

    [TestMethod]
    public void ShouldCapture_returns_true_when_cache_is_stale()
    {
        var previous = new MagnifierFrameState(new PointerPoint(100, 100), TimeSpan.FromMilliseconds(100));

        bool shouldCapture = MagnifierRefreshPolicy.ShouldCapture(
            new PointerPoint(100, 100),
            previous,
            TimeSpan.FromMilliseconds(900));

        Assert.IsTrue(shouldCapture);
    }
}
