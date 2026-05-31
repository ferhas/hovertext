using HoverText.Core.Overlay;
using HoverText.Core.Probing;
using HoverText.Core.Settings;

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
    public void CalculateFontSize_shrinks_long_text_without_going_below_minimum()
    {
        double fontSize = OverlayTextSizing.CalculateFontSize(
            56,
            "扶绥县就业信息化平台报价表0324.xlsx",
            ProbeDisplayKind.Text);

        Assert.IsTrue(fontSize < 56);
        Assert.IsTrue(fontSize >= HoverTextSettings.MinimumFontSize);
    }

    [TestMethod]
    public void CalculateFontSize_keeps_input_loupe_large()
    {
        double fontSize = OverlayTextSizing.CalculateFontSize(40, "正在输入", ProbeDisplayKind.Input);

        Assert.AreEqual(76, fontSize);
    }
}
