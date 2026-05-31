namespace HoverText.Core.Probing;

public readonly record struct ProbeTextSelection(string? Text, ProbeDisplayKind DisplayKind)
{
    public static ProbeTextSelection ForText(string? text)
    {
        return new ProbeTextSelection(text, ProbeDisplayKind.Text);
    }

    public static ProbeTextSelection ForActionLike(string? name, string? value, string? helpText)
    {
        string? automationText = FirstNonBlank(name, value);
        return !string.IsNullOrWhiteSpace(automationText)
            ? new ProbeTextSelection(automationText, ProbeDisplayKind.Text)
            : new ProbeTextSelection(helpText, ProbeDisplayKind.Tooltip);
    }

    private static string? FirstNonBlank(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
