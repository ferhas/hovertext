using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HoverText.App.Ui;

public static class AppIcon
{
    public static string IconPath => Path.Combine(AppContext.BaseDirectory, "Assets", "hovertext.ico");

    public static ImageSource? LoadImageSource()
    {
        if (!File.Exists(IconPath))
        {
            return null;
        }

        var image = BitmapFrame.Create(new Uri(IconPath, UriKind.Absolute));
        image.Freeze();
        return image;
    }
}
