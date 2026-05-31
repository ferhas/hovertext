using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Text;
using HoverText.Core.Overlay;
using HoverText.Core.Probing;

namespace HoverText.App.Probing;

public sealed class UiAutomationTextProbeSource : ITextProbeSource
{
    public Task<TextProbeResult> TryReadAsync(PointerPoint point, CancellationToken cancellationToken = default)
    {
        try
        {
            TextProbeResult pointResult = TryReadElement(AutomationElement.FromPoint(new System.Windows.Point(point.X, point.Y)), point);
            if (pointResult.HasText)
            {
                return Task.FromResult(pointResult);
            }

            TextProbeResult focusedInputResult = TryReadFocusedInput(point);
            return Task.FromResult(focusedInputResult.HasText ? focusedInputResult : pointResult);
        }
        catch
        {
            return Task.FromResult(TextProbeResult.None(ProbeSource.UiAutomation));
        }
    }

    private static TextProbeResult TryReadElement(AutomationElement? element, PointerPoint point)
    {
        if (element is null)
        {
            return TextProbeResult.None(ProbeSource.UiAutomation);
        }

        bool isInput = IsInputElement(element);
        string? text = TryReadTextPattern(element, point)
            ?? TryReadValuePattern(element)
            ?? TryReadProperty(element, AutomationElement.HelpTextProperty)
            ?? TryReadProperty(element, AutomationElement.NameProperty);
        ProbeDisplayKind displayKind = isInput
            ? ProbeDisplayKind.Input
            : GuessDisplayKind(element, text);

        return ToResult(text, displayKind);
    }

    private static TextProbeResult TryReadFocusedInput(PointerPoint point)
    {
        try
        {
            AutomationElement focused = AutomationElement.FocusedElement;
            return IsInputElement(focused)
                ? TryReadElement(focused, point)
                : TextProbeResult.None(ProbeSource.UiAutomation);
        }
        catch
        {
            return TextProbeResult.None(ProbeSource.UiAutomation);
        }
    }

    private static string? TryReadTextPattern(AutomationElement element, PointerPoint point)
    {
        if (!element.TryGetCurrentPattern(TextPattern.Pattern, out object pattern))
        {
            return null;
        }

        TextPatternRange range = ((TextPattern)pattern).RangeFromPoint(new System.Windows.Point(point.X, point.Y));
        string text = range.GetText(512);
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static string? TryReadValuePattern(AutomationElement element)
    {
        if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern))
        {
            return null;
        }

        string value = ((ValuePattern)pattern).Current.Value;
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string? TryReadProperty(AutomationElement element, AutomationProperty property)
    {
        object value = element.GetCurrentPropertyValue(property, ignoreDefaultValue: true);
        return value is string text && !string.IsNullOrWhiteSpace(text) ? text : null;
    }

    private static bool IsInputElement(AutomationElement element)
    {
        object controlType = element.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty, ignoreDefaultValue: true);
        return Equals(controlType, ControlType.Edit)
            || Equals(controlType, ControlType.Document);
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

        object controlType = element.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty, ignoreDefaultValue: true);
        if (Equals(controlType, ControlType.Button)
            || Equals(controlType, ControlType.MenuItem)
            || Equals(controlType, ControlType.Hyperlink))
        {
            return ProbeDisplayKind.Tooltip;
        }

        return ProbeDisplayKind.Text;
    }

    private static TextProbeResult ToResult(string? text, ProbeDisplayKind displayKind)
    {
        if (displayKind != ProbeDisplayKind.Input && string.IsNullOrWhiteSpace(text))
        {
            return TextProbeResult.None(ProbeSource.UiAutomation);
        }

        string normalized = string.Join(' ', (text ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return TextProbeResult.Found(normalized, ProbeSource.UiAutomation, displayKind);
    }
}
