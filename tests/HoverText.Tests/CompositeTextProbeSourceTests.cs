using HoverText.Core.Overlay;
using HoverText.Core.Probing;

namespace HoverText.Tests;

[TestClass]
public sealed class CompositeTextProbeSourceTests
{
    [TestMethod]
    public async Task TryReadAsync_uses_first_text_result_from_available_sources()
    {
        var first = new RecordingTextProbeSource(TextProbeResult.None(ProbeSource.Ocr));
        var second = new RecordingTextProbeSource(TextProbeResult.Found("钉钉消息内容", ProbeSource.Ocr));
        var third = new RecordingTextProbeSource(TextProbeResult.Found("unused", ProbeSource.Ocr));
        var source = new CompositeTextProbeSource(first, second, third);

        TextProbeResult result = await source.TryReadAsync(new PointerPoint(24, 48));

        Assert.IsTrue(result.HasText);
        Assert.AreEqual("钉钉消息内容", result.Text);
        Assert.AreEqual(ProbeSource.Ocr, result.Source);
        Assert.AreEqual(1, first.Calls);
        Assert.AreEqual(1, second.Calls);
        Assert.AreEqual(0, third.Calls);
    }

    [TestMethod]
    public async Task TryReadAsync_returns_none_when_all_sources_are_unavailable()
    {
        var first = new RecordingTextProbeSource(TextProbeResult.None(ProbeSource.Ocr));
        var second = new RecordingTextProbeSource(TextProbeResult.None(ProbeSource.Ocr));
        var source = new CompositeTextProbeSource(first, second);

        TextProbeResult result = await source.TryReadAsync(new PointerPoint(24, 48));

        Assert.IsFalse(result.HasText);
        Assert.AreEqual(ProbeSource.Ocr, result.Source);
        Assert.AreEqual(1, first.Calls);
        Assert.AreEqual(1, second.Calls);
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
}
