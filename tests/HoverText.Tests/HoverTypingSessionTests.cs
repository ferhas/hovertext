using HoverText.Core.Overlay;
using HoverText.Core.Settings;
using HoverText.Core.Typing;

namespace HoverText.Tests;

[TestClass]
public sealed class HoverTypingSessionTests
{
    [TestMethod]
    public void Evaluate_hides_when_hover_typing_is_disabled()
    {
        var session = new HoverTypingSession();
        HoverTextSettings settings = HoverTextSettings.CreateDefault() with
        {
            IsHoverTypingEnabled = false
        };
        var snapshot = HoverTypingSnapshot.FromTextEntry(
            "field-1",
            "hello",
            new PixelRect(100, 200, 360, 42));

        HoverTypingDisplay display = session.Evaluate(settings, snapshot, isTriggerPressed: true, escapePressed: false);

        Assert.IsFalse(display.IsVisible);
    }

    [TestMethod]
    public void Evaluate_shows_large_typing_text_for_focused_editable_text()
    {
        var session = new HoverTypingSession();
        HoverTextSettings settings = HoverTextSettings.CreateDefault() with
        {
            IsHoverTypingEnabled = true
        };
        var anchor = new PixelRect(100, 200, 360, 42);
        var snapshot = HoverTypingSnapshot.FromTextEntry("field-1", "hello", anchor);

        HoverTypingDisplay display = session.Evaluate(settings, snapshot, isTriggerPressed: true, escapePressed: false);

        Assert.IsTrue(display.IsVisible);
        Assert.AreEqual("hello", display.Text);
        Assert.AreEqual(anchor, display.AnchorBounds);
    }

    [TestMethod]
    public void Evaluate_hides_focused_editable_text_until_the_trigger_key_is_pressed()
    {
        var session = new HoverTypingSession();
        HoverTextSettings settings = HoverTextSettings.CreateDefault() with
        {
            IsHoverTypingEnabled = true
        };
        var snapshot = HoverTypingSnapshot.FromTextEntry(
            "field-1",
            "hello",
            new PixelRect(100, 200, 360, 42));

        HoverTypingDisplay display = session.Evaluate(settings, snapshot, isTriggerPressed: false, escapePressed: false);

        Assert.IsFalse(display.IsVisible);
    }

    [TestMethod]
    public void Evaluate_hides_after_escape_until_the_user_types_more_text()
    {
        var session = new HoverTypingSession();
        HoverTextSettings settings = HoverTextSettings.CreateDefault() with
        {
            IsHoverTypingEnabled = true
        };
        var anchor = new PixelRect(100, 200, 360, 42);
        var first = HoverTypingSnapshot.FromTextEntry("field-1", "hello", anchor);
        var updated = HoverTypingSnapshot.FromTextEntry("field-1", "hello!", anchor);

        HoverTypingDisplay hidden = session.Evaluate(settings, first, isTriggerPressed: true, escapePressed: true);
        HoverTypingDisplay stillHidden = session.Evaluate(settings, first, isTriggerPressed: true, escapePressed: false);
        HoverTypingDisplay shownAgain = session.Evaluate(settings, updated, isTriggerPressed: true, escapePressed: false);

        Assert.IsFalse(hidden.IsVisible);
        Assert.IsFalse(stillHidden.IsVisible);
        Assert.IsTrue(shownAgain.IsVisible);
        Assert.AreEqual("hello!", shownAgain.Text);
    }

    [TestMethod]
    public void Evaluate_clears_escape_suppression_after_text_is_cleared()
    {
        var session = new HoverTypingSession();
        HoverTextSettings settings = HoverTextSettings.CreateDefault() with
        {
            IsHoverTypingEnabled = true
        };
        var anchor = new PixelRect(100, 200, 360, 42);
        var first = HoverTypingSnapshot.FromTextEntry("field-1", "hello", anchor);
        var empty = HoverTypingSnapshot.FromTextEntry("field-1", string.Empty, anchor);

        _ = session.Evaluate(settings, first, isTriggerPressed: true, escapePressed: true);
        _ = session.Evaluate(settings, empty, isTriggerPressed: true, escapePressed: false);
        HoverTypingDisplay shownAgain = session.Evaluate(settings, first, isTriggerPressed: true, escapePressed: false);

        Assert.IsTrue(shownAgain.IsVisible);
        Assert.AreEqual("hello", shownAgain.Text);
    }

    [TestMethod]
    public void Evaluate_hides_when_focused_input_text_is_too_long()
    {
        var session = new HoverTypingSession();
        HoverTextSettings settings = HoverTextSettings.CreateDefault() with
        {
            IsHoverTypingEnabled = true
        };
        string longText = new('a', InputTextDisplayPolicy.MaxDisplayCharacters + 1);
        var snapshot = HoverTypingSnapshot.FromTextEntry(
            "field-1",
            longText,
            new PixelRect(100, 200, 360, 42));

        HoverTypingDisplay display = session.Evaluate(settings, snapshot, isTriggerPressed: true, escapePressed: false);

        Assert.IsFalse(display.IsVisible);
    }

    [TestMethod]
    public void Evaluate_hides_when_focused_input_text_has_too_many_lines()
    {
        var session = new HoverTypingSession();
        HoverTextSettings settings = HoverTextSettings.CreateDefault() with
        {
            IsHoverTypingEnabled = true
        };
        string manyLines = string.Join('\n', Enumerable.Range(0, InputTextDisplayPolicy.MaxDisplayLines + 1).Select(_ => "line"));
        var snapshot = HoverTypingSnapshot.FromTextEntry(
            "field-1",
            manyLines,
            new PixelRect(100, 200, 360, 42));

        HoverTypingDisplay display = session.Evaluate(settings, snapshot, isTriggerPressed: true, escapePressed: false);

        Assert.IsFalse(display.IsVisible);
    }
}
