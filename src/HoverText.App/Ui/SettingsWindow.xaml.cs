using System.Windows;
using HoverText.Core.Platform;
using HoverText.Core.Settings;

namespace HoverText.App.Ui;

public partial class SettingsWindow : Window
{
    private readonly Func<HoverTextSettings, Task> saveAsync;
    private readonly StartupUiPolicy uiPolicy;
    private HoverTextSettings settings;
    private TriggerKey selectedTriggerKey;
    private bool isClosingForReal;

    public SettingsWindow(HoverTextSettings settings, Func<HoverTextSettings, Task> saveAsync, StartupUiPolicy uiPolicy)
    {
        InitializeComponent();
        Icon = AppIcon.LoadImageSource();
        this.settings = settings;
        this.saveAsync = saveAsync;
        this.uiPolicy = uiPolicy;
        LoadSettings(settings);
    }

    public void LoadSettings(HoverTextSettings value)
    {
        selectedTriggerKey = value.TriggerKey;
        FontSizeSlider.Value = value.FontSize;
        PollIntervalSlider.Value = value.PollIntervalMilliseconds;
        ForegroundBox.Text = value.Foreground;
        BackgroundBox.Text = value.Background;
        OcrBox.IsChecked = value.IsOcrEnabled;
        StartupBox.IsChecked = value.StartWithWindows;
        UpdateSliderLabels();
        UpdateTriggerButtons();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        settings = settings with
        {
            TriggerKey = selectedTriggerKey,
            FontSize = (int)FontSizeSlider.Value,
            PollIntervalMilliseconds = (int)PollIntervalSlider.Value,
            Foreground = ForegroundBox.Text,
            Background = BackgroundBox.Text,
            IsOcrEnabled = OcrBox.IsChecked == true,
            StartWithWindows = StartupBox.IsChecked == true
        };

        await saveAsync(settings);
        isClosingForReal = true;
        Close();
    }

    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateSliderLabels();
    }

    private void TriggerButton_Click(object sender, RoutedEventArgs e)
    {
        selectedTriggerKey = sender switch
        {
            FrameworkElement { Name: nameof(ControlButton) } => TriggerKey.Control,
            FrameworkElement { Name: nameof(ShiftButton) } => TriggerKey.Shift,
            _ => TriggerKey.Alt
        };
        UpdateTriggerButtons();
    }

    private void ForegroundPreset_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is string color)
        {
            ForegroundBox.Text = color;
        }
    }

    private void BackgroundPreset_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is string color)
        {
            BackgroundBox.Text = color;
        }
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        LoadSettings(HoverTextSettings.CreateDefault());
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        isClosingForReal = true;
        Close();
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (uiPolicy.HideSettingsWindowOnMinimize && WindowState == WindowState.Minimized)
        {
            Hide();
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (uiPolicy.HideSettingsWindowOnMinimize && !isClosingForReal)
        {
            e.Cancel = true;
            Hide();
            WindowState = WindowState.Normal;
            return;
        }

        base.OnClosing(e);
    }

    private void UpdateSliderLabels()
    {
        if (FontSizeValue is null || PollIntervalValue is null)
        {
            return;
        }

        FontSizeValue.Text = $"{(int)FontSizeSlider.Value}px";
        PollIntervalValue.Text = $"{(int)PollIntervalSlider.Value} ms";
    }

    private void UpdateTriggerButtons()
    {
        if (AltButton is null || ControlButton is null || ShiftButton is null)
        {
            return;
        }

        SetTriggerButtonState(AltButton, selectedTriggerKey == TriggerKey.Alt);
        SetTriggerButtonState(ControlButton, selectedTriggerKey == TriggerKey.Control);
        SetTriggerButtonState(ShiftButton, selectedTriggerKey == TriggerKey.Shift);
    }

    private static void SetTriggerButtonState(System.Windows.Controls.Button button, bool isSelected)
    {
        button.Background = isSelected
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(8, 145, 178))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 41, 59));
        button.BorderBrush = isSelected
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 211, 238))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(71, 85, 105));
    }
}
