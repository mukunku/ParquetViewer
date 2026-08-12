using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ParquetViewer.Controls;

public class ThemableToolStripSeperator : ToolStripSeparator
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public new Color BackColor { get; set; } = Color.Transparent;

    /// <remarks>
    /// The default paint event ignores BackColor which is why we have this class and override.
    /// </remarks>
    protected override void OnPaint(PaintEventArgs e)
    {
        if (BackColor != Color.Transparent)
        {
            e.Graphics.FillRectangle(new SolidBrush(BackColor), 0, 0, Width, Height);
            e.Graphics.DrawLine(new Pen(base.ForeColor), 30, Height / 2, Width - 4, Height / 2);
        }
        else
        {
            base.OnPaint(e);
        }
    }
}