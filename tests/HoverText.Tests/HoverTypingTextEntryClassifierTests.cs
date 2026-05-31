using HoverText.Core.Typing;

namespace HoverText.Tests;

[TestClass]
public sealed class HoverTypingTextEntryClassifierTests
{
    [TestMethod]
    public void IsEditableTextEntry_accepts_keyboard_focusable_text_controls_with_readable_text_patterns()
    {
        bool isEditable = HoverTypingTextEntryClassifier.IsEditableTextEntry(
            controlTypeName: "ControlType.Text",
            className: string.Empty,
            isKeyboardFocusable: true,
            isEnabled: true,
            supportsValuePattern: false,
            supportsTextPattern: true);

        Assert.IsTrue(isEditable);
    }

    [TestMethod]
    public void IsEditableTextEntry_rejects_non_focusable_static_text()
    {
        bool isEditable = HoverTypingTextEntryClassifier.IsEditableTextEntry(
            controlTypeName: "ControlType.Text",
            className: string.Empty,
            isKeyboardFocusable: false,
            isEnabled: true,
            supportsValuePattern: false,
            supportsTextPattern: true);

        Assert.IsFalse(isEditable);
    }

    [TestMethod]
    public void IsEditableTextEntry_accepts_codeditor_prosemirror_group()
    {
        bool isEditable = HoverTypingTextEntryClassifier.IsEditableTextEntry(
            controlTypeName: "ControlType.Group",
            className: "ProseMirror ProseMirror-focused",
            isKeyboardFocusable: true,
            isEnabled: true,
            supportsValuePattern: false,
            supportsTextPattern: true);

        Assert.IsTrue(isEditable);
    }
}
