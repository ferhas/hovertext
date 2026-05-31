using Microsoft.Win32;

namespace HoverText.App.Platform;

public sealed class StartupManager(string applicationName)
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public void SetEnabled(bool enabled)
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        if (key is null)
        {
            return;
        }

        if (enabled)
        {
            string executablePath = Environment.ProcessPath ?? "";
            if (!string.IsNullOrWhiteSpace(executablePath))
            {
                key.SetValue(applicationName, $"\"{executablePath}\"");
            }
        }
        else
        {
            key.DeleteValue(applicationName, throwOnMissingValue: false);
        }
    }
}
