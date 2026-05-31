using HoverText.Core.Probing;

namespace HoverText.Tests;

[TestClass]
public sealed class ProbeTextSelectionTests
{
    [TestMethod]
    public void ForActionLike_prefers_name_over_help_text()
    {
        ProbeTextSelection selection = ProbeTextSelection.ForActionLike(
            name: "置顶对话",
            value: null,
            helpText: "归档对话");

        Assert.AreEqual("置顶对话", selection.Text);
        Assert.AreEqual(ProbeDisplayKind.Text, selection.DisplayKind);
    }

    [TestMethod]
    public void ForActionLike_prefers_value_over_help_text_when_name_is_missing()
    {
        ProbeTextSelection selection = ProbeTextSelection.ForActionLike(
            name: null,
            value: "真实按钮文本",
            helpText: "提示文本");

        Assert.AreEqual("真实按钮文本", selection.Text);
        Assert.AreEqual(ProbeDisplayKind.Text, selection.DisplayKind);
    }

    [TestMethod]
    public void ForActionLike_uses_help_text_only_as_tip_fallback()
    {
        ProbeTextSelection selection = ProbeTextSelection.ForActionLike(
            name: null,
            value: null,
            helpText: "归档对话");

        Assert.AreEqual("归档对话", selection.Text);
        Assert.AreEqual(ProbeDisplayKind.Tooltip, selection.DisplayKind);
    }
}
