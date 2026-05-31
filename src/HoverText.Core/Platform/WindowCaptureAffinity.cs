namespace HoverText.Core.Platform;

public enum WindowCaptureAffinity : uint
{
    None = 0,
    MonitorOnly = 0x00000001,
    ExcludeFromCapture = 0x00000011
}
