using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ParquetViewer.Controls;

internal class ProgressButton : Button
{
    private string _text;

    private volatile int _percent;
    private int _invalidateQueued;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new string Text { get => _text; set => _text = value; }

    public ProgressButton() : base()
    {
        //We will render our own text so it's drawn above the progress bar
        _text = base.Text ?? string.Empty;
        base.Text = string.Empty;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Percent
    {
        get => _percent;
        set
        {
            var percent = Math.Clamp(value, 0, 101);

            if (_percent == percent)
                return;

            _percent = percent;

            if (!IsHandleCreated)
                return;

            //Progress can arrive faster than the UI thread can paint, so collapse a burst of updates into a
            //single repaint. Set the flag before queueing; only the caller that wins the exchange posts.
            if (Interlocked.CompareExchange(ref _invalidateQueued, 1, 0) != 0)
                return;

            try
            {
                BeginInvoke(() =>
                {
                    Interlocked.Exchange(ref _invalidateQueued, 0);
                    Invalidate();
                });
            }
            catch (InvalidOperationException)
            {
                //The handle went away between the check above and here
                Interlocked.Exchange(ref _invalidateQueued, 0);
            }
        }
    }

    protected override void OnPaint(PaintEventArgs args)
    {
        base.OnPaint(args);

        const int padding = 4;
        var percent = _percent;

        if (percent > 0)
        {
            var width = (ClientSize.Width - (padding * 2)) * percent / 100f;
            using var brush = new SolidBrush(Color.FromArgb(160, 40, 160, 60));
            args.Graphics.FillRectangle(brush, padding, padding, width, ClientSize.Height - (padding * 2));
        }

        TextRenderer.DrawText(
            args.Graphics,
            Text,
            Font,
            new Point(ClientRectangle.Width, (ClientRectangle.Height / 2) + 1), //Needs +1 to match native alignment
            Enabled ? ForeColor : SystemColors.GrayText,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.NoPrefix);

    }
}
