namespace HoverText.Core.Settings;

public enum TriggerKey
{
    Alt,
    Control,
    Shift
}

public sealed record HoverTextSettings
{
    public TriggerKey TriggerKey { get; init; }

    public int FontSize { get; init; }

    public string Foreground { get; init; } = "";

    public string Background { get; init; } = "";

    public int PollIntervalMilliseconds { get; init; }

    public bool IsOcrEnabled { get; init; }

    public bool StartWithWindows { get; init; }

    public static HoverTextSettings CreateDefault()
    {
        return new HoverTextSettings
        {
            TriggerKey = TriggerKey.Alt,
            FontSize = 56,
            Foreground = "#f8fafc",
            Background = "#111827",
            PollIntervalMilliseconds = 150,
            IsOcrEnabled = true,
            StartWithWindows = false
        };
    }
}
