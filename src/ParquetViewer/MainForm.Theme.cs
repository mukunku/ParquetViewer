using ParquetViewer.Controls;
using ParquetViewer.Helpers;
using System.Drawing;
using System.Windows.Forms;

namespace ParquetViewer
{
    public partial class MainForm
    {
        public override void SetTheme(Theme theme)
        {
            if (DesignMode)
            {
                return;
            }

            base.SetTheme(theme);
            this.mainGridView.GridTheme = theme;
            this.mainMenuStrip.BackColor = theme.FormBackgroundColor;
            this.mainMenuStrip.ForeColor = theme.TextColor;
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
            this.mainStatusStrip.BackColor = theme.FormBackgroundColor;
            this.mainStatusStrip.ForeColor = theme.TextColor;
            this.mainGridView.BorderStyle = BorderStyle.Fixed3D;
            this.searchFilterLabel.LinkColor = theme.HyperlinkColor;
            this.searchFilterLabel.ActiveLinkColor = theme.ActiveHyperlinkColor;
            this.runQueryButton.BackColor = Color.White;
            this.clearFilterButton.BackColor = Color.White;

            this.mainMenuStrip.Renderer = theme.ToolStripRenderer;
        }
    }
}