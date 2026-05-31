using System.Runtime.InteropServices;
using HoverText.Core.Overlay;

namespace HoverText.App.Input;

public sealed class CursorPositionProvider
{
    public PointerPoint GetCursorPosition()
    {
        return GetCursorPos(out NativePoint point)
            ? new PointerPoint(point.X, point.Y)
            : new PointerPoint(0, 0);
    }

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out NativePoint point);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct NativePoint
    {
        public readonly int X;
        public readonly int Y;
    }
}
