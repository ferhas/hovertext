using HoverText.Core.Overlay;
using HoverText.Core.Probing;
using HoverText.Core.Settings;

namespace HoverText.Tests;

[TestClass]
public sealed class OverlayLayoutFingerprintTests
{
    [TestMethod]
    public void From_ignores_cursor_position_for_same_content()
    {
        HoverTextSettings settings = HoverTextSettings.CreateDefault();
        ProbeResult result = ProbeResult.FromText(TextProbeResult.Found("HoverText", ProbeSource.UiAutomation));

        OverlayLayoutFingerprint first = OverlayLayoutFingerprint.From(result, settings);
        OverlayLayoutFingerprint second = OverlayLayoutFingerprint.From(result, settings);

        Assert.AreEqual(first, second);
    }

    [TestMethod]
    public void From_changes_when_font_size_changes()
    {
        ProbeResult result = ProbeResult.FromText(TextProbeResult.Found("HoverText", ProbeSource.UiAutomation));

        OverlayLayoutFingerprint first = OverlayLayoutFingerprint.From(result, HoverTextSettings.CreateDefault() with { FontSize = 40 });
        OverlayLayoutFingerprint second = OverlayLayoutFingerprint.From(result, HoverTextSettings.CreateDefault() with { FontSize = 56 });

        Assert.AreNotEqual(first, second);
    }

    [TestMethod]
    public void From_changes_when_magnifier_geometry_changes()
    {
        HoverTextSettings settings = HoverTextSettings.CreateDefault();
        ProbeResult firstResult = ProbeResult.FromMagnifier(
            MagnifierResult.Captured(new PixelRect(10, 10, 120, 80), 3.0));
        ProbeResult secondResult = ProbeResult.FromMagnifier(
            MagnifierResult.Captured(new PixelRect(10, 10, 160, 80), 3.0));

        OverlayLayoutFingerprint first = OverlayLayoutFingerprint.From(firstResult, settings);
        OverlayLayoutFingerprint second = OverlayLayoutFingerprint.From(secondResult, settings);

        Assert.AreNotEqual(first, second);
    }
}
