using System.Windows.Media.Imaging;
using HoverText.Core.Overlay;
using HoverText.Core.Probing;
using HoverText.Core.Settings;

namespace HoverText.App.Probing;

public sealed class NativeWindowsMagnifierBackend : IMagnifierBackend
{
    private readonly NativeMagnifierWindow window = new();
    private readonly ScreenMagnifierFallback fallback = new();
    private bool useFallback;

    public MagnifierBackendKind Kind => MagnifierBackendKind.NativeWindows;

    public BitmapSource? LastCapture => fallback.LastCapture;

    public bool UsesExternalWindow => !useFallback;

    public bool CanReuseCapture => false;

    public async Task<MagnifierResult> CaptureAsync(PointerPoint point, CancellationToken cancellationToken = default)
    {
        if (!window.EnsureReady(MagnifierDefaults.Scale))
        {
            useFallback = true;
            return await fallback.CaptureAsync(point, cancellationToken);
        }

        useFallback = false;
        PixelRect area = MagnifierCaptureArea.FromPointer(point);
        return MagnifierResult.Captured(area, MagnifierDefaults.Scale);
    }

    public void Dispose()
    {
        window.Dispose();
        fallback.Dispose();
    }

    public void ShowExternal(PointerPoint cursor, MagnifierResult result, HoverTextSettings settings)
    {
        window.Show(cursor, result.CaptureArea, result.Scale);
    }

    public void HideExternal()
    {
        window.Hide();
    }
}
