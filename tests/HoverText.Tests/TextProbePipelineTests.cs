using HoverText.Core.Overlay;
using HoverText.Core.Probing;
using HoverText.Core.Settings;

namespace HoverText.Tests;

[TestClass]
public sealed class TextProbePipelineTests
{
    [TestMethod]
    public async Task ProbeAsync_uses_ui_automation_text_before_ocr_or_magnifier()
    {
        var automation = new RecordingTextProbeSource(TextProbeResult.Found("Open settings", ProbeSource.UiAutomation));
        var ocr = new RecordingTextProbeSource(TextProbeResult.Found("OCR text", ProbeSource.Ocr));
        var magnifier = new RecordingMagnifierFallback(MagnifierResult.Captured(new PixelRect(10, 10, 120, 80), 2.0));
        var pipeline = new TextProbePipeline(automation, ocr, magnifier);

        ProbeResult result = await pipeline.ProbeAsync(new PointerPoint(32, 64), HoverTextSettings.CreateDefault());

        Assert.AreEqual(ProbeSource.UiAutomation, result.Source);
        Assert.AreEqual("Open settings", result.DisplayText);
        Assert.AreEqual(1, automation.Calls);
        Assert.AreEqual(0, ocr.Calls);
        Assert.AreEqual(0, magnifier.Calls);
    }

    [TestMethod]
    public async Task ProbeAsync_uses_ocr_when_automation_has_no_text_and_ocr_is_enabled()
    {
        var automation = new RecordingTextProbeSource(TextProbeResult.None(ProbeSource.UiAutomation));
        var ocr = new RecordingTextProbeSource(TextProbeResult.Found("OCR fallback", ProbeSource.Ocr));
        var magnifier = new RecordingMagnifierFallback(MagnifierResult.Captured(new PixelRect(10, 10, 120, 80), 2.0));
        var pipeline = new TextProbePipeline(automation, ocr, magnifier);

        ProbeResult result = await pipeline.ProbeAsync(new PointerPoint(32, 64), HoverTextSettings.CreateDefault());

        Assert.AreEqual(ProbeSource.Ocr, result.Source);
        Assert.AreEqual("OCR fallback", result.DisplayText);
        Assert.AreEqual(1, automation.Calls);
        Assert.AreEqual(1, ocr.Calls);
        Assert.AreEqual(0, magnifier.Calls);
    }

    [TestMethod]
    public async Task ProbeAsync_uses_magnifier_when_text_sources_are_unavailable()
    {
        var automation = new RecordingTextProbeSource(TextProbeResult.None(ProbeSource.UiAutomation));
        var ocr = new RecordingTextProbeSource(TextProbeResult.None(ProbeSource.Ocr));
        var magnifier = new RecordingMagnifierFallback(MagnifierResult.Captured(new PixelRect(20, 24, 200, 140), 2.5));
        int beforeMagnifierCalls = 0;
        var pipeline = new TextProbePipeline(automation, ocr, magnifier, () =>
        {
            beforeMagnifierCalls++;
            return Task.CompletedTask;
        });

        ProbeResult result = await pipeline.ProbeAsync(new PointerPoint(32, 64), HoverTextSettings.CreateDefault());

        Assert.AreEqual(ProbeSource.Magnifier, result.Source);
        Assert.AreEqual(ProbeDisplayKind.Magnifier, result.DisplayKind);
        Assert.AreEqual("", result.DisplayText);
        Assert.AreEqual(1, automation.Calls);
        Assert.AreEqual(1, ocr.Calls);
        Assert.AreEqual(1, beforeMagnifierCalls);
        Assert.AreEqual(1, magnifier.Calls);
    }

    [TestMethod]
    public async Task ProbeAsync_does_not_run_before_magnifier_callback_when_text_is_found()
    {
        var automation = new RecordingTextProbeSource(TextProbeResult.Found("Readable", ProbeSource.UiAutomation));
        var ocr = new RecordingTextProbeSource(TextProbeResult.None(ProbeSource.Ocr));
        var magnifier = new RecordingMagnifierFallback(MagnifierResult.Captured(new PixelRect(20, 24, 200, 140), 3.0));
        int beforeMagnifierCalls = 0;
        var pipeline = new TextProbePipeline(automation, ocr, magnifier, () =>
        {
            beforeMagnifierCalls++;
            return Task.CompletedTask;
        });

        ProbeResult result = await pipeline.ProbeAsync(new PointerPoint(32, 64), HoverTextSettings.CreateDefault());

        Assert.AreEqual(ProbeDisplayKind.Text, result.DisplayKind);
        Assert.AreEqual(0, beforeMagnifierCalls);
        Assert.AreEqual(0, magnifier.Calls);
    }

    [TestMethod]
    public async Task ProbeAsync_shows_tooltip_text_without_falling_back_to_magnifier()
    {
        var automation = new RecordingTextProbeSource(
            TextProbeResult.Found("Settings", ProbeSource.UiAutomation, ProbeDisplayKind.Tooltip));
        var ocr = new RecordingTextProbeSource(TextProbeResult.None(ProbeSource.Ocr));
        var magnifier = new RecordingMagnifierFallback(MagnifierResult.Captured(new PixelRect(20, 24, 200, 140), 2.0));
        var pipeline = new TextProbePipeline(automation, ocr, magnifier);

        ProbeResult result = await pipeline.ProbeAsync(new PointerPoint(32, 64), HoverTextSettings.CreateDefault());

        Assert.AreEqual(ProbeDisplayKind.Tooltip, result.DisplayKind);
        Assert.AreEqual("Settings", result.DisplayText);
        Assert.AreEqual(0, magnifier.Calls);
    }

    [TestMethod]
    public async Task ProbeAsync_preserves_input_text_kind_for_editing_loupe()
    {
        var automation = new RecordingTextProbeSource(
            TextProbeResult.Found("hello world", ProbeSource.UiAutomation, ProbeDisplayKind.Input));
        var ocr = new RecordingTextProbeSource(TextProbeResult.None(ProbeSource.Ocr));
        var magnifier = new RecordingMagnifierFallback(MagnifierResult.Captured(new PixelRect(20, 24, 200, 140), 2.0));
        var pipeline = new TextProbePipeline(automation, ocr, magnifier);

        ProbeResult result = await pipeline.ProbeAsync(new PointerPoint(32, 64), HoverTextSettings.CreateDefault());

        Assert.AreEqual(ProbeDisplayKind.Input, result.DisplayKind);
        Assert.AreEqual("hello world", result.DisplayText);
        Assert.AreEqual(0, magnifier.Calls);
    }

    [TestMethod]
    public async Task ProbeAsync_preserves_empty_input_kind_without_magnifier()
    {
        var automation = new RecordingTextProbeSource(
            TextProbeResult.Found("", ProbeSource.UiAutomation, ProbeDisplayKind.Input));
        var ocr = new RecordingTextProbeSource(TextProbeResult.None(ProbeSource.Ocr));
        var magnifier = new RecordingMagnifierFallback(MagnifierResult.Captured(new PixelRect(20, 24, 200, 140), 2.0));
        var pipeline = new TextProbePipeline(automation, ocr, magnifier);

        ProbeResult result = await pipeline.ProbeAsync(new PointerPoint(32, 64), HoverTextSettings.CreateDefault());

        Assert.AreEqual(ProbeDisplayKind.Input, result.DisplayKind);
        Assert.AreEqual("", result.DisplayText);
        Assert.AreEqual(0, magnifier.Calls);
    }

    [TestMethod]
    public async Task ProbeAsync_skips_ocr_when_setting_is_disabled()
    {
        var automation = new RecordingTextProbeSource(TextProbeResult.None(ProbeSource.UiAutomation));
        var ocr = new RecordingTextProbeSource(TextProbeResult.Found("OCR fallback", ProbeSource.Ocr));
        var magnifier = new RecordingMagnifierFallback(MagnifierResult.Captured(new PixelRect(20, 24, 200, 140), 2.0));
        var pipeline = new TextProbePipeline(automation, ocr, magnifier);
        HoverTextSettings settings = HoverTextSettings.CreateDefault() with { IsOcrEnabled = false };

        ProbeResult result = await pipeline.ProbeAsync(new PointerPoint(32, 64), settings);

        Assert.AreEqual(ProbeSource.Magnifier, result.Source);
        Assert.AreEqual(0, ocr.Calls);
        Assert.AreEqual(1, magnifier.Calls);
    }

    private sealed class RecordingTextProbeSource(TextProbeResult result) : ITextProbeSource
    {
        public int Calls { get; private set; }

        public Task<TextProbeResult> TryReadAsync(PointerPoint point, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(result);
        }
    }

    private sealed class RecordingMagnifierFallback(MagnifierResult result) : IMagnifierFallback
    {
        public int Calls { get; private set; }

        public Task<MagnifierResult> CaptureAsync(PointerPoint point, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(result);
        }
    }
}
