using System.Runtime.InteropServices;
using HoverText.Core.Settings;

namespace HoverText.App.Input;

public sealed class KeyboardTriggerReader
{
    private const int VkShift = 0x10;
    private const int VkControl = 0x11;
    private const int VkMenu = 0x12;
    private const int VkLeftShift = 0xA0;
    private const int VkRightShift = 0xA1;
    private const int VkLeftControl = 0xA2;
    private const int VkRightControl = 0xA3;
    private const int VkLeftMenu = 0xA4;
    private const int VkRightMenu = 0xA5;
    private readonly Func<int, short> getKeyState;

    private static readonly int[] TypingVirtualKeys =
    [
        0x08, // Backspace
        0x09, // Tab
        0x0D, // Enter
        0x20, // Space
        0x2E, // Delete
        0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39,
        0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A,
        0x4B, 0x4C, 0x4D, 0x4E, 0x4F, 0x50, 0x51, 0x52, 0x53, 0x54,
        0x55, 0x56, 0x57, 0x58, 0x59, 0x5A,
        0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF, 0xC0, 0xDB, 0xDC, 0xDD, 0xDE
    ];

    public KeyboardTriggerReader()
        : this(GetAsyncKeyState)
    {
    }

    internal KeyboardTriggerReader(Func<int, short> getKeyState)
    {
        this.getKeyState = getKeyState;
    }

    public bool IsPressed(TriggerKey triggerKey)
    {
        (int leftKey, int rightKey) = triggerKey switch
        {
            TriggerKey.Control => (VkLeftControl, VkRightControl),
            TriggerKey.Shift => (VkLeftShift, VkRightShift),
            _ => (VkLeftMenu, VkRightMenu)
        };

        return IsKeyDown(leftKey) || IsKeyDown(rightKey);
    }

    public bool HasTypingActivity()
    {
        foreach (int virtualKey in TypingVirtualKeys)
        {
            if ((getKeyState(virtualKey) & 0x0001) != 0)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsKeyDown(int virtualKey)
    {
        return (getKeyState(virtualKey) & 0x8000) != 0;
    }

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int virtualKeyCode);
}
