using HoverText.Core.Platform;

namespace HoverText.Tests;

[TestClass]
public sealed class StartupUiPolicyTests
{
    [TestMethod]
    public void Default_policy_shows_taskbar_close_surface()
    {
        StartupUiPolicy policy = StartupUiPolicy.CreateDefault();

        Assert.IsTrue(policy.ShowTaskbarControlWindow);
        Assert.IsTrue(policy.ControlWindowCloseExitsApplication);
    }
}
