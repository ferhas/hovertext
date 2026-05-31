using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Forms = System.Windows.Forms;
using HoverText.Core.Overlay;
using HoverText.Core.Probing;
using HoverText.Core.Settings;

namespace HoverText.App.Ui;

public partial class OverlayWindow : Window
{
    private const int GwlExStyle = -20;
    private const int WsExTransparent = 0x00000020;
    private const int WsExToolWindow = 0x00000080;
    private const int WsExNoActivate = 0x08000000;

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
        ApplySettings(settings);
        SourceLabel.Text = result.Source switch
        {
            ProbeSource.UiAutomation => "UI Automation",
            ProbeSource.Ocr => "OCR",
            ProbeSource.Magnifier => "Magnifier",
            _ => "HoverText"
        };

        ApplyDisplayKind(result.DisplayKind, settings);

        if (result.Source == ProbeSource.Magnifier && magnifierCapture is not null)
        {
            MagnifierImage.Source = magnifierCapture;
            MagnifierImage.Visibility = Visibility.Visible;
            if (result.Magnifier is not null)
            {
                MagnifierImage.Width = result.Magnifier.CaptureArea.Width * result.Magnifier.Scale;
                MagnifierImage.Height = result.Magnifier.CaptureArea.Height * result.Magnifier.Scale;
            }
        }
        else
        {
            MagnifierImage.Source = null;
            MagnifierImage.Visibility = Visibility.Collapsed;
            MagnifierImage.Width = double.NaN;
            MagnifierImage.Height = double.NaN;
        }

        DisplayText.Text = result.DisplayKind == ProbeDisplayKind.Input && string.IsNullOrWhiteSpace(result.DisplayText)
            ? " "
            : result.DisplayText;
        DisplayText.Visibility = string.IsNullOrWhiteSpace(DisplayText.Text) && result.DisplayKind != ProbeDisplayKind.Input
            ? Visibility.Collapsed
            : Visibility.Visible;
        UpdateLayout();
        if (result.DisplayKind == ProbeDisplayKind.Input && result.AnchorBounds is not null)
        {
            PositionNearAnchor(result.AnchorBounds.Value);
        }
        else
        {
            PositionNearCursor(cursor);
        }

        if (!IsVisible)
        {
            Show();
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        IntPtr handle = new WindowInteropHelper(this).Handle;
        int style = GetWindowLong(handle, GwlExStyle);
        SetWindowLong(handle, GwlExStyle, style | WsExTransparent | WsExToolWindow | WsExNoActivate);
    }

    private void ApplySettings(HoverTextSettings settings)
    {
        DisplayText.FontSize = settings.FontSize;
        DisplayText.Foreground = ToBrush(settings.Foreground, System.Windows.Media.Brushes.White);
        SourceLabel.Foreground = ToBrush(settings.Foreground, System.Windows.Media.Brushes.WhiteSmoke);
        Shell.Background = ToBrush(settings.Background, System.Windows.Media.Brushes.Black);
    }

    private void ApplyDisplayKind(ProbeDisplayKind displayKind, HoverTextSettings settings)
    {
        Shell.CornerRadius = new CornerRadius(8);
        Shell.BorderThickness = new Thickness(1);
        Shell.MinWidth = 0;
        Shell.MinHeight = 0;
        SourceLabel.Visibility = Visibility.Visible;

        if (displayKind == ProbeDisplayKind.Input)
        {
            SourceLabel.Text = "Input";
            SourceLabel.Visibility = Visibility.Collapsed;
            Shell.Background = ToBrush(settings.Background, System.Windows.Media.Brushes.Black);
            Shell.BorderBrush = System.Windows.Media.Brushes.DeepSkyBlue;
            Shell.BorderThickness = new Thickness(2);
            Shell.CornerRadius = new CornerRadius(10);
            Shell.MinWidth = 560;
            Shell.MinHeight = 132;
            DisplayText.Foreground = ToBrush(settings.Foreground, System.Windows.Media.Brushes.White);
            DisplayText.FontSize = Math.Max(settings.FontSize, 76);
            return;
        }

        if (displayKind == ProbeDisplayKind.Tooltip)
        {
            SourceLabel.Text = "Tip";
            Shell.BorderBrush = System.Windows.Media.Brushes.DeepSkyBlue;
        }
        else
        {
            Shell.BorderBrush = ToBrush(settings.Foreground, System.Windows.Media.Brushes.White);
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
        Forms.Screen screen = Forms.Screen.FromPoint(new System.Drawing.Point(anchor.Left + anchor.Width / 2, anchor.Top + anchor.Height / 2));
        System.Drawing.Rectangle area = screen.WorkingArea;
        var workArea = new PixelRect(area.Left, area.Top, area.Width, area.Height);
        var overlaySize = new PixelSize((int)Math.Ceiling(ActualWidth), (int)Math.Ceiling(ActualHeight));
        PixelRect placement = OverlayPlacement.PlaceNearAnchor(anchor, overlaySize, workArea, margin: 8, offset: 14);
        Left = placement.Left;
        Top = placement.Top;
    }

    private static System.Windows.Media.Brush ToBrush(string color, System.Windows.Media.Brush fallback)
    {
        try
        {
            return (System.Windows.Media.Brush)new BrushConverter().ConvertFromString(color)!;
        }
        catch
        {
            return fallback;
        }
    }

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr windowHandle, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr windowHandle, int index, int newLong);
}
