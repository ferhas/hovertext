namespace HoverText.Core.Probing;

public sealed record ProbeResult(
    ProbeSource Source,
    string DisplayText,
    MagnifierResult? Magnifier,
    ProbeDisplayKind DisplayKind)
{
    public static ProbeResult FromText(TextProbeResult result)
    {
        return new ProbeResult(result.Source, result.Text, null, result.DisplayKind);
    }

    public static ProbeResult FromMagnifier(MagnifierResult result)
    {
        return new ProbeResult(ProbeSource.Magnifier, string.Empty, result, ProbeDisplayKind.Magnifier);
    }

    public static ProbeResult Empty()
    {
        return new ProbeResult(ProbeSource.None, string.Empty, null, ProbeDisplayKind.Empty);
    }
}
