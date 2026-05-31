namespace HoverText.Core.Typing;

public static class HoverTypingTextSanitizer
{
    private static readonly string[] PlaceholderTexts =
    [
        "要求后续变更",
        "Ask for follow-up changes",
        "Message Codex",
        "Message"
    ];

    public static string NormalizeForDisplay(
        string text,
        string className,
        IReadOnlyCollection<string> descendantTextNames)
    {
        string normalized = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Trim('\r', '\n');

        if (HasClassToken(className, "ProseMirror")
            && IsKnownPlaceholderEcho(normalized, descendantTextNames))
        {
            return string.Empty;
        }

        return normalized;
    }

    private static bool IsKnownPlaceholderEcho(string text, IReadOnlyCollection<string> descendantTextNames)
    {
        if (!PlaceholderTexts.Any(placeholder => string.Equals(text, placeholder, StringComparison.Ordinal)))
        {
            return false;
        }

        return descendantTextNames.Any(name => string.Equals(name, text, StringComparison.Ordinal));
    }

    private static bool HasClassToken(string className, string token)
    {
        return className
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Contains(token, StringComparer.Ordinal);
    }
}
