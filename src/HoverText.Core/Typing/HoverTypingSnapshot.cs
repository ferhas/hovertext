using HoverText.Core.Overlay;

namespace HoverText.Core.Typing;

public sealed record HoverTypingSnapshot(
    bool HasTextEntry,
    string FocusKey,
    string Text,
    PixelRect? AnchorBounds)
{
    public static HoverTypingSnapshot FromTextEntry(string focusKey, string text, PixelRect? anchorBounds)
    {
        return new HoverTypingSnapshot(true, focusKey, text, anchorBounds);
    }

    public static HoverTypingSnapshot None()
    {
        return new HoverTypingSnapshot(false, string.Empty, string.Empty, null);
    }
}
