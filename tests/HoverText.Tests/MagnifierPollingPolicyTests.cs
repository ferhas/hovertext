using HoverText.Core.Probing;

namespace HoverText.Tests;

[TestClass]
public sealed class MagnifierPollingPolicyTests
{
    [TestMethod]
    public void GetInterval_uses_fast_interval_while_magnifier_is_pressed()
    {
        TimeSpan interval = MagnifierPollingPolicy.GetInterval(
            configuredInterval: TimeSpan.FromMilliseconds(150),
            isMagnifierPressed: true);

        Assert.AreEqual(TimeSpan.FromMilliseconds(16), interval);
    }

    [TestMethod]
    public void GetInterval_uses_configured_interval_when_magnifier_is_not_pressed()
    {
        TimeSpan interval = MagnifierPollingPolicy.GetInterval(
            configuredInterval: TimeSpan.FromMilliseconds(150),
            isMagnifierPressed: false);

        Assert.AreEqual(TimeSpan.FromMilliseconds(150), interval);
    }
}
