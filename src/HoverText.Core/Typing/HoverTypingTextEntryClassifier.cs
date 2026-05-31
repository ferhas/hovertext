namespace HoverText.Core.Typing;

public static class HoverTypingTextEntryClassifier
{
    public static bool IsEditableTextEntry(
        string controlTypeName,
        string className,
        bool isKeyboardFocusable,
        bool isEnabled,
        bool supportsValuePattern,
        bool supportsTextPattern)
    {
        if (!isEnabled)
        {
            return false;
        }

        if (controlTypeName is "ControlType.Edit" or "ControlType.Document")
        {
            return true;
        }

        if (controlTypeName == "ControlType.Group"
            && HasClassToken(className, "ProseMirror")
            && isKeyboardFocusable
            && supportsTextPattern)
        {
            return true;
        }

        return controlTypeName == "ControlType.Text"
            && isKeyboardFocusable
            && (supportsValuePattern || supportsTextPattern);
    }

    private static bool HasClassToken(string className, string token)
    {
        return className
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Contains(token, StringComparer.Ordinal);
    }
}
