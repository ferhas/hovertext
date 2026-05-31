using HoverText.Core.Settings;

namespace HoverText.Tests;

[TestClass]
public sealed class SettingsTests
{
    [TestMethod]
    public void Default_settings_match_mvp_plan()
    {
        HoverTextSettings settings = HoverTextSettings.CreateDefault();

        Assert.AreEqual(TriggerKey.Alt, settings.TriggerKey);
        Assert.AreEqual(56, settings.FontSize);
        Assert.AreEqual(150, settings.PollIntervalMilliseconds);
        Assert.IsTrue(settings.IsOcrEnabled);
        Assert.IsTrue(settings.IsMagnifierEnabled);
        Assert.IsFalse(settings.StartWithWindows);
    }

    [TestMethod]
    public async Task JsonSettingsStore_round_trips_user_preferences()
    {
        string path = Path.Combine(Path.GetTempPath(), $"hovertext-{Guid.NewGuid():N}.json");
        var store = new JsonSettingsStore(path);
        var original = HoverTextSettings.CreateDefault() with
        {
            FontSize = 72,
            Foreground = "#fff4b8",
            Background = "#1a1f2b",
            TriggerKey = TriggerKey.Control,
            IsOcrEnabled = false,
            StartWithWindows = true,
            PollIntervalMilliseconds = 100
        };

        try
        {
            await store.SaveAsync(original);
            HoverTextSettings loaded = await store.LoadAsync();

            Assert.AreEqual(original, loaded);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [TestMethod]
    public async Task JsonSettingsStore_upgrades_legacy_small_font_size()
    {
        string path = Path.Combine(Path.GetTempPath(), $"hovertext-{Guid.NewGuid():N}.json");
        var store = new JsonSettingsStore(path);
        var legacy = HoverTextSettings.CreateDefault() with { FontSize = 40 };

        try
        {
            await store.SaveAsync(legacy);
            HoverTextSettings loaded = await store.LoadAsync();

            Assert.AreEqual(56, loaded.FontSize);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [TestMethod]
    public async Task JsonSettingsStore_enables_magnifier_for_legacy_settings_without_property()
    {
        string path = Path.Combine(Path.GetTempPath(), $"hovertext-{Guid.NewGuid():N}.json");
        const string legacyJson = """
            {
              "TriggerKey": 0,
              "FontSize": 56,
              "Foreground": "#f8fafc",
              "Background": "#111827",
              "PollIntervalMilliseconds": 150,
              "IsOcrEnabled": true,
              "StartWithWindows": false
            }
            """;
        var store = new JsonSettingsStore(path);

        try
        {
            await File.WriteAllTextAsync(path, legacyJson);
            HoverTextSettings loaded = await store.LoadAsync();

            Assert.IsTrue(loaded.IsMagnifierEnabled);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [TestMethod]
    public async Task JsonSettingsStore_preserves_disabled_magnifier_setting()
    {
        string path = Path.Combine(Path.GetTempPath(), $"hovertext-{Guid.NewGuid():N}.json");
        const string json = """
            {
              "TriggerKey": 0,
              "FontSize": 56,
              "Foreground": "#f8fafc",
              "Background": "#111827",
              "PollIntervalMilliseconds": 150,
              "IsOcrEnabled": true,
              "IsMagnifierEnabled": false,
              "StartWithWindows": false
            }
            """;
        var store = new JsonSettingsStore(path);

        try
        {
            await File.WriteAllTextAsync(path, json);
            HoverTextSettings loaded = await store.LoadAsync();

            Assert.IsFalse(loaded.IsMagnifierEnabled);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
