using HoverText.Core.Probing;

namespace HoverText.Tests;

[TestClass]
public sealed class ProbeTextNormalizerTests
{
    [TestMethod]
    public void NormalizeNonInput_collapses_whitespace()
    {
        Assert.AreEqual("置顶 对话", ProbeTextNormalizer.NormalizeNonInput("  置顶\r\n对话  "));
    }

    [TestMethod]
    public void NormalizeNonInput_removes_adjacent_duplicate_tokens()
    {
        Assert.AreEqual("置顶对话", ProbeTextNormalizer.NormalizeNonInput("置顶对话 置顶对话"));
    }

    [TestMethod]
    public void NormalizeNonInput_preserves_different_action_tokens()
    {
        Assert.AreEqual("置顶对话 归档对话", ProbeTextNormalizer.NormalizeNonInput("置顶对话 归档对话"));
    }
}
