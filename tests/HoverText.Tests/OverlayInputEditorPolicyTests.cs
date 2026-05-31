using HoverText.Core.Overlay;
using HoverText.Core.Probing;

namespace HoverText.Tests;

[TestClass]
public sealed class OverlayInputEditorPolicyTests
{
    [TestMethod]
    public void PrepareText_preserves_empty_input_text_without_placeholder_space()
    {
        string text = OverlayInputEditorPolicy.PrepareText(
            "",
            ProbeDisplayKind.Input);

        Assert.AreEqual("", text);
    }

    [TestMethod]
    public void ShouldReplaceEditorText_does_not_overwrite_active_editor_text()
    {
        bool shouldReplace = OverlayInputEditorPolicy.ShouldReplaceEditorText(
            ProbeDisplayKind.Input,
            wasInputMode: true,
            isEditorFocused: true,
            currentText: "用户正在编辑",
            incomingText: "旧内容");

        Assert.IsFalse(shouldReplace);
    }

    [TestMethod]
    public void ShouldReplaceEditorText_fills_initial_text_when_entering_input_mode()
    {
        bool shouldReplace = OverlayInputEditorPolicy.ShouldReplaceEditorText(
            ProbeDisplayKind.Input,
            wasInputMode: false,
            isEditorFocused: true,
            currentText: "",
            incomingText: "原控件内容");

        Assert.IsTrue(shouldReplace);
    }

    [TestMethod]
    public void ShouldFocusEditor_does_not_reset_focus_when_editor_is_already_active()
    {
        bool shouldFocus = OverlayInputEditorPolicy.ShouldFocusEditor(
            ProbeDisplayKind.Input,
            wasInputMode: true,
            isEditorFocused: true);

        Assert.IsFalse(shouldFocus);
    }

    [TestMethod]
    public void ShouldFocusEditor_focuses_when_entering_input_mode()
    {
        bool shouldFocus = OverlayInputEditorPolicy.ShouldFocusEditor(
            ProbeDisplayKind.Input,
            wasInputMode: false,
            isEditorFocused: false);

        Assert.IsTrue(shouldFocus);
    }

    [TestMethod]
    public void ShouldKeepOpenWithoutTrigger_keeps_visible_input_editor_even_before_focus_lands()
    {
        bool shouldKeepOpen = OverlayInputEditorPolicy.ShouldKeepOpenWithoutTrigger(
            isEditorOpen: true);

        Assert.IsTrue(shouldKeepOpen);
    }
}
