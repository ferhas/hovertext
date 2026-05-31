namespace HoverText.Core.Probing;

public static class ProbeTextNormalizer
{
    public static string NormalizeNonInput(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string[] parts = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var uniqueParts = new List<string>();
        foreach (string part in parts)
        {
            if (uniqueParts.Count == 0 || !string.Equals(uniqueParts[^1], part, StringComparison.Ordinal))
            {
                uniqueParts.Add(part);
            }
        }

        return string.Join(' ', uniqueParts);
    }
}
