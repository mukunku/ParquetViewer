using System;
using System.Drawing;
using System.Windows.Forms;

namespace ParquetViewer.Controls;

/// <summary>
/// Only exists to allow us to show tooltips on disabled checkbox elements.
/// https://stackoverflow.com/q/1732140/1458738
/// </summary>
public class CheckboxWithTooltip : StylableCheckBox
{
    private readonly ToolTip _tooltip = new();
    private bool _tooltipShown = false;

    public CheckboxWithTooltip(Control parent) : base()
    {
        ArgumentNullException.ThrowIfNull(parent);
        parent.MouseMove += (_, e) =>
        {
            var control = parent?.GetChildAtPoint(e.Location);
            if (control == this)
            {
                if (!Enabled && !_tooltipShown)
                {
                    //It's important the tooltip is outside the checkbox control's bounds; otherwise the MouseLeave event handler doesn't work very well.
                    var point = new Point(e.Location.X, (int)(Height * 1.5));

                    _tooltip.Show(_tooltip.GetToolTip(this), this, point);
                    _tooltipShown = true;
                }
            }
            else if (_tooltipShown)
            {
                _tooltipShown = false;
                _tooltip.Hide(this);
            }
        };
        parent.MouseLeave += (_, _) =>
        {
            _tooltipShown = false;
            if (!IsDisposed)
                _tooltip.Hide(this);
        };
    }

    public void SetTooltip(string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);

        _tooltip.SetToolTip(this, text);
    }
}