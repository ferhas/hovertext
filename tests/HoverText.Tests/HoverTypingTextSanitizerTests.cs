using HoverText.Core.Typing;

namespace HoverText.Tests;

[TestClass]
public sealed class HoverTypingTextSanitizerTests
{
    [TestMethod]
    public void NormalizeForDisplay_removes_prosemirror_placeholder_echo()
    {
        string text = HoverTypingTextSanitizer.NormalizeForDisplay(
            "\r\n要求后续变更",
            "ProseMirror ProseMirror-focused",
            ["要求后续变更"]);

        Assert.AreEqual(string.Empty, text);
    }

    [TestMethod]
    public void NormalizeForDisplay_keeps_actual_prosemirror_input()
    {
        string text = HoverTypingTextSanitizer.NormalizeForDisplay(
            "\r\n修复 Codex 输入框",
            "ProseMirror",
            ["要求后续变更"]);

        Assert.AreEqual("修复 Codex 输入框", text);
    }
}
