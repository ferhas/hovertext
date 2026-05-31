using HoverText.Core.Probing;

namespace HoverText.Tests;

[TestClass]
public sealed class HoverTypingActivityGateTests
{
    [TestMethod]
    public void HasRecentActivity_is_false_before_typing()
    {
        var gate = new HoverTypingActivityGate(TimeSpan.FromMilliseconds(900));

        Assert.IsFalse(gate.HasRecentActivity(TimeSpan.Zero));
    }

    [TestMethod]
    public void HasRecentActivity_is_true_inside_activity_window()
    {
        var gate = new HoverTypingActivityGate(TimeSpan.FromMilliseconds(900));

        gate.RecordTypingActivity(TimeSpan.FromMilliseconds(1000));

        Assert.IsTrue(gate.HasRecentActivity(TimeSpan.FromMilliseconds(1600)));
    }

    [TestMethod]
    public void HasRecentActivity_is_false_after_activity_window()
    {
        var gate = new HoverTypingActivityGate(TimeSpan.FromMilliseconds(900));

        gate.RecordTypingActivity(TimeSpan.FromMilliseconds(1000));

        Assert.IsFalse(gate.HasRecentActivity(TimeSpan.FromMilliseconds(2001)));
    }
}
