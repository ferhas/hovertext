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
    private const int VkEscape = 0x1B;
    private readonly Func<int, short> getKeyState;

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
        return triggerKey switch
        {
            TriggerKey.Alt => IsKeyDown(VkLeftMenu),
            TriggerKey.Control => IsKeyDown(VkLeftControl) || IsKeyDown(VkRightControl),
            TriggerKey.Shift => IsKeyDown(VkLeftShift) || IsKeyDown(VkRightShift),
            _ => false
        };
    }

    public bool IsMagnifierPressed()
    {
        return IsKeyDown(VkRightMenu);
    }

    public bool IsEscapePressed()
    {
        short state = getKeyState(VkEscape);
        return (state & 0x8000) != 0 || (state & 0x0001) != 0;
    }

    private bool IsKeyDown(int virtualKey)
    {
        return (getKeyState(virtualKey) & 0x8000) != 0;
    }

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int virtualKeyCode);
}
