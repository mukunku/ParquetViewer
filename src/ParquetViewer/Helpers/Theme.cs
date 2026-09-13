using System;
using System.Drawing;
using System.Windows.Forms;

namespace ParquetViewer.Helpers;

public readonly struct Theme : IEquatable<Theme>
{
    public Color CellBackgroundColor { get; }
    public Color TextColor { get; }
    public Color AlternateRowsCellBackgroundColor { get; } //TODO: This was difficult to implement with byte array columns which sometimes are clickable and sometimes not
    public Color ColumnHeaderColor { get; }
    public Color RowHeaderColor { get; }
    public DataGridViewHeaderBorderStyle RowHeaderBorderStyle { get; }
    public Color GridBackgroundColor { get; }
    public Color GridColor { get; }
    public Color HyperlinkColor { get; }
    public Color CellPlaceholderTextColor { get; }
    public Color SelectionBackColor { get; }
    public Color FormBackgroundColor { get; }
    public Color DisabledTextColor { get; }
    public Color FrozenColumnHeaderColor { get; }
    public Color FrozenCellBackgroundColor { get; }

    private readonly Func<Theme, ToolStripProfessionalRenderer>? _toolStripRendererProvider = null;
    public bool HasToolStripRendererProvider => _toolStripRendererProvider is not null;
    public ToolStripProfessionalRenderer ToolStripRenderer => _toolStripRendererProvider?.Invoke(this) ?? new ToolStripProfessionalRenderer();

    public Color ActiveHyperlinkColor { get; }

    public Theme(
        Color cellBackgroundColor,
        Color textColor,
        Color alternateRowsCellBackgroundColor,
        Color columnHeaderColor,
        Color rowHeaderColor,
        DataGridViewHeaderBorderStyle rowHeaderBorderStyle,
        Color gridBackgroundColor,
        Color gridColor,
        Color hyperlinkColor,
        Color cellPlaceholderTextColor,
        Color selectionBackColor,
        Color formBackgroundColor,
        Func<Theme, ToolStripProfessionalRenderer>? toolStripRendererProvider,
        Color activeHyperlinkColor,
        Color disabledTextColor,
        Color frozenColumnHeaderColor,
        Color frozenCellBackgroundColor)
    {
        CellBackgroundColor = cellBackgroundColor;
        TextColor = textColor;
        AlternateRowsCellBackgroundColor = alternateRowsCellBackgroundColor;
        ColumnHeaderColor = columnHeaderColor;
        RowHeaderColor = rowHeaderColor;
        RowHeaderBorderStyle = rowHeaderBorderStyle;
        GridBackgroundColor = gridBackgroundColor;
        GridColor = gridColor;
        HyperlinkColor = hyperlinkColor;
        CellPlaceholderTextColor = cellPlaceholderTextColor;
        SelectionBackColor = selectionBackColor;
        FormBackgroundColor = formBackgroundColor;
        _toolStripRendererProvider = toolStripRendererProvider;
        ActiveHyperlinkColor = activeHyperlinkColor;
        DisabledTextColor = disabledTextColor;
        FrozenColumnHeaderColor = frozenColumnHeaderColor;
        FrozenCellBackgroundColor = frozenCellBackgroundColor;
    }

    public static Theme DarkModeTheme => new(
        Color.FromArgb(13, 17, 23),
        Color.FromArgb(240, 246, 252),
        Color.FromArgb(21, 27, 35),
        Color.FromArgb(30, 34, 38),
        Color.FromArgb(30, 34, 38),
        DataGridViewHeaderBorderStyle.Single,
        Color.FromArgb(33, 33, 33),
        Color.FromArgb(61, 68, 77),
        Color.FromArgb(68, 147, 248),
        Color.FromArgb(145, 152, 161),
        Color.FromArgb(64, 129, 201),
        Color.FromArgb(44, 44, 44),
        (theme) => { return new DarkModeToolStripRenderer(theme); },
        Color.LightGray,
        Color.DarkGray,
        Color.FromArgb(33, 37, 63),
        Color.FromArgb(40, 44, 48)
        );

    public static Theme LightModeTheme => new(
        SystemColors.Window,
        SystemColors.WindowText,
        SystemColors.Window,
        SystemColors.ControlLight,
        SystemColors.Control,
        DataGridViewHeaderBorderStyle.Raised,
        SystemColors.ControlDark,
        SystemColors.WindowFrame,
        Color.Navy,
        SystemColors.ActiveCaptionText,
        SystemColors.Highlight,
        SystemColors.Control,
        null,
        Color.Red,
        Color.DarkGray,
        SystemColors.InactiveCaption,
        SystemColors.InactiveBorder
        );

    public bool Equals(Theme other) => GetHashCode() == other.GetHashCode(); //Not perfect but good enough
    public static bool operator ==(Theme first, Theme second) => first.Equals(second);
    public static bool operator !=(Theme first, Theme second) => !first.Equals(second);
    public override bool Equals(object? obj) => obj is Theme theme && Equals(theme);
    public override int GetHashCode()
    {
        var hashcode = new HashCode();
        hashcode.Add(CellBackgroundColor);
        hashcode.Add(TextColor);
        hashcode.Add(AlternateRowsCellBackgroundColor);
        hashcode.Add(ColumnHeaderColor);
        hashcode.Add(RowHeaderColor);
        hashcode.Add(RowHeaderBorderStyle);
        hashcode.Add(GridBackgroundColor);
        hashcode.Add(GridColor);
        hashcode.Add(HyperlinkColor);
        hashcode.Add(CellPlaceholderTextColor);
        hashcode.Add(SelectionBackColor);
        hashcode.Add(FormBackgroundColor);
        hashcode.Add(ActiveHyperlinkColor);
        hashcode.Add(DisabledTextColor);
        hashcode.Add(FrozenColumnHeaderColor);
        hashcode.Add(FrozenCellBackgroundColor);
        return hashcode.ToHashCode();
    }
}

