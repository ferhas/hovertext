using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Forms = System.Windows.Forms;
using HoverText.Core.Overlay;
using HoverText.Core.Platform;
using HoverText.Core.Probing;
using HoverText.Core.Settings;
using HoverText.Core.Typing;

namespace HoverText.App.Ui;

public partial class OverlayWindow : Window
{
    private const int GwlExStyle = -20;
    private const int WsExTransparent = 0x00000020;
    private const int WsExToolWindow = 0x00000080;
    private const int WsExNoActivate = 0x08000000;
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoMove = 0x0002;
    private const uint SwpNoZOrder = 0x0004;
    private const uint SwpNoActivate = 0x0010;
    private const uint SwpFrameChanged = 0x0020;
    private OverlayLayoutFingerprint? lastLayoutFingerprint;
    private string? cachedForegroundColor;
    private string? cachedBackgroundColor;
    private System.Windows.Media.Brush foregroundBrush = System.Windows.Media.Brushes.White;
    private System.Windows.Media.Brush backgroundBrush = System.Windows.Media.Brushes.Black;
    private PixelSize? lastMagnifierDisplaySize;
    private IntPtr windowHandle;

    public OverlayWindow()
    {
        InitializeComponent();
    }

    public void ShowProbeResult(
        ProbeResult result,
        HoverTextSettings settings,
        PointerPoint cursor,
        BitmapSource? magnifierCapture)
    {
        OverlayLayoutFingerprint layoutFingerprint = OverlayLayoutFingerprint.From(result, settings);
        bool needsLayout = !IsVisible || lastLayoutFingerprint != layoutFingerprint;

        ApplySettings(settings, result);
        string sourceText = result.Source switch
        {
            ProbeSource.UiAutomation => "UI Automation",
            ProbeSource.Ocr => "OCR",
            ProbeSource.Magnifier => "Magnifier",
            _ => "HoverText"
        };

        if (SourceLabel.Text != sourceText)
        {
            SourceLabel.Text = sourceText;
        }

        if (needsLayout)
        {
            ApplyDisplayKind(result.DisplayKind, settings);
        }

        UpdateMagnifier(result, magnifierCapture, needsLayout);
        UpdateDisplayText(result);

        if (needsLayout)
        {
            UpdateLayout();
            lastLayoutFingerprint = layoutFingerprint;
        }

        PositionNearCursor(cursor);

        SetInteractive(false);
        if (!IsVisible)
        {
            Show();
        }
    }

