using System.Windows;

namespace HoverText.App.Ui;

public partial class ControlWindow : Window
{
    private readonly Action openSettings;
    private readonly Action exit;
    private bool isExiting;

    public ControlWindow(Action openSettings, Action exit)
    {
        InitializeComponent();
        Icon = AppIcon.LoadImageSource();
        this.openSettings = openSettings;
        this.exit = exit;
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        openSettings();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        ExitApplication();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        base.OnClosing(e);
        if (!e.Cancel && !isExiting)
        {
            ExitApplication();
        }
    }

    private void ExitApplication()
    {
        if (isExiting)
        {
            return;
        }

        isExiting = true;
        exit();
    }
}