internal class DarkModeToolStripRenderer : ToolStripProfessionalRenderer
{
    private readonly Theme _theme;

    public DarkModeToolStripRenderer(Theme theme, ProfessionalColorTable? colorTable = null)
        : base(colorTable ?? new DarkModeColorTable(theme))
    {
        _theme = theme;
    }

    /// <summary>
    /// Colors the arrow for toolstrip menu items that have sub-items.
    /// </summary>
    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        e.ArrowColor = _theme.TextColor;
        base.OnRenderArrow(e);
    }
}

internal class DarkModeColorTable : ProfessionalColorTable
{
    private readonly Color _borderColor = Color.FromArgb(171, 173, 179);
    private readonly Theme _theme;

    public DarkModeColorTable(Theme theme)
    {
        base.UseSystemColors = false; //Not sure if this is needed but adding anyway
        _theme = theme;
    }

    #region Border color
    public override Color MenuBorder => Color.Transparent;
    public override Color MenuItemBorder => _borderColor;
    public override Color ToolStripDropDownBackground => _borderColor;
    public override Color ImageMarginGradientBegin => _borderColor;
    public override Color ImageMarginGradientMiddle => _borderColor;
    public override Color ImageMarginGradientEnd => _borderColor;
    #endregion

    #region Highlighted and Selected background color
    public override Color MenuItemSelectedGradientBegin => _theme.SelectionBackColor;
    public override Color MenuItemSelectedGradientEnd => _theme.SelectionBackColor;
    public override Color MenuItemPressedGradientBegin => _theme.SelectionBackColor;
    public override Color MenuItemPressedGradientMiddle => _theme.SelectionBackColor;
    public override Color MenuItemPressedGradientEnd => _theme.SelectionBackColor;
    #endregion

    #region Dropdown hover colors
    public override Color ButtonSelectedGradientBegin => _theme.SelectionBackColor;
    public override Color ButtonSelectedGradientMiddle => _theme.SelectionBackColor;
    public override Color ButtonSelectedGradientEnd => _theme.SelectionBackColor;
    #endregion
}