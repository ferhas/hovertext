using HoverText.Core.Overlay;
using HoverText.Core.Probing;

namespace HoverText.Tests;

[TestClass]
public sealed class OverlayTextSizingTests
{
    [TestMethod]
    public void CalculateFontSize_keeps_short_text_at_configured_size()
    {
        double fontSize = OverlayTextSizing.CalculateFontSize(56, "设置", ProbeDisplayKind.Text);

        Assert.AreEqual(56, fontSize);
    }

    [TestMethod]
    public void CalculateFontSize_shrinks_long_text_within_adaptive_range()
    {
        double fontSize = OverlayTextSizing.CalculateFontSize(
            56,
            "扶绥县就业信息化平台报价表0324.xlsx",
            ProbeDisplayKind.Text);

        Assert.IsTrue(fontSize < 56);
        Assert.IsTrue(fontSize >= 48);
    }

    [TestMethod]
    public void CalculateFontSize_does_not_shrink_ordinary_text_below_48()
    {
        double fontSize = OverlayTextSizing.CalculateFontSize(
            56,
            string.Concat(Enumerable.Repeat("扶绥县就业信息化平台报价表0324.xlsx", 4)),
            ProbeDisplayKind.Text);

        Assert.AreEqual(48, fontSize);
    }

    [TestMethod]
    public void CalculateFontSize_caps_ordinary_text_at_56()
    {
        double fontSize = OverlayTextSizing.CalculateFontSize(72, "设置", ProbeDisplayKind.Text);

        Assert.AreEqual(56, fontSize);
    }

    [TestMethod]
    public void CalculateFontSize_keeps_input_loupe_large()
    {
        double fontSize = OverlayTextSizing.CalculateFontSize(40, "正在输入", ProbeDisplayKind.Input);

        Assert.AreEqual(76, fontSize);
    }
}
