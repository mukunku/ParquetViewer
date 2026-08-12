using ParquetViewer.Helpers;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParquetViewer.Controls;

public partial class QuickPeekForm : FormBase
{
    private readonly string _originalTitle = string.Empty;

    private string _titleSuffix = string.Empty;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string TitleSuffix
    {
        get => _titleSuffix;
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _titleSuffix = value;
                Text = $"{_originalTitle} - {_titleSuffix}";
            }
            else
            {
                _titleSuffix = string.Empty;
                Text = _originalTitle;
            }
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Guid UniqueTag { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int SourceRowIndex { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int SourceColumnIndex { get; set; }

    public event EventHandler<TakeMeBackEventArgs>? TakeMeBackEvent;

    public QuickPeekForm()
    {
        InitializeComponent();
        _originalTitle = Text;
        closeWindowButton.Size = new Size(1, 1); //hide the close button. We only use it as the form's `CloseButton` so the user can close the window by hitting ESC.
        MaximumSize = new Size(Screen.FromControl(this).WorkingArea.Width, Screen.FromControl(this).WorkingArea.Height); //In case we have really large images
    }

    public QuickPeekForm(string titleSuffix, DataTable data, Guid uniqueTag, int sourceRowIndex, int sourceColumnIndex) : this()
    {
        TitleSuffix = titleSuffix;
        UniqueTag = uniqueTag;
        SourceRowIndex = sourceRowIndex;
        SourceColumnIndex = sourceColumnIndex;

        mainTableLayoutPanel.Controls.Remove(mainPictureBox);
        mainTableLayoutPanel.Controls.Remove(saveImageToFileButton);
        mainTableLayoutPanel.RowCount -= 1; //Remove the bottom row to get rid of the image save button
        mainTableLayoutPanel.SetColumnSpan(mainGridView, 2);
        mainPictureBox = null;

        mainGridView.DataSource = data ?? throw new ArgumentNullException(nameof(data));
        mainGridView.ClearSelection();
    }

    public QuickPeekForm(string titleSuffix, Image image, Guid uniqueTag, int sourceRowIndex, int sourceColumnIndex) : this()
    {
        TitleSuffix = titleSuffix;
        UniqueTag = uniqueTag;
        SourceRowIndex = sourceRowIndex;
        SourceColumnIndex = sourceColumnIndex;

        mainTableLayoutPanel.Controls.Remove(mainGridView);
        mainGridView = null;

        mainPictureBox.Image = image ?? throw new ArgumentNullException(nameof(image));
        mainTableLayoutPanel.SetColumn(mainPictureBox, 0);
        mainTableLayoutPanel.SetColumnSpan(mainPictureBox, 2);
    }

    private void QuickPeekForm_Load(object sender, EventArgs e)
    {
        if (mainGridView is not null)
        {
            //Make the form as wide as the number of columns
            var width = mainGridView.RowHeadersWidth + 26; // needed to add this magic number in my testing
            foreach (DataGridViewColumn column in mainGridView.Columns)
            {
                width += column.Width;
            }
            if (mainGridView.Rows.Count > 8) //8 is a magic number... Better than nothing imo
            {
                width += 24; //widen for scrollbar
            }

            Width = Math.Min(Math.Max(width, 280), 900); //900 pixel max seems reasonable, right?

            if (mainGridView.Rows.Count == 1)
            {
                Height = 200;
            }
        }
        else if (mainPictureBox is not null)
        {
            Text += $" ({Resources.Strings.DimensionsText}: {mainPictureBox.Image!.PhysicalDimension.Width} x {mainPictureBox.Image.PhysicalDimension.Height})";
            Text += $" ({Resources.Strings.TypeText}: {mainPictureBox.Image.RawFormat})";

            Width = Math.Max(Math.Min((int)(Screen.FromControl(this).WorkingArea.Width / 1.8), mainPictureBox.Image.Width), 400);
            Height = Math.Max(Math.Min((int)(Screen.FromControl(this).WorkingArea.Height / 1.8), mainPictureBox.Image.Height), 400);

            Size = mainPictureBox.RenderedSize() + new Size(0, 80);

            saveImageToFileButton.Text = Resources.Strings.SaveImageToFileButtonTextFormat.Format(mainPictureBox.Image.RawFormat);
        }
        else
        {
            throw new ApplicationException("QuickPeek form was not created correctly");
        }

        Location = new Point(Cursor.Position.X + 5, Cursor.Position.Y);

        //Keep the form on the screen
        var xOverflow = Left + Width - Screen.FromControl(this).WorkingArea.Width;
        if (xOverflow > 0)
            Left -= xOverflow;

        var yOverflow = Top + Height - Screen.FromControl(this).WorkingArea.Height;
        if (yOverflow > 0)
            Top -= yOverflow;
    }

    private void TakeMeBackLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            TakeMeBackEvent?.Invoke(this, new TakeMeBackEventArgs(UniqueTag, SourceRowIndex, SourceColumnIndex));
    }

    public void DisableTakeMeBackLink()
    {
        takeMeBackLinkLabel.Text = $"<<< {Resources.Strings.CantGoBackLinkButtonText}";
    }

    private void CloseWindowButton_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void saveImageToFileButton_Click(object sender, EventArgs e)
    {
        using var saveFileDialog = new SaveFileDialog
        {
            Filter = $"{mainPictureBox.Image!.RawFormat.ToString().ToUpperInvariant()} image|*.{mainPictureBox.Image.RawFormat.ToString().ToLowerInvariant()}",
            Title = Resources.Strings.SaveImageAsButtonText.Format(mainPictureBox.Image.RawFormat.ToString().ToUpperInvariant())
        };

        saveFileDialog.ShowDialog();

        if (!string.IsNullOrWhiteSpace(saveFileDialog.FileName))
        {
            using var bitmap = new Bitmap(mainPictureBox.Image);
            bitmap.Save(saveFileDialog.FileName, mainPictureBox.Image.RawFormat);

            MessageBox.Show(this,
                Resources.Strings.ImageSavedToDiskMessage.Format(saveFileDialog.FileName),
                Resources.Strings.ImageSavedToDiskTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private async void copyToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
            mainPictureBox.Cursor = Cursors.WaitCursor;
            Clipboard.SetImage(mainPictureBox.Image!);
            await Task.Delay(100); //allow cursor to change
        }
        finally
        {
            mainPictureBox.Cursor = Cursors.Default;
        }
    }

    public override void SetTheme(Theme theme)
    {
        if (DesignMode)
        {
            return;
        }

        base.SetTheme(theme);
        if (mainGridView is not null)
            mainGridView.GridTheme = theme;

        saveImageToFileButton.ForeColor = Color.Black;
        takeMeBackLinkLabel.LinkColor = theme.HyperlinkColor;
        takeMeBackLinkLabel.ActiveLinkColor = theme.TextColor;
    }
}

public class TakeMeBackEventArgs(Guid uniqueTag, int sourceRowIndex, int sourceColumnIndex) : EventArgs
{
    public Guid UniqueTag { get; } = uniqueTag;
    public int SourceRowIndex { get; } = sourceRowIndex;
    public int SourceColumnIndex { get; } = sourceColumnIndex;
}