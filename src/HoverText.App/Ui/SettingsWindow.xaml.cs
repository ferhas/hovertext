using System.Windows;
using HoverText.Core.Settings;

namespace HoverText.App.Ui;

public partial class SettingsWindow : Window
{
    private readonly Func<HoverTextSettings, Task> saveAsync;
    private HoverTextSettings settings;

    public SettingsWindow(HoverTextSettings settings, Func<HoverTextSettings, Task> saveAsync)
    {
        InitializeComponent();
        Icon = AppIcon.LoadImageSource();
        this.settings = settings;
        this.saveAsync = saveAsync;
        TriggerKeyBox.ItemsSource = Enum.GetValues<TriggerKey>();
        LoadSettings(settings);
    }

    private void LoadSettings(HoverTextSettings value)
    {
        TriggerKeyBox.SelectedItem = value.TriggerKey;
        FontSizeSlider.Value = value.FontSize;
        PollIntervalSlider.Value = value.PollIntervalMilliseconds;
        ForegroundBox.Text = value.Foreground;
        BackgroundBox.Text = value.Background;
        OcrBox.IsChecked = value.IsOcrEnabled;
        StartupBox.IsChecked = value.StartWithWindows;
        UpdateSliderLabels();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        settings = settings with
        {
            TriggerKey = (TriggerKey)(TriggerKeyBox.SelectedItem ?? TriggerKey.Alt),
            FontSize = (int)FontSizeSlider.Value,
            PollIntervalMilliseconds = (int)PollIntervalSlider.Value,
            Foreground = ForegroundBox.Text,
            Background = BackgroundBox.Text,
            IsOcrEnabled = OcrBox.IsChecked == true,
            StartWithWindows = StartupBox.IsChecked == true
        };

        await saveAsync(settings);
        Close();
    }

    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateSliderLabels();
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
        Close();
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
}
