using System.IO;
using Forms = System.Windows.Forms;
using Drawing = System.Drawing;
using HoverText.Core.Settings;

namespace HoverText.App.Ui;

public sealed class TrayIconService : IDisposable
{
    private readonly Forms.NotifyIcon notifyIcon;
    private readonly Forms.ToolStripMenuItem ocrItem;
    private readonly Forms.ToolStripMenuItem startupItem;

    public TrayIconService(
        HoverTextSettings settings,
        Action showControlWindow,
        Action openSettings,
        Action toggleOcr,
        Action toggleStartup,
        Action exit)
    {
        ocrItem = new Forms.ToolStripMenuItem("OCR fallback", null, (_, _) => toggleOcr()) { CheckOnClick = false };
        startupItem = new Forms.ToolStripMenuItem("Start with Windows", null, (_, _) => toggleStartup()) { CheckOnClick = false };
        var contextMenu = new Forms.ContextMenuStrip();
        contextMenu.Items.Add(new Forms.ToolStripMenuItem("Show HoverText", null, (_, _) => showControlWindow()));
        contextMenu.Items.Add(new Forms.ToolStripMenuItem("Settings", null, (_, _) => openSettings()));
        contextMenu.Items.Add(ocrItem);
        contextMenu.Items.Add(startupItem);
        contextMenu.Items.Add(new Forms.ToolStripSeparator());
        contextMenu.Items.Add(new Forms.ToolStripMenuItem("Exit", null, (_, _) => exit()));

        notifyIcon = new Forms.NotifyIcon
        {
            Icon = LoadIcon(),
            Text = "HoverText",
            ContextMenuStrip = contextMenu,
            Visible = true
        };
        notifyIcon.MouseUp += (_, e) =>
        {
            if (e.Button == Forms.MouseButtons.Left)
            {
                showControlWindow();
            }
        };
        notifyIcon.DoubleClick += (_, _) => showControlWindow();
        UpdateSettings(settings);
    }

    public void UpdateSettings(HoverTextSettings settings)
    {
        ocrItem.Checked = settings.IsOcrEnabled;
        startupItem.Checked = settings.StartWithWindows;
    }

    public void Dispose()
    {
        notifyIcon.Visible = false;
        notifyIcon.Dispose();
    }

    private static Drawing.Icon LoadIcon()
    {
        string iconPath = AppIcon.IconPath;
        return File.Exists(iconPath) ? new Drawing.Icon(iconPath) : Drawing.SystemIcons.Information;
    }
}
