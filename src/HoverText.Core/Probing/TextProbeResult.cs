using HoverText.Core.Overlay;

namespace HoverText.Core.Probing;

public enum ProbeDisplayKind
{
    Text,
    Tooltip,
    Magnifier,
    Empty
}

public sealed record TextProbeResult(
    bool HasText,
    string Text,
    ProbeSource Source,
    ProbeDisplayKind DisplayKind,
    PixelRect? AnchorBounds = null)
{
    public static TextProbeResult Found(
        string text,
        ProbeSource source,
        ProbeDisplayKind displayKind = ProbeDisplayKind.Text,
        PixelRect? anchorBounds = null)
    {
        string displayText = text.Trim();
        return new TextProbeResult(true, displayText, source, displayKind, anchorBounds);
    }

    public static TextProbeResult None(ProbeSource source)
    {
        return new TextProbeResult(false, string.Empty, source, ProbeDisplayKind.Empty);
    }
}
