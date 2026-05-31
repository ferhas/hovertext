using HoverText.Core.Probing;
using HoverText.Core.Settings;

namespace HoverText.Core.Overlay;

public readonly record struct OverlayLayoutFingerprint(
    ProbeSource Source,
    ProbeDisplayKind DisplayKind,
    string DisplayText,
    int FontSize,
    string Foreground,
    string Background,
    PixelRect? AnchorBounds,
    PixelRect? MagnifierArea,
    double MagnifierScale)
{
    public static OverlayLayoutFingerprint From(ProbeResult result, HoverTextSettings settings)
    {
        return new OverlayLayoutFingerprint(
            result.Source,
            result.DisplayKind,
            result.DisplayText,
            settings.FontSize,
            settings.Foreground,
            settings.Background,
            result.AnchorBounds,
            result.Magnifier?.CaptureArea,
            result.Magnifier?.Scale ?? 0);
    }
}
