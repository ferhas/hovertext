using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using HoverText.Core.Overlay;
using HoverText.Core.Probing;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using Forms = System.Windows.Forms;

namespace HoverText.App.Probing;

public sealed class WindowsOcrTextProbeSource : ITextProbeSource
{
    private const int CaptureWidth = 560;
    private const int CaptureHeight = 220;
    private const int CaptureScale = 2;
    private readonly Lazy<OcrEngine?> engine = new(CreateEngine);

    public async Task<TextProbeResult> TryReadAsync(PointerPoint point, CancellationToken cancellationToken = default)
    {
        OcrEngine? ocrEngine = engine.Value;
        if (ocrEngine is null)
        {
            return TextProbeResult.None(ProbeSource.Ocr);
        }

        try
        {
            byte[] image = CaptureAroundPoint(point);
            using var stream = new InMemoryRandomAccessStream();
            using (IOutputStream outputStream = stream.GetOutputStreamAt(0))
            using (var writer = new DataWriter(outputStream))
            {
                writer.WriteBytes(image);
                await writer.StoreAsync().AsTask(cancellationToken);
                await writer.FlushAsync().AsTask(cancellationToken);
                writer.DetachStream();
            }

            stream.Seek(0);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream).AsTask(cancellationToken);
            using SoftwareBitmap bitmap = await decoder
                .GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied)
                .AsTask(cancellationToken);
            OcrResult result = await ocrEngine.RecognizeAsync(bitmap).AsTask(cancellationToken);
            string normalized = Normalize(result);
            return string.IsNullOrWhiteSpace(normalized)
                ? TextProbeResult.None(ProbeSource.Ocr)
                : TextProbeResult.Found(normalized, ProbeSource.Ocr);
        }
        catch
        {
            return TextProbeResult.None(ProbeSource.Ocr);
        }
    }

    private static OcrEngine? CreateEngine()
    {
        OcrEngine? userProfileEngine = OcrEngine.TryCreateFromUserProfileLanguages();
        if (userProfileEngine is not null)
        {
            return userProfileEngine;
        }

        Language? chinese = OcrEngine.AvailableRecognizerLanguages
            .FirstOrDefault(language => language.LanguageTag.StartsWith("zh", StringComparison.OrdinalIgnoreCase));
        if (chinese is not null)
        {
            return OcrEngine.TryCreateFromLanguage(chinese);
        }

        Language? fallback = OcrEngine.AvailableRecognizerLanguages.FirstOrDefault();
        return fallback is null ? null : OcrEngine.TryCreateFromLanguage(fallback);
    }

    private static byte[] CaptureAroundPoint(PointerPoint point)
    {
        Rectangle virtualScreen = Forms.SystemInformation.VirtualScreen;
        int x = Clamp(point.X - CaptureWidth / 2, virtualScreen.Left, Math.Max(virtualScreen.Left, virtualScreen.Right - CaptureWidth));
        int y = Clamp(point.Y - CaptureHeight / 2, virtualScreen.Top, Math.Max(virtualScreen.Top, virtualScreen.Bottom - CaptureHeight));

        using var capture = new Bitmap(CaptureWidth, CaptureHeight, PixelFormat.Format32bppArgb);
        using (Graphics graphics = Graphics.FromImage(capture))
        {
            graphics.CopyFromScreen(x, y, 0, 0, new Size(CaptureWidth, CaptureHeight), CopyPixelOperation.SourceCopy);
        }

        using var scaled = new Bitmap(CaptureWidth * CaptureScale, CaptureHeight * CaptureScale, PixelFormat.Format32bppArgb);
        using (Graphics graphics = Graphics.FromImage(scaled))
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.DrawImage(capture, new Rectangle(0, 0, scaled.Width, scaled.Height));
        }

        using var memory = new MemoryStream();
        scaled.Save(memory, ImageFormat.Png);
        return memory.ToArray();
    }

    private static int Clamp(int value, int min, int max)
    {
        return Math.Min(Math.Max(value, min), max);
    }

    private static string Normalize(OcrResult result)
    {
        return string.Join(
            ' ',
            result.Lines
                .Select(line => line.Text.Trim())
                .Where(line => line.Length > 0));
    }
}
