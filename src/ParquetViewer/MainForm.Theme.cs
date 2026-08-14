using ParquetViewer.Controls;
using ParquetViewer.Helpers;
using System.Drawing;
using System.Windows.Forms;

namespace ParquetViewer;

public partial class MainForm
{
    public override void SetTheme(Theme theme)
    {
        if (DesignMode)
        {
            return;
        }

        base.SetTheme(theme);
        mainGridView.GridTheme = theme;
        mainMenuStrip.BackColor = theme.FormBackgroundColor;
        mainMenuStrip.ForeColor = theme.TextColor;
        foreach (ToolStripItem item in mainMenuStrip.Children())
        {
            //HACK: Small hack to determine if we're in light mode and should use the default paint event
            var shouldUseDefaultSeparatorPaintEvent = !theme.HasToolStripRendererProvider;
            if (item is ThemableToolStripSeperator separator)
            {
                separator.BackColor = shouldUseDefaultSeparatorPaintEvent ? Color.Transparent /*disable custom paint event*/ : theme.FormBackgroundColor;
            }

            item.BackColor = theme.FormBackgroundColor;
            item.ForeColor = theme.TextColor;
        }
        mainStatusStrip.BackColor = theme.FormBackgroundColor;
        mainStatusStrip.ForeColor = theme.TextColor;
        mainGridView.BorderStyle = BorderStyle.Fixed3D;
        searchFilterLabel.LinkColor = theme.HyperlinkColor;
        searchFilterLabel.ActiveLinkColor = theme.ActiveHyperlinkColor;
        runQueryButton.BackColor = Color.White;
        clearFilterButton.BackColor = Color.White;

        mainMenuStrip.Renderer = theme.ToolStripRenderer;
    }
}