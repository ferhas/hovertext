namespace HoverText.Core.Probing;

public enum ProbeDisplayKind
{
    Text,
    Tooltip,
    Input,
    Magnifier,
    Empty
}

public sealed record TextProbeResult(bool HasText, string Text, ProbeSource Source, ProbeDisplayKind DisplayKind)
{
    public static TextProbeResult Found(
        string text,
        ProbeSource source,
        ProbeDisplayKind displayKind = ProbeDisplayKind.Text)
    {
        return new TextProbeResult(true, text.Trim(), source, displayKind);
    }

    public static TextProbeResult None(ProbeSource source)
    {
        return new TextProbeResult(false, string.Empty, source, ProbeDisplayKind.Empty);
    }
}
