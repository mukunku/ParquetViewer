using ParquetViewer;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

/// <summary>
/// Represents a Windows <see cref="CheckBox"/>.
/// Source: https://github.com/Assorted-Development/winforms-stylable-controls/blob/89ecc4f0a3dcb518965b473981d26cec706994e4/StylableWinFormsControls/StylableWinFormsControls/Controls/StylableCheckBox.cs
/// </summary>
public class StylableCheckBox : CheckBox
{
    /// Sent when the window background must be erased (for example, when a window is resized).
    /// The message is sent to prepare an invalidated portion of a window for painting.
    /// </summary>
    internal const int WM_ERASEBKGND = 0x14;

    private bool _disableCustomRendering;
    private Rectangle _textRectangleValue;

    /// <summary>
    /// Gets or sets the foreground color of the checkbox label if a checkbox is disabled
    /// </summary>
    public Color DisabledForeColor
    {
        get;
        set;
    }

    public StylableCheckBox()
    {
        this._disableCustomRendering = !AppSettings.DarkMode;
        if (!_disableCustomRendering)
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // skip because we draw it.
        if (_disableCustomRendering)
        {
            base.OnPaintBackground(e);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (_disableCustomRendering)
        {
            base.OnPaint(e);
        }
        else
        {
            drawCheckBox(e.Graphics);
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (!_disableCustomRendering)
        {
            // Filter out the WM_ERASEBKGND message since we do that on our own with foreground painting
            if (m.Msg == WM_ERASEBKGND)
            {
                // return 0 (no erasing)
                m.Result = (IntPtr)1;

                return;
            }
        }

        base.WndProc(ref m);
    }

    private void drawCheckBox(Graphics graphics)
    {
        Size glyphSize = CheckBoxRenderer.GetGlyphSize(graphics, getCheckBoxState());

        // Calculate the text bounds, excluding the check box.
        Rectangle textRectangle = getTextRectangle(glyphSize);

        // center box vertically with text, especially necessary for multiline,
        // but align if disabled because the glyph looks slightly different then.
        Rectangle glyphBounds = new(
            ClientRectangle.Location with
            {
                Y = textRectangle.Location.Y +
                    ((textRectangle.Height - textRectangle.Location.Y) / 2) -
                    (glyphSize.Height / 2)
            },
            glyphSize);
        glyphBounds.Inflate(1, 1); //make the checkbox slightly bigger

        // Paint over text since ít might look slightly offset
        // if the calculation between disabled and enabled control positions differ
        // and the previous checkbox doesn't get correctly erased (which happens sometimes)
        // And: Don't do Graphics.Clear, not good with RDP sessions
        using (Brush backBrush = new SolidBrush(BackColor))
        {
            graphics.FillRectangle(backBrush, ClientRectangle);
        }

        if (this.CheckState == CheckState.Indeterminate)
        {
            ControlPaint.DrawMixedCheckBox(graphics, glyphBounds, getButtonState() | ButtonState.Flat);
        }
        else
        {
            ControlPaint.DrawCheckBox(graphics, glyphBounds, getButtonState() | ButtonState.Flat);
        }

        Color textColor = Enabled ? ForeColor : DisabledForeColor;

        TextRenderer.DrawText(
            graphics, Text, Font, textRectangle, textColor,
            TextFormatFlags.VerticalCenter);

        if (Focused && ShowFocusCues)
        {
            ControlPaint.DrawFocusRectangle(graphics, ClientRectangle);
        }
    }

    private Rectangle _oldClientRectangle = Rectangle.Empty;

    private Size _oldGlyphSize = Size.Empty;
    private Rectangle _textRectangle = Rectangle.Empty;

    private Rectangle getTextRectangle(Size glyphSize)
    {
        // don't spend unnecessary time on PInvokes
        if (_oldClientRectangle == ClientRectangle && _oldGlyphSize == glyphSize)
        {
            return _textRectangle;
        }

        _textRectangleValue.X = ClientRectangle.X +
                               glyphSize.Width +
                               3;

        _textRectangleValue.Y = ClientRectangle.Y;
        _textRectangleValue.Width = ClientRectangle.Width - glyphSize.Width;
        _textRectangleValue.Height = ClientRectangle.Height;

        _oldClientRectangle = ClientRectangle;
        _textRectangle = _textRectangleValue;
        _oldGlyphSize = glyphSize;

        return _textRectangleValue;
    }
    /// <summary>
    /// gets the <see cref="ButtonState"/> based on the current <see cref="CheckState"/>
    /// </summary>
    private ButtonState getButtonState()
    {
        return CheckState switch
        {
            CheckState.Checked => Enabled ? ButtonState.Checked : ButtonState.Checked | ButtonState.Inactive,
            CheckState.Unchecked => Enabled ? ButtonState.Normal : ButtonState.Inactive,
            //Downlevel mixed drawing works only if ButtonState.Checked is set
            CheckState.Indeterminate => Enabled ? ButtonState.Checked : ButtonState.Checked | ButtonState.Inactive,
            _ => ButtonState.Normal,
        };
    }
    /// <summary>
    /// gets the <see cref="CheckBoxState"/> based on the current <see cref="CheckState"/>
    /// </summary>
    private CheckBoxState getCheckBoxState()
    {
        return CheckState switch
        {
            CheckState.Checked => Enabled ? CheckBoxState.CheckedNormal : CheckBoxState.CheckedDisabled,
            CheckState.Unchecked => Enabled ? CheckBoxState.UncheckedNormal : CheckBoxState.UncheckedDisabled,
            CheckState.Indeterminate => Enabled ? CheckBoxState.MixedNormal : CheckBoxState.MixedDisabled,
            _ => CheckBoxState.UncheckedNormal,
        };
    }
}