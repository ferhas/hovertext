using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HoverText.App.Ui;

namespace HoverText.Tests;

[TestClass]
public sealed class OverlayWindowDesignTests
{
    [TestMethod]
    public void Overlay_body_text_uses_regular_weight()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                AssertOverlayBodyTextWeight();
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

    private static void AssertOverlayBodyTextWeight()
    {
        var window = new OverlayWindow();

        try
        {
            TextBlock displayText = FindChildren<TextBlock>(window)
                .Single(textBlock => textBlock.Name == "DisplayText");

            Assert.AreEqual(FontWeights.Normal, displayText.FontWeight);
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
