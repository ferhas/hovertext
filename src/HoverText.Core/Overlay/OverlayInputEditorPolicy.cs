using HoverText.Core.Probing;

namespace HoverText.Core.Overlay;

public static class OverlayInputEditorPolicy
{
    public static string PrepareText(string? text, ProbeDisplayKind displayKind)
    {
        return displayKind == ProbeDisplayKind.Input
            ? text ?? string.Empty
            : text ?? string.Empty;
    }

    public static bool ShouldReplaceEditorText(
        ProbeDisplayKind displayKind,
        bool wasInputMode,
        bool isEditorFocused,
        string currentText,
        string incomingText)
    {
        return displayKind == ProbeDisplayKind.Input
            && (!isEditorFocused || !wasInputMode)
            && !string.Equals(currentText, incomingText, StringComparison.Ordinal);
    }

    public static bool ShouldFocusEditor(
        ProbeDisplayKind displayKind,
        bool wasInputMode,
        bool isEditorFocused)
    {
        return displayKind == ProbeDisplayKind.Input
            && (!wasInputMode || !isEditorFocused);
    }

    public static bool ShouldKeepOpenWithoutTrigger(bool isEditorOpen)
    {
        return isEditorOpen;
    }
}
