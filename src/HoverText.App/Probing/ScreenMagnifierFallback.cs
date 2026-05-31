using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using HoverText.Core.Overlay;
using HoverText.Core.Probing;

namespace HoverText.App.Probing;

public sealed class ScreenMagnifierFallback : IMagnifierFallback
{
    private const int CaptureWidth = 240;
    private const int CaptureHeight = 140;

    public BitmapSource? LastCapture { get; private set; }

    public Task<MagnifierResult> CaptureAsync(PointerPoint point, CancellationToken cancellationToken = default)
    {
        try
        {
            int x = Math.Max(0, point.X - CaptureWidth / 2);
            int y = Math.Max(0, point.Y - CaptureHeight / 2);
            using var bitmap = new Bitmap(CaptureWidth, CaptureHeight, PixelFormat.Format32bppArgb);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(x, y, 0, 0, new Size(CaptureWidth, CaptureHeight));
            LastCapture = ToBitmapSource(bitmap);
            return Task.FromResult(MagnifierResult.Captured(new PixelRect(x, y, CaptureWidth, CaptureHeight), 2.0));
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
}