    public void ShowHoverTypingDisplay(
        HoverTypingDisplay display,
        HoverTextSettings settings,
        PointerPoint fallbackCursor)
    {
        var result = new ProbeResult(
            ProbeSource.UiAutomation,
            display.Text,
            null,
            ProbeDisplayKind.Text,
            display.AnchorBounds);
        OverlayLayoutFingerprint layoutFingerprint = OverlayLayoutFingerprint.From(result, settings);
        bool needsLayout = !IsVisible || lastLayoutFingerprint != layoutFingerprint;

        ApplySettings(settings, result);
        if (SourceLabel.Text != "Typing")
        {
            SourceLabel.Text = "Typing";
        }

        if (needsLayout)
        {
            ApplyDisplayKind(result.DisplayKind, settings);
        }

        UpdateMagnifier(result, null, needsLayout);
        UpdateDisplayText(result);

        if (needsLayout)
        {
            UpdateLayout();
            lastLayoutFingerprint = layoutFingerprint;
        }

        if (display.AnchorBounds is PixelRect anchor)
        {
            PositionNearAnchor(anchor);
        }
        else
        {
            PositionNearCursor(fallbackCursor);
        }

        SetInteractive(false);
        if (!IsVisible)
        {
            Show();
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        windowHandle = new WindowInteropHelper(this).Handle;
        SetInteractive(false);
        EnableCaptureExclusion();
    }

    private void ApplySettings(HoverTextSettings settings, ProbeResult result)
    {
        foregroundBrush = GetCachedBrush(settings.Foreground, System.Windows.Media.Brushes.White, ref cachedForegroundColor, ref foregroundBrush);
        backgroundBrush = GetCachedBrush(settings.Background, System.Windows.Media.Brushes.Black, ref cachedBackgroundColor, ref backgroundBrush);

        double targetFontSize = OverlayTextSizing.CalculateFontSize(settings.FontSize, result.DisplayText, result.DisplayKind);
        if (!DisplayText.FontSize.Equals(targetFontSize))
        {
            DisplayText.FontSize = targetFontSize;
        }

        DisplayText.Foreground = foregroundBrush;
        SourceLabel.Foreground = foregroundBrush;
        Shell.Background = backgroundBrush;
    }

    private void ApplyDisplayKind(ProbeDisplayKind displayKind, HoverTextSettings settings)
    {
        Shell.CornerRadius = new CornerRadius(8);
        Shell.BorderThickness = new Thickness(1);
        Shell.MinWidth = 0;
        Shell.MinHeight = 0;
        SourceLabel.Visibility = Visibility.Visible;

        if (displayKind == ProbeDisplayKind.Tooltip)
        {
            SourceLabel.Text = "Tip";
            Shell.BorderBrush = System.Windows.Media.Brushes.DeepSkyBlue;
        }
        else
        {
            Shell.BorderBrush = foregroundBrush;
        }
    }

    private void UpdateMagnifier(ProbeResult result, BitmapSource? magnifierCapture, bool needsLayout)
    {
        if (result.Source == ProbeSource.Magnifier && magnifierCapture is not null)
        {
            if (!ReferenceEquals(MagnifierImage.Source, magnifierCapture))
            {
                MagnifierImage.Source = magnifierCapture;
            }

            MagnifierImage.Visibility = Visibility.Visible;
            if (result.Magnifier is not null && needsLayout)
            {
                var displaySize = new PixelSize(
                    (int)Math.Round(result.Magnifier.CaptureArea.Width * result.Magnifier.Scale),
                    (int)Math.Round(result.Magnifier.CaptureArea.Height * result.Magnifier.Scale));
                if (lastMagnifierDisplaySize != displaySize)
                {
                    MagnifierImage.Width = displaySize.Width;
                    MagnifierImage.Height = displaySize.Height;
                    lastMagnifierDisplaySize = displaySize;
                }
            }

            return;
        }

        if (MagnifierImage.Source is not null)
        {
            MagnifierImage.Source = null;
        }

        MagnifierImage.Visibility = Visibility.Collapsed;
        MagnifierImage.Width = double.NaN;
        MagnifierImage.Height = double.NaN;
        lastMagnifierDisplaySize = null;
    }

    private void UpdateDisplayText(ProbeResult result)
    {
        string displayText = result.DisplayText;
        if (DisplayText.Text != displayText)
        {
            DisplayText.Text = displayText;
        }

        Visibility textVisibility = string.IsNullOrWhiteSpace(DisplayText.Text)
            ? Visibility.Collapsed
            : Visibility.Visible;
        if (DisplayText.Visibility != textVisibility)
        {
            DisplayText.Visibility = textVisibility;
        }
    }

    private void PositionNearCursor(PointerPoint cursor)
    {
        Forms.Screen screen = Forms.Screen.FromPoint(new System.Drawing.Point(cursor.X, cursor.Y));
        System.Drawing.Rectangle area = screen.WorkingArea;
        var workArea = new PixelRect(area.Left, area.Top, area.Width, area.Height);
        var overlaySize = new PixelSize((int)Math.Ceiling(ActualWidth), (int)Math.Ceiling(ActualHeight));
        PixelRect placement = OverlayPlacement.PlaceNearCursor(cursor, overlaySize, workArea, margin: 8, offset: 18);
        Left = placement.Left;
        Top = placement.Top;
    }

    private void PositionNearAnchor(PixelRect anchor)
    {
        Forms.Screen screen = Forms.Screen.FromRectangle(new System.Drawing.Rectangle(
            anchor.Left,
            anchor.Top,
            anchor.Width,
            anchor.Height));
        System.Drawing.Rectangle area = screen.WorkingArea;
        var workArea = new PixelRect(area.Left, area.Top, area.Width, area.Height);
        var overlaySize = new PixelSize((int)Math.Ceiling(ActualWidth), (int)Math.Ceiling(ActualHeight));
        PixelRect placement = OverlayPlacement.PlaceNearAnchor(anchor, overlaySize, workArea, margin: 8, offset: 14);
        Left = placement.Left;
        Top = placement.Top;
    }

    private static System.Windows.Media.Brush GetCachedBrush(
        string color,
        System.Windows.Media.Brush fallback,
        ref string? cachedColor,
        ref System.Windows.Media.Brush cachedBrush)
    {
        if (string.Equals(cachedColor, color, StringComparison.Ordinal))
        {
            return cachedBrush;
        }

        cachedColor = color;
        cachedBrush = ToBrush(color, fallback);
        return cachedBrush;
    }

    private static System.Windows.Media.Brush ToBrush(string color, System.Windows.Media.Brush fallback)
    {
        try
        {
            var brush = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString(color)!;
            if (brush.CanFreeze)
            {
                brush.Freeze();
            }

            return brush;
        }
        catch
        {
            return fallback;
        }
    }

    private void SetInteractive(bool isInteractive)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return;
        }

        int style = GetWindowLong(windowHandle, GwlExStyle) | WsExToolWindow;
        if (isInteractive)
        {
            style &= ~WsExTransparent;
            style &= ~WsExNoActivate;
            Focusable = true;
        }
        else
        {
            style |= WsExTransparent | WsExNoActivate;
            Focusable = false;
        }

        SetWindowLong(windowHandle, GwlExStyle, style);
        uint flags = SwpNoMove | SwpNoSize | SwpNoZOrder | SwpFrameChanged;
        if (!isInteractive)
        {
            flags |= SwpNoActivate;
        }

        SetWindowPos(
            windowHandle,
            IntPtr.Zero,
            0,
            0,
            0,
            0,
            flags);
    }

    private void EnableCaptureExclusion()
    {
        if (windowHandle == IntPtr.Zero)
        {
            return;
        }

        if (!SetWindowDisplayAffinity(windowHandle, WindowCaptureAffinity.ExcludeFromCapture))
        {
            _ = SetWindowDisplayAffinity(windowHandle, WindowCaptureAffinity.MonitorOnly);
        }
    }

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr windowHandle, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr windowHandle, int index, int newLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr windowHandle,
        IntPtr windowHandleInsertAfter,
        int x,
        int y,
        int width,
        int height,
        uint flags);

    [DllImport("user32.dll")]
    private static extern bool SetWindowDisplayAffinity(IntPtr windowHandle, WindowCaptureAffinity affinity);
}
