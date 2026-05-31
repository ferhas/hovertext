using HoverText.Core.Platform;

namespace HoverText.Tests;

[TestClass]
public sealed class StartupUiPolicyTests
{
    [TestMethod]
    public void Default_policy_starts_minimized_to_tray()
    {
        StartupUiPolicy policy = StartupUiPolicy.CreateDefault();

        Assert.IsFalse(policy.ShowTaskbarControlWindow);
        Assert.IsTrue(policy.ControlWindowCloseExitsApplication);
    }
}
