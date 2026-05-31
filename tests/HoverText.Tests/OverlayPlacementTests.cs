using HoverText.Core.Overlay;

namespace HoverText.Tests;

[TestClass]
public sealed class OverlayPlacementTests
{
    [TestMethod]
    public void PlaceNearCursor_clamps_overlay_inside_work_area()
    {
        var cursor = new PointerPoint(1910, 1070);
        var overlaySize = new PixelSize(320, 140);
        var workArea = new PixelRect(0, 0, 1920, 1080);

        PixelRect placement = OverlayPlacement.PlaceNearCursor(cursor, overlaySize, workArea, margin: 8, offset: 16);

        Assert.IsTrue(placement.Left >= 8, "Overlay should not run past the left screen edge.");
        Assert.IsTrue(placement.Top >= 8, "Overlay should not run past the top screen edge.");
        Assert.IsTrue(placement.Right <= 1912, "Overlay should not run past the right screen edge.");
        Assert.IsTrue(placement.Bottom <= 1072, "Overlay should not run past the bottom screen edge.");
    }

    [TestMethod]
    public void PlaceNearCursor_prefers_lower_right_when_there_is_room()
    {
        var cursor = new PointerPoint(100, 120);
        var overlaySize = new PixelSize(260, 96);
        var workArea = new PixelRect(0, 0, 1920, 1080);

        PixelRect placement = OverlayPlacement.PlaceNearCursor(cursor, overlaySize, workArea, margin: 8, offset: 16);

        Assert.AreEqual(116, placement.Left);
        Assert.AreEqual(136, placement.Top);
    }
}
