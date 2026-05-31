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

        await using FileStream stream = File.OpenRead(path);
        HoverTextSettings? settings = await JsonSerializer.DeserializeAsync<HoverTextSettings>(stream, Options, cancellationToken);
        return Normalize(settings ?? HoverTextSettings.CreateDefault());
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

    private static HoverTextSettings Normalize(HoverTextSettings settings)
    {
        HoverTextSettings defaults = HoverTextSettings.CreateDefault();
        return settings.FontSize < defaults.FontSize
            ? settings with { FontSize = defaults.FontSize }
            : settings;
    }
}
