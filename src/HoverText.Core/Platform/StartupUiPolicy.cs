namespace HoverText.Core.Platform;

public sealed record StartupUiPolicy(
    bool ShowTaskbarControlWindow,
    bool ControlWindowCloseExitsApplication,
    bool HideSettingsWindowOnMinimize)
{
    public static StartupUiPolicy CreateDefault()
    {
        return new StartupUiPolicy(
            ShowTaskbarControlWindow: false,
            ControlWindowCloseExitsApplication: true,
            HideSettingsWindowOnMinimize: true);
    }
}
