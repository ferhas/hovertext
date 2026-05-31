using System.Windows;
using System.Windows.Controls;
using HoverText.App.Ui;
using HoverText.Core.Platform;
using HoverText.Core.Settings;

namespace HoverText.Tests;

[TestClass]
public sealed class SettingsWindowDesignTests
{
    [TestMethod]
    public void Settings_window_groups_options_into_polished_sections()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                AssertSettingsWindowSections();
            }
            catch (Exception ex)
            {
                threadException = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (threadException is not null)
        {
            throw threadException;
        }
    }

    private static void AssertSettingsWindowSections()
    {
        var window = new SettingsWindow(
            HoverTextSettings.CreateDefault(),
            _ => Task.CompletedTask,
            StartupUiPolicy.CreateDefault());

        try
        {
            string[] textBlocks = FindChildren<TextBlock>(window)
                .Select(textBlock => textBlock.Text)
                .ToArray();

            CollectionAssert.IsSubsetOf(
                new[] { "快捷触发", "显示外观", "识别能力", "系统偏好" },
                textBlocks);
        }
        finally
        {
            window.Close();
        }
    }

    private static IEnumerable<T> FindChildren<T>(DependencyObject parent)
        where T : DependencyObject
    {
        foreach (object child in LogicalTreeHelper.GetChildren(parent))
        {
            if (child is T typedChild)
            {
                yield return typedChild;
            }

            if (child is DependencyObject dependencyChild)
            {
                foreach (T descendant in FindChildren<T>(dependencyChild))
                {
                    yield return descendant;
                }
            }
        }
    }
}
