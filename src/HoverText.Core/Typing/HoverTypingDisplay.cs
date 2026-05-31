using HoverText.Core.Overlay;

namespace HoverText.Core.Typing;

public sealed record HoverTypingDisplay(bool IsVisible, string Text, PixelRect? AnchorBounds)
{
    public static HoverTypingDisplay Visible(string text, PixelRect? anchorBounds)
    {
        return new HoverTypingDisplay(true, text, anchorBounds);
    }

    public static HoverTypingDisplay Hidden()
    {
        return new HoverTypingDisplay(false, string.Empty, null);
    }
}
