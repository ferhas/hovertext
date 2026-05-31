using HoverText.Core.Platform;

namespace HoverText.Tests;

[TestClass]
public sealed class WindowCaptureAffinityTests
{
    [TestMethod]
    public void ExcludeFromCapture_matches_windows_constant()
    {
        Assert.AreEqual(0x00000011u, (uint)WindowCaptureAffinity.ExcludeFromCapture);
    }

    [TestMethod]
    public void MonitorOnly_matches_fallback_windows_constant()
    {
        Assert.AreEqual(0x00000001u, (uint)WindowCaptureAffinity.MonitorOnly);
    }
}
