using System.Runtime.InteropServices;
using Forms = System.Windows.Forms;
using HoverText.Core.Overlay;
using HoverText.Core.Platform;

namespace HoverText.App.Probing;

internal sealed class NativeMagnifierWindow : IDisposable
{
    private const string MagnifierWindowClass = "Magnifier";
    private const string StaticWindowClass = "Static";
    private const int WsPopup = unchecked((int)0x80000000);
    private const int WsChild = 0x40000000;
    private const int WsVisible = 0x10000000;
    private const int WsBorder = 0x00800000;
    private const int WsExTopmost = 0x00000008;
    private const int WsExTransparent = 0x00000020;
    private const int WsExToolWindow = 0x00000080;
    private const int WsExNoActivate = 0x08000000;
    private const int SwHide = 0;
    private const int SwShowNoActivate = 4;
    private const uint SwpNoActivate = 0x0010;
    private const uint SwpShowWindow = 0x0040;
    private const int MsShowMagnifiedCursor = 0x0001;

    private IntPtr parentHandle;
    private IntPtr magnifierHandle;
    private bool initialized;

    public bool EnsureReady(double scale)
    {
        if (!EnsureCreated(scale))
        {
            return false;
        }

        return true;
    }

    public void Show(PointerPoint cursor, PixelRect source, double scale)
    {
        if (!EnsureReady(scale))
        {
            return;
        }

        var displaySize = new PixelSize(
            (int)Math.Round(source.Width * scale),
            (int)Math.Round(source.Height * scale));
        PixelRect placement = PlaceNearCursor(cursor, displaySize);

        SetWindowPos(parentHandle, IntPtr.Zero, placement.Left, placement.Top, placement.Width, placement.Height, SwpNoActivate | SwpShowWindow);
        SetWindowPos(magnifierHandle, IntPtr.Zero, 0, 0, placement.Width, placement.Height, SwpNoActivate | SwpShowWindow);
        MagSetWindowSource(magnifierHandle, new NativeRect(source.Left, source.Top, source.Right, source.Bottom));
        ShowWindow(parentHandle, SwShowNoActivate);
        InvalidateRect(magnifierHandle, IntPtr.Zero, true);
    }

    public void Hide()
    {
        if (parentHandle != IntPtr.Zero)
        {
            ShowWindow(parentHandle, SwHide);
        }
    }

    public void Dispose()
    {
        if (magnifierHandle != IntPtr.Zero)
        {
            DestroyWindow(magnifierHandle);
            magnifierHandle = IntPtr.Zero;
        }

        if (parentHandle != IntPtr.Zero)
        {
            DestroyWindow(parentHandle);
            parentHandle = IntPtr.Zero;
        }

        if (initialized)
        {
            MagUninitialize();
            initialized = false;
        }
    }

    private bool EnsureCreated(double scale)
    {
        if (parentHandle != IntPtr.Zero && magnifierHandle != IntPtr.Zero)
        {
            return true;
        }

        if (!initialized)
        {
            initialized = MagInitialize();
            if (!initialized)
            {
                return false;
            }
        }

        parentHandle = CreateWindowEx(
            WsExTopmost | WsExToolWindow | WsExNoActivate | WsExTransparent,
            StaticWindowClass,
            "HoverText Native Magnifier",
            WsPopup | WsBorder,
            0,
            0,
            1,
            1,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);
        if (parentHandle == IntPtr.Zero)
        {
            return false;
        }

        _ = SetWindowDisplayAffinity(parentHandle, WindowCaptureAffinity.ExcludeFromCapture)
            || SetWindowDisplayAffinity(parentHandle, WindowCaptureAffinity.MonitorOnly);

        magnifierHandle = CreateWindowEx(
            0,
            MagnifierWindowClass,
            "HoverText Magnifier Control",
            WsChild | WsVisible | MsShowMagnifiedCursor,
            0,
            0,
            1,
            1,
            parentHandle,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);
        if (magnifierHandle == IntPtr.Zero)
        {
            DestroyWindow(parentHandle);
            parentHandle = IntPtr.Zero;
            return false;
        }

        var transform = NativeMagTransform.Identity(scale);
        return MagSetWindowTransform(magnifierHandle, ref transform);
    }

    private static PixelRect PlaceNearCursor(PointerPoint cursor, PixelSize displaySize)
    {
        Forms.Screen screen = Forms.Screen.FromPoint(new System.Drawing.Point(cursor.X, cursor.Y));
        System.Drawing.Rectangle area = screen.WorkingArea;
        var workArea = new PixelRect(area.Left, area.Top, area.Width, area.Height);
        return OverlayPlacement.PlaceNearCursor(cursor, displaySize, workArea, margin: 8, offset: 18);
    }

    [DllImport("Magnification.dll", ExactSpelling = true)]
    private static extern bool MagInitialize();

    [DllImport("Magnification.dll", ExactSpelling = true)]
    private static extern bool MagUninitialize();

    [DllImport("Magnification.dll", ExactSpelling = true)]
    private static extern bool MagSetWindowSource(IntPtr hwnd, NativeRect rect);

    [DllImport("Magnification.dll", ExactSpelling = true)]
    private static extern bool MagSetWindowTransform(IntPtr hwnd, ref NativeMagTransform transform);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowEx(
        int extendedStyle,
        string className,
        string windowName,
        int style,
        int x,
        int y,
        int width,
        int height,
        IntPtr parent,
        IntPtr menu,
        IntPtr instance,
        IntPtr param);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyWindow(IntPtr windowHandle);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ShowWindow(IntPtr windowHandle, int command);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr windowHandle,
        IntPtr windowHandleInsertAfter,
        int x,
        int y,
        int width,
        int height,
        uint flags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool InvalidateRect(IntPtr windowHandle, IntPtr rect, bool erase);

    [DllImport("user32.dll")]
    private static extern bool SetWindowDisplayAffinity(IntPtr windowHandle, WindowCaptureAffinity affinity);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct NativeRect(int left, int top, int right, int bottom)
    {
        public readonly int Left = left;
        public readonly int Top = top;
        public readonly int Right = right;
        public readonly int Bottom = bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeMagTransform
    {
        private float m00;
        private float m01;
        private float m02;
        private float m10;
        private float m11;
        private float m12;
        private float m20;
        private float m21;
        private float m22;

        public static NativeMagTransform Identity(double scale)
        {
            return new NativeMagTransform
            {
                m00 = (float)scale,
                m11 = (float)scale,
                m22 = 1.0f
            };
        }
    }
}
