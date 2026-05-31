using System.IO;

namespace HoverText.App;

public static class AppPaths
{
    public static string SettingsFilePath
    {
        get
        {
            string directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "HoverText");
            return Path.Combine(directory, "settings.json");
        }
    }
}
