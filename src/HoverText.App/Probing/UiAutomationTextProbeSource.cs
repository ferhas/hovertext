using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Text;
using HoverText.Core.Overlay;
using HoverText.Core.Probing;

namespace HoverText.App.Probing;

public sealed class UiAutomationTextProbeSource : ITextProbeSource, IFocusedInputProbeSource
{
    private AutomationElement? lastFocusedInput;

    public Task<TextProbeResult> TryReadAsync(PointerPoint point, CancellationToken cancellationToken = default)
    {
        try
        {
            TextProbeResult pointResult = TryReadElement(
                AutomationElement.FromPoint(new System.Windows.Point(point.X, point.Y)),
                point,
                requireInput: false);
            if (pointResult.DisplayKind == ProbeDisplayKind.Input)
            {
                return Task.FromResult(pointResult);
            }

            TextProbeResult focusedInputResult = TryReadFocusedInput(point);
            if (focusedInputResult.DisplayKind == ProbeDisplayKind.Input
                && IsPointInside(focusedInputResult.AnchorBounds, point))
            {
                return Task.FromResult(focusedInputResult);
            }

            if (pointResult.HasText)
            {
                return Task.FromResult(pointResult);
            }

            return Task.FromResult(focusedInputResult.HasText ? focusedInputResult : pointResult);
        }
        catch
        {
            return Task.FromResult(TextProbeResult.None(ProbeSource.UiAutomation));
        }
    }

