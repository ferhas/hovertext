using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Text;
using HoverText.Core.Overlay;
using HoverText.Core.Typing;

namespace HoverText.App.Probing;

public sealed class UiAutomationHoverTypingProbeSource : IHoverTypingProbeSource
{
    public Task<HoverTypingSnapshot> ReadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            AutomationElement focused = AutomationElement.FocusedElement;
            HoverTypingSnapshot snapshot = ReadFocusedElement(focused);
            if (snapshot.HasTextEntry)
            {
                return Task.FromResult(snapshot);
            }

            AutomationElement? fallback = FindEditableDescendantNearFocus(focused);
            return Task.FromResult(ReadFocusedElement(fallback));
        }
        catch
        {
            return Task.FromResult(HoverTypingSnapshot.None());
        }
    }

    private static HoverTypingSnapshot ReadFocusedElement(AutomationElement? element)
    {
        if (element is null || !IsEditableTextElement(element) || IsPasswordElement(element))
        {
            return HoverTypingSnapshot.None();
        }

        string className = ReadStringProperty(element, AutomationElement.ClassNameProperty);
        string? rawText = TryReadValuePattern(element) ?? TryReadTextPattern(element);
        string? text = rawText is null
            ? null
            : HoverTypingTextSanitizer.NormalizeForDisplay(
                rawText,
                className,
                ReadDescendantTextNames(element));
        if (text is null)
        {
            return HoverTypingSnapshot.None();
        }

        return HoverTypingSnapshot.FromTextEntry(
            CreateFocusKey(element),
            text,
            TryGetBounds(element));
    }

    private static bool IsEditableTextElement(AutomationElement element)
    {
        object controlType = element.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty, ignoreDefaultValue: true);
        string className = ReadStringProperty(element, AutomationElement.ClassNameProperty);
        object isEnabled = element.GetCurrentPropertyValue(AutomationElement.IsEnabledProperty, ignoreDefaultValue: true);
        object isKeyboardFocusable = element.GetCurrentPropertyValue(AutomationElement.IsKeyboardFocusableProperty, ignoreDefaultValue: true);

        return HoverTypingTextEntryClassifier.IsEditableTextEntry(
            controlType is ControlType type ? type.ProgrammaticName : string.Empty,
            className,
            isKeyboardFocusable is bool focusable && focusable,
            isEnabled is not bool enabled || enabled,
            element.TryGetCurrentPattern(ValuePattern.Pattern, out _),
            element.TryGetCurrentPattern(TextPattern.Pattern, out _));
    }

    private static bool IsPasswordElement(AutomationElement element)
    {
        object value = element.GetCurrentPropertyValue(AutomationElement.IsPasswordProperty, ignoreDefaultValue: true);
        return value is bool isPassword && isPassword;
    }

    private static string? TryReadValuePattern(AutomationElement element)
    {
        if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern))
        {
            return null;
        }

        try
        {
            return Normalize(((ValuePattern)pattern).Current.Value ?? string.Empty);
        }
        catch
        {
            return null;
        }
    }

    private static string? TryReadTextPattern(AutomationElement element)
    {
        if (!element.TryGetCurrentPattern(TextPattern.Pattern, out object pattern))
        {
            return null;
        }

        try
        {
            return Normalize(((TextPattern)pattern).DocumentRange.GetText(4096));
        }
        catch
        {
            return null;
        }
    }

    private static string Normalize(string text)
    {
        return text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .TrimEnd('\r', '\n');
    }

    private static AutomationElement? FindEditableDescendantNearFocus(AutomationElement focused)
    {
        AutomationElement? root = FindAncestorDocument(focused);
        if (root is null)
        {
            return null;
        }

        AutomationElementCollection descendants = root.FindAll(
            TreeScope.Descendants,
            System.Windows.Automation.Condition.TrueCondition);
        AutomationElement? best = null;
        int bestTop = int.MinValue;
        for (int index = 0; index < descendants.Count; index++)
        {
            AutomationElement candidate = descendants[index];
            if (!IsEditableTextElement(candidate))
            {
                continue;
            }

            PixelRect? bounds = TryGetBounds(candidate);
            if (bounds is null)
            {
                continue;
            }

            if (bounds.Value.Top >= bestTop)
            {
                best = candidate;
                bestTop = bounds.Value.Top;
            }
        }

        return best;
    }

    private static AutomationElement? FindAncestorDocument(AutomationElement element)
    {
        AutomationElement? current = element;
        for (int depth = 0; depth < 8 && current is not null; depth++)
        {
            object controlType = current.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty, ignoreDefaultValue: true);
            object automationId = current.GetCurrentPropertyValue(AutomationElement.AutomationIdProperty, ignoreDefaultValue: true);
            if (Equals(controlType, ControlType.Document)
                && automationId is string id
                && string.Equals(id, "RootWebArea", StringComparison.Ordinal))
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

    private static string CreateFocusKey(AutomationElement element)
    {
        try
        {
            int[] runtimeId = element.GetRuntimeId();
            if (runtimeId.Length > 0)
            {
                return string.Join(".", runtimeId);
            }
        }
        catch
        {
        }

        string automationId = ReadStringProperty(element, AutomationElement.AutomationIdProperty);
        string name = ReadStringProperty(element, AutomationElement.NameProperty);
        PixelRect? bounds = TryGetBounds(element);
        return $"{automationId}|{name}|{bounds}";
    }

    private static string ReadStringProperty(AutomationElement element, AutomationProperty property)
    {
        object value = element.GetCurrentPropertyValue(property, ignoreDefaultValue: true);
        return value as string ?? string.Empty;
    }

    private static string[] ReadDescendantTextNames(AutomationElement element)
    {
        AutomationElementCollection descendants = element.FindAll(
            TreeScope.Descendants,
            System.Windows.Automation.Condition.TrueCondition);
        var names = new List<string>();
        for (int index = 0; index < descendants.Count; index++)
        {
            AutomationElement descendant = descendants[index];
            object controlType = descendant.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty, ignoreDefaultValue: true);
            if (!Equals(controlType, ControlType.Text))
            {
                continue;
            }

            string name = ReadStringProperty(descendant, AutomationElement.NameProperty);
            if (!string.IsNullOrWhiteSpace(name))
            {
                names.Add(name.Trim());
            }
        }

        return [.. names];
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
}
