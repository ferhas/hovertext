using HoverText.Core.Typing;

namespace HoverText.Tests;

[TestClass]
public sealed class InputTextDisplayPolicyTests
{
    [TestMethod]
    public void ShouldDisplay_allows_text_at_300_characters()
    {
        string text = new('a', 300);

        Assert.IsTrue(InputTextDisplayPolicy.ShouldDisplay(text));
    }

    [TestMethod]
    public void ShouldDisplay_hides_text_over_300_characters()
    {
        string text = new('a', 301);

        Assert.IsFalse(InputTextDisplayPolicy.ShouldDisplay(text));
    }

    [TestMethod]
    public void ShouldDisplay_allows_text_at_5_lines()
    {
        string text = string.Join('\n', Enumerable.Range(0, 5).Select(_ => "line"));

        Assert.IsTrue(InputTextDisplayPolicy.ShouldDisplay(text));
    }

    [TestMethod]
    public void ShouldDisplay_hides_text_over_5_lines()
    {
        string text = string.Join('\n', Enumerable.Range(0, 6).Select(_ => "line"));

        Assert.IsFalse(InputTextDisplayPolicy.ShouldDisplay(text));
    }
}
