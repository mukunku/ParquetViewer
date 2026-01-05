using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ParquetViewer.Controls
{
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
                e.Graphics.FillRectangle(new SolidBrush(this.BackColor), 0, 0, this.Width, this.Height);
                e.Graphics.DrawLine(new Pen(base.ForeColor), 30, this.Height / 2, this.Width - 4, this.Height / 2);
            }
            else
            {
                base.OnPaint(e);
            }
        }
    }
}
