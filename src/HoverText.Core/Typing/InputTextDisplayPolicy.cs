namespace HoverText.Core.Typing;

public static class InputTextDisplayPolicy
{
    public const int MaxDisplayCharacters = 300;
    public const int MaxDisplayLines = 5;

    public static bool ShouldDisplay(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal);
        if (normalized.Length > MaxDisplayCharacters)
        {
            return false;
        }

        int lineCount = normalized.Count(character => character == '\n') + 1;
        return lineCount <= MaxDisplayLines;
    }
}
