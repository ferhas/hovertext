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
        Assert.AreEqual(40, settings.FontSize);
        Assert.AreEqual(150, settings.PollIntervalMilliseconds);
        Assert.IsTrue(settings.IsOcrEnabled);
        Assert.IsFalse(settings.StartWithWindows);
    }

    [TestMethod]
    public async Task JsonSettingsStore_round_trips_user_preferences()
    {
        string path = Path.Combine(Path.GetTempPath(), $"hovertext-{Guid.NewGuid():N}.json");
        var store = new JsonSettingsStore(path);
        var original = HoverTextSettings.CreateDefault() with
        {
            FontSize = 48,
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
}
