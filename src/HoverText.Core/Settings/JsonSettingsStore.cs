using System.Text.Json;

namespace HoverText.Core.Settings;

public sealed class JsonSettingsStore(string path)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public async Task<HoverTextSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
        {
            return HoverTextSettings.CreateDefault();
        }

        string json = await File.ReadAllTextAsync(path, cancellationToken);
        HoverTextSettings? settings = JsonSerializer.Deserialize<HoverTextSettings>(json, Options);
        return Normalize(
            settings ?? HoverTextSettings.CreateDefault(),
            HasProperty(json, nameof(HoverTextSettings.IsMagnifierEnabled)));
    }

    public async Task SaveAsync(HoverTextSettings settings, CancellationToken cancellationToken = default)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using FileStream stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, settings, Options, cancellationToken);
    }

    private static HoverTextSettings Normalize(HoverTextSettings settings, bool hasMagnifierSetting)
    {
        HoverTextSettings defaults = HoverTextSettings.CreateDefault();
        if (settings.FontSize < defaults.FontSize)
        {
            settings = settings with { FontSize = defaults.FontSize };
        }

        return hasMagnifierSetting
            ? settings
            : settings with { IsMagnifierEnabled = defaults.IsMagnifierEnabled };
    }

    private static bool HasProperty(string json, string propertyName)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.ValueKind == JsonValueKind.Object
            && document.RootElement.TryGetProperty(propertyName, out _);
    }
}
