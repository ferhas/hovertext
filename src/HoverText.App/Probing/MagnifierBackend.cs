using System.Windows.Media.Imaging;
using HoverText.Core.Overlay;
using HoverText.Core.Probing;
using HoverText.Core.Settings;

namespace HoverText.App.Probing;

public interface IMagnifierBackend : IMagnifierFallback, IDisposable
{
    MagnifierBackendKind Kind { get; }

    BitmapSource? LastCapture { get; }

    bool UsesExternalWindow { get; }

    bool CanReuseCapture { get; }

    void ShowExternal(PointerPoint cursor, MagnifierResult result, HoverTextSettings settings);

    void HideExternal();
}

public static class MagnifierBackendFactory
{
    public static IMagnifierBackend Create(HoverTextSettings settings)
    {
        return settings.MagnifierBackend switch
        {
            MagnifierBackendKind.NativeWindows => new NativeWindowsMagnifierBackend(),
            MagnifierBackendKind.GpuDesktopDuplication => new GpuDesktopDuplicationMagnifierBackend(),
            _ => new ScreenMagnifierFallback()
        };
    }
}
