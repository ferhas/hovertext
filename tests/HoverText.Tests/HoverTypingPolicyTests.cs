using HoverText.Core.Probing;
using HoverText.Core.Settings;

namespace HoverText.Tests;

[TestClass]
public sealed class HoverTypingPolicyTests
{
    [TestMethod]
    public void ShouldShowWithoutTrigger_returns_true_for_enabled_input_result()
    {
        ProbeResult result = ProbeResult.FromText(
            TextProbeResult.Found("正在输入", ProbeSource.UiAutomation, ProbeDisplayKind.Input));

        Assert.IsTrue(HoverTypingPolicy.ShouldShowWithoutTrigger(HoverTextSettings.CreateDefault(), result));
    }

    [TestMethod]
    public void ShouldShowWithoutTrigger_returns_false_when_hover_typing_is_disabled()
    {
        ProbeResult result = ProbeResult.FromText(
            TextProbeResult.Found("正在输入", ProbeSource.UiAutomation, ProbeDisplayKind.Input));
        HoverTextSettings settings = HoverTextSettings.CreateDefault() with { IsHoverTypingEnabled = false };

        Assert.IsFalse(HoverTypingPolicy.ShouldShowWithoutTrigger(settings, result));
    }

    [TestMethod]
    public void ShouldShowWithoutTrigger_returns_false_for_non_input_result()
    {
        ProbeResult result = ProbeResult.FromText(
            TextProbeResult.Found("普通文本", ProbeSource.UiAutomation, ProbeDisplayKind.Text));

        Assert.IsFalse(HoverTypingPolicy.ShouldShowWithoutTrigger(HoverTextSettings.CreateDefault(), result));
    }
}
