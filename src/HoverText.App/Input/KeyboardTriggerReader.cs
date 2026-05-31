using System.Runtime.InteropServices;
using HoverText.Core.Settings;

namespace HoverText.App.Input;

public sealed class KeyboardTriggerReader
{
    public bool IsPressed(TriggerKey triggerKey)
    {
        int virtualKey = triggerKey switch
        {
            TriggerKey.Control => 0x11,
            TriggerKey.Shift => 0x10,
            _ => 0x12
        };

        return (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
    }

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int virtualKeyCode);
}
