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
            GetPropertyNames(json));
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

    private static HoverTextSettings Normalize(HoverTextSettings settings, IReadOnlySet<string> propertyNames)
    {
        HoverTextSettings defaults = HoverTextSettings.CreateDefault();
        if (settings.FontSize < HoverTextSettings.MinimumFontSize)
        {
            settings = settings with { FontSize = HoverTextSettings.MinimumFontSize };
        }

        if (!propertyNames.Contains(nameof(HoverTextSettings.IsMagnifierEnabled)))
        {
            settings = settings with { IsMagnifierEnabled = defaults.IsMagnifierEnabled };
        }

        if (!propertyNames.Contains(nameof(HoverTextSettings.MagnifierBackend))
            || !Enum.IsDefined(settings.MagnifierBackend))
        {
            settings = settings with { MagnifierBackend = defaults.MagnifierBackend };
        }

        return settings;
    }

    private static HashSet<string> GetPropertyNames(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            return [];
        }

        return document.RootElement
            .EnumerateObject()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);
    }
}