    public Task<TextProbeResult> TryReadFocusedInputAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return Task.FromResult(TryReadFocusedInput(null, strictTextInput: true));
        }
        catch
        {
            return Task.FromResult(TextProbeResult.None(ProbeSource.UiAutomation));
        }
    }

    public Task<bool> TryWriteFocusedInputAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            if (lastFocusedInput is null || IsPasswordElement(lastFocusedInput))
            {
                return Task.FromResult(false);
            }

            if (!lastFocusedInput.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern))
            {
                return Task.FromResult(false);
            }

            var valuePattern = (ValuePattern)pattern;
            if (valuePattern.Current.IsReadOnly)
            {
                return Task.FromResult(false);
            }

            valuePattern.SetValue(text);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private TextProbeResult TryReadElement(AutomationElement? element, PointerPoint? point, bool requireInput, bool strictTextInput = false)
    {
        if (element is null)
        {
            return TextProbeResult.None(ProbeSource.UiAutomation);
        }

        element = requireInput ? FindInputElement(element, strictTextInput) : FindInputElement(element, strictTextInput: false) ?? element;
        if (element is null || IsPasswordElement(element))
        {
            return TextProbeResult.None(ProbeSource.UiAutomation);
        }

        bool isInput = IsInputElement(element);
        string? text = isInput
            ? TryReadInputText(element)
            : TryReadNonInputText(element, point);
        ProbeDisplayKind displayKind = isInput
            ? ProbeDisplayKind.Input
            : GuessDisplayKind(element, text);
        PixelRect? anchorBounds = TryGetBounds(element);
        if (displayKind == ProbeDisplayKind.Input && requireInput)
        {
            lastFocusedInput = element;
        }

        return ToResult(text, displayKind, anchorBounds);
    }

    private TextProbeResult TryReadFocusedInput(PointerPoint? point, bool strictTextInput = false)
    {
        try
        {
            AutomationElement focused = AutomationElement.FocusedElement;
            return TryReadElement(focused, point, requireInput: true, strictTextInput);
        }
        catch
        {
            return TextProbeResult.None(ProbeSource.UiAutomation);
        }
    }

    private static string? TryReadTextAtPoint(AutomationElement element, PointerPoint? point)
    {
        if (point is null || !element.TryGetCurrentPattern(TextPattern.Pattern, out object pattern))
        {
            return null;
        }

        try
        {
            TextPatternRange range = ((TextPattern)pattern).RangeFromPoint(new System.Windows.Point(point.Value.X, point.Value.Y));
            return NormalizeNonInputText(range.GetText(512));
        }
        catch
        {
            return null;
        }
    }

    private static string? TryReadNonInputText(AutomationElement element, PointerPoint? point)
    {
        return IsActionLikeElement(element)
            ? TryReadProperty(element, AutomationElement.HelpTextProperty)
                ?? TryReadProperty(element, AutomationElement.NameProperty)
                ?? TryReadValuePattern(element, allowEmpty: false)
                ?? TryReadTextAtPoint(element, point)
            : TryReadTextAtPoint(element, point)
                ?? TryReadValuePattern(element, allowEmpty: false)
                ?? TryReadProperty(element, AutomationElement.HelpTextProperty)
                ?? TryReadProperty(element, AutomationElement.NameProperty);
    }

    private static string TryReadInputText(AutomationElement element)
    {
        return TryReadValuePattern(element, allowEmpty: true)
            ?? TryReadFullTextPattern(element)
            ?? string.Empty;
    }

    private static string? TryReadFullTextPattern(AutomationElement element)
    {
        if (!element.TryGetCurrentPattern(TextPattern.Pattern, out object pattern))
        {
            return null;
        }

        try
        {
            string text = ((TextPattern)pattern).DocumentRange.GetText(2048);
            return NormalizeInputText(text);
        }
        catch
        {
            return null;
        }
    }

    private static string? TryReadValuePattern(AutomationElement element, bool allowEmpty)
    {
        if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern))
        {
            return null;
        }

        try
        {
            string? value = ((ValuePattern)pattern).Current.Value;
            return allowEmpty ? value ?? string.Empty : NormalizeNonInputText(value);
        }
        catch
        {
            return null;
        }
    }

    private static string? TryReadProperty(AutomationElement element, AutomationProperty property)
    {
        object value = element.GetCurrentPropertyValue(property, ignoreDefaultValue: true);
        return value is string text && !string.IsNullOrWhiteSpace(text) ? text : null;
    }

    private static bool IsInputElement(AutomationElement element)
    {
        return IsTextEntryElement(element);
    }

    private static bool IsTextEntryElement(AutomationElement element)
    {
        object controlType = element.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty, ignoreDefaultValue: true);
        return Equals(controlType, ControlType.Edit);
    }

    private static AutomationElement? FindInputElement(AutomationElement? element, bool strictTextInput)
    {
        AutomationElement? current = element;
        for (int depth = 0; depth < 5 && current is not null; depth++)
        {
            bool isInput = strictTextInput ? IsTextEntryElement(current) : IsInputElement(current);
            if (isInput)
            {
                return current;
            }

            try
            {
                current = TreeWalker.ControlViewWalker.GetParent(current);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    private static bool IsPasswordElement(AutomationElement element)
    {
        object value = element.GetCurrentPropertyValue(AutomationElement.IsPasswordProperty, ignoreDefaultValue: true);
        return value is bool isPassword && isPassword;
    }

    private static ProbeDisplayKind GuessDisplayKind(AutomationElement element, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return ProbeDisplayKind.Empty;
        }

        string? helpText = TryReadProperty(element, AutomationElement.HelpTextProperty);
        if (!string.IsNullOrWhiteSpace(helpText) && string.Equals(helpText.Trim(), text.Trim(), StringComparison.Ordinal))
        {
            return ProbeDisplayKind.Tooltip;
        }

        if (IsActionLikeElement(element))
        {
            return ProbeDisplayKind.Tooltip;
        }

        return ProbeDisplayKind.Text;
    }

    private static bool IsActionLikeElement(AutomationElement element)
    {
        object controlType = element.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty, ignoreDefaultValue: true);
        return Equals(controlType, ControlType.Button)
            || Equals(controlType, ControlType.MenuItem)
            || Equals(controlType, ControlType.Hyperlink)
            || Equals(controlType, ControlType.TabItem)
            || Equals(controlType, ControlType.ListItem);
    }

    private static TextProbeResult ToResult(string? text, ProbeDisplayKind displayKind, PixelRect? anchorBounds)
    {
        if (displayKind != ProbeDisplayKind.Input && string.IsNullOrWhiteSpace(text))
        {
            return TextProbeResult.None(ProbeSource.UiAutomation);
        }

        string normalized = displayKind == ProbeDisplayKind.Input
            ? NormalizeInputText(text ?? string.Empty)
            : ProbeTextNormalizer.NormalizeNonInput(text);
        return TextProbeResult.Found(normalized, ProbeSource.UiAutomation, displayKind, anchorBounds);
    }

    private static string? NormalizeNonInputText(string? text)
    {
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static string NormalizeInputText(string text)
    {
        return text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .TrimEnd('\r', '\n');
    }

    private static PixelRect? TryGetBounds(AutomationElement element)
    {
        object value = element.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty, ignoreDefaultValue: true);
        if (value is not Rect bounds || bounds.IsEmpty || bounds.Width <= 0 || bounds.Height <= 0)
        {
            return null;
        }

        return new PixelRect(
            (int)Math.Round(bounds.Left),
            (int)Math.Round(bounds.Top),
            (int)Math.Round(bounds.Width),
            (int)Math.Round(bounds.Height));
    }

    private static bool IsPointInside(PixelRect? bounds, PointerPoint point)
    {
        return bounds is null
            || (point.X >= bounds.Value.Left
                && point.X <= bounds.Value.Right
                && point.Y >= bounds.Value.Top
                && point.Y <= bounds.Value.Bottom);
    }
}
