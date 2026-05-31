using HoverText.Core.Probing;

namespace HoverText.Tests;

[TestClass]
public sealed class MagnifierDefaultsTests
{
    [TestMethod]
    public void Default_magnifier_scale_is_three_times()
    {
        Assert.AreEqual(3.0, MagnifierDefaults.Scale);
    }
}
