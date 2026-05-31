using HoverText.Core.Settings;

namespace HoverText.Core.Typing;

public sealed class HoverTypingSession
{
    private string? suppressedFocusKey;
    private string? suppressedText;

    public HoverTypingDisplay Evaluate(
        HoverTextSettings settings,
        HoverTypingSnapshot snapshot,
        bool escapePressed)
    {
        if (!settings.IsHoverTypingEnabled || !snapshot.HasTextEntry || string.IsNullOrEmpty(snapshot.FocusKey))
        {
            ClearSuppression();
            return HoverTypingDisplay.Hidden();
        }

        string text = snapshot.Text.TrimEnd('\r', '\n');
        if (string.IsNullOrWhiteSpace(text))
        {
            if (string.Equals(suppressedFocusKey, snapshot.FocusKey, StringComparison.Ordinal)
                && !string.Equals(suppressedText, text, StringComparison.Ordinal))
            {
                ClearSuppression();
            }

            return HoverTypingDisplay.Hidden();
        }

        if (escapePressed)
        {
            suppressedFocusKey = snapshot.FocusKey;
            suppressedText = text;
            return HoverTypingDisplay.Hidden();
        }

        if (string.Equals(suppressedFocusKey, snapshot.FocusKey, StringComparison.Ordinal)
            && string.Equals(suppressedText, text, StringComparison.Ordinal))
        {
            return HoverTypingDisplay.Hidden();
        }

        ClearSuppression();
        return HoverTypingDisplay.Visible(text, snapshot.AnchorBounds);
    }

    private void ClearSuppression()
    {
        suppressedFocusKey = null;
        suppressedText = null;
    }
}
