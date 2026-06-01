using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using HoverText.Core.Overlay;
using HoverText.Core.Probing;
using HoverText.Core.Settings;

namespace HoverText.App.Probing;

public sealed class ScreenMagnifierFallback : IMagnifierBackend
{
    public BitmapSource? LastCapture { get; private set; }

    public MagnifierBackendKind Kind => MagnifierBackendKind.GdiBitmap;

    public bool UsesExternalWindow => false;

    public bool CanReuseCapture => true;

    public Task<MagnifierResult> CaptureAsync(PointerPoint point, CancellationToken cancellationToken = default)
    {
        try
        {
            PixelRect captureArea = MagnifierCaptureArea.FromPointer(point);
            using var bitmap = new Bitmap(captureArea.Width, captureArea.Height, PixelFormat.Format32bppArgb);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(captureArea.Left, captureArea.Top, 0, 0, new Size(captureArea.Width, captureArea.Height));
            LastCapture = ToBitmapSource(bitmap);
            return Task.FromResult(MagnifierResult.Captured(captureArea, MagnifierDefaults.Scale));
        }
        catch
        {
            LastCapture = null;
            return Task.FromResult(MagnifierResult.None());
        }
    }

    private static BitmapSource ToBitmapSource(Bitmap bitmap)
    {
        IntPtr handle = bitmap.GetHbitmap();
        try
        {
            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(
                handle,
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            return source;
        }
        finally
        {
            DeleteObject(handle);
        }
    }

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr objectHandle);

    public void Dispose()
    {
        LastCapture = null;
    }

    public void ShowExternal(PointerPoint cursor, MagnifierResult result, HoverTextSettings settings)
    {
    }

    public void HideExternal()
    {
    }
}
