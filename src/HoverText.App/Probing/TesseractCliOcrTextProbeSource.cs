using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using HoverText.Core.Overlay;
using HoverText.Core.Probing;

namespace HoverText.App.Probing;

public sealed class TesseractCliOcrTextProbeSource : ITextProbeSource
{
    private const int CaptureWidth = 360;
    private const int CaptureHeight = 160;
    private readonly string? executablePath;

    public TesseractCliOcrTextProbeSource()
    {
        executablePath = FindTesseract();
    }

    public async Task<TextProbeResult> TryReadAsync(PointerPoint point, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return TextProbeResult.None(ProbeSource.Ocr);
        }

        string imagePath = Path.Combine(Path.GetTempPath(), $"hovertext-ocr-{Guid.NewGuid():N}.png");
        try
        {
            CaptureAroundPoint(point, imagePath);
            string output = await RunTesseractAsync(imagePath, cancellationToken);
            string normalized = string.Join(' ', output.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            return string.IsNullOrWhiteSpace(normalized)
                ? TextProbeResult.None(ProbeSource.Ocr)
                : TextProbeResult.Found(normalized, ProbeSource.Ocr);
        }
        catch
        {
            return TextProbeResult.None(ProbeSource.Ocr);
        }
        finally
        {
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
        }
    }

    private static void CaptureAroundPoint(PointerPoint point, string imagePath)
    {
        int x = Math.Max(0, point.X - CaptureWidth / 2);
        int y = Math.Max(0, point.Y - CaptureHeight / 2);
        using var bitmap = new Bitmap(CaptureWidth, CaptureHeight);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(x, y, 0, 0, new Size(CaptureWidth, CaptureHeight));
        bitmap.Save(imagePath, ImageFormat.Png);
    }

    private async Task<string> RunTesseractAsync(string imagePath, CancellationToken cancellationToken)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = executablePath!,
            Arguments = $"\"{imagePath}\" stdout --psm 6",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        process.Start();
        Task<string> outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken).WaitAsync(TimeSpan.FromSeconds(2), cancellationToken);
        return await outputTask;
    }

    private static string? FindTesseract()
    {
        string[] candidates =
        [
            "tesseract.exe",
            @"C:\Program Files\Tesseract-OCR\tesseract.exe",
            @"C:\Program Files (x86)\Tesseract-OCR\tesseract.exe"
        ];

        string? path = Environment.GetEnvironmentVariable("PATH");
        foreach (string candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }

            foreach (string directory in (path ?? "").Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                string fullPath = Path.Combine(directory.Trim(), candidate);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
        }

        return null;
    }
}
