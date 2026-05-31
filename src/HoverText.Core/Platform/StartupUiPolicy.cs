namespace HoverText.Core.Platform;

public sealed record StartupUiPolicy(bool ShowTaskbarControlWindow, bool ControlWindowCloseExitsApplication)
{
    public static StartupUiPolicy CreateDefault()
    {
        return new StartupUiPolicy(
            ShowTaskbarControlWindow: false,
            ControlWindowCloseExitsApplication: true);
    }
}
