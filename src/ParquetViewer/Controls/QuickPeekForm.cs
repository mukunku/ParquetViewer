using ParquetViewer.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParquetViewer.Controls
{
    public partial class QuickPeekForm : FormBase
    {
        private readonly string originalTitle = string.Empty;

        private string titleSuffix = string.Empty;
        public string TitleSuffix
        {
            get => titleSuffix;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.titleSuffix = value;
                    this.Text = $"{originalTitle} - {titleSuffix}";
                }
                else
                {
                    this.titleSuffix = string.Empty;
                    this.Text = originalTitle;
                }
            }
        }

        public Guid UniqueTag { get; set; }
        public int SourceRowIndex { get; set; }
        public int SourceColumnIndex { get; set; }

        public event EventHandler<TakeMeBackEventArgs>? TakeMeBackEvent;

        public QuickPeekForm()
        {
            InitializeComponent();
            this.originalTitle = this.Text;
            this.closeWindowButton.Size = new Size(1, 1); //hide the close button. We only use it as the form's `CloseButton` so the user can close the window by hitting ESC.
            MaximumSize = new Size(Screen.FromControl(this).WorkingArea.Width, Screen.FromControl(this).WorkingArea.Height); //In case we have really large images
        }

        public QuickPeekForm(string titleSuffix, DataTable data, Guid uniqueTag, int sourceRowIndex, int sourceColumnIndex) : this()
        {
            this.TitleSuffix = titleSuffix;
            this.UniqueTag = uniqueTag;
            this.SourceRowIndex = sourceRowIndex;
            this.SourceColumnIndex = sourceColumnIndex;

            this.mainTableLayoutPanel.Controls.Remove(this.mainPictureBox);
            this.mainTableLayoutPanel.Controls.Remove(this.saveImageToFileButton);
            this.mainTableLayoutPanel.RowCount -= 1; //Remove the bottom row to get rid of the image save button
            this.mainTableLayoutPanel.SetColumnSpan(this.mainGridView, 2);
            this.mainPictureBox = null;

            this.mainGridView.DataSource = data ?? throw new ArgumentNullException(nameof(data));
            this.mainGridView.ClearSelection();
        }

        public QuickPeekForm(string titleSuffix, Image image, Guid uniqueTag, int sourceRowIndex, int sourceColumnIndex) : this()
        {
            this.TitleSuffix = titleSuffix;
            this.UniqueTag = uniqueTag;
            this.SourceRowIndex = sourceRowIndex;
            this.SourceColumnIndex = sourceColumnIndex;

            this.mainTableLayoutPanel.Controls.Remove(this.mainGridView);
            this.mainGridView = null;

            this.mainPictureBox.Image = image ?? throw new ArgumentNullException(nameof(image));
            this.mainTableLayoutPanel.SetColumn(this.mainPictureBox, 0);
            this.mainTableLayoutPanel.SetColumnSpan(this.mainPictureBox, 2);
        }

        private void QuickPeekForm_Load(object sender, EventArgs e)
        {
            if (this.mainGridView is not null)
            {
                //Make the form as wide as the number of columns
                var width = this.mainGridView.RowHeadersWidth + 26; // needed to add this magic number in my testing
                foreach (DataGridViewColumn column in this.mainGridView.Columns)
                {
                    width += column.Width;
                }
                this.Width = Math.Min(Math.Max(width, 280), 900); //900 pixel max seems reasonable, right?

                if (this.mainGridView.Rows.Count == 1)
                {
                    this.Height = 200;
                }
            }
            else if (this.mainPictureBox is not null)
            {
                this.Text += $" (Dimensions: {this.mainPictureBox.Image.PhysicalDimension.Width} x {this.mainPictureBox.Image.PhysicalDimension.Height})";
                this.Text += $" (Type: {this.mainPictureBox.Image.RawFormat})";

                this.Width = Math.Max(Math.Min((int)(Screen.FromControl(this).WorkingArea.Width / 1.8), this.mainPictureBox.Image.Width), 400);
                this.Height = Math.Max(Math.Min((int)(Screen.FromControl(this).WorkingArea.Height / 1.8), this.mainPictureBox.Image.Height), 400);

                this.Size = this.mainPictureBox.RenderedSize() + new Size(0, 80);

                this.saveImageToFileButton.Text = $"Save as {this.mainPictureBox.Image.RawFormat}";
            }
            else
            {
                throw new ApplicationException("This should never happen");
            }

            this.Location = new Point(Cursor.Position.X + 5, Cursor.Position.Y);

            //Keep the form on the screen
            var xOverflow = this.Left + this.Width - Screen.FromControl(this).WorkingArea.Width;
            if (xOverflow > 0)
                this.Left -= xOverflow;

            var yOverflow = this.Top + this.Height - Screen.FromControl(this).WorkingArea.Height;
            if (yOverflow > 0)
                this.Top -= yOverflow;
        }

        private void TakeMeBackLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                TakeMeBackEvent?.Invoke(this, new TakeMeBackEventArgs(this.UniqueTag, this.SourceRowIndex, this.SourceColumnIndex));
        }

        public void DisableTakeMeBackLink()
        {
            this.takeMeBackLinkLabel.Text = "<<< can't go back";
        }

        private void CloseWindowButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void saveImageToFileButton_Click(object sender, EventArgs e)
        {
            using var saveFileDialog = new SaveFileDialog
            {
                Filter = $"{this.mainPictureBox.Image.RawFormat.ToString().ToUpperInvariant()} image|*.{this.mainPictureBox.Image.RawFormat.ToString().ToLowerInvariant()}",
                Title = $"Save image as {this.mainPictureBox.Image.RawFormat.ToString().ToUpperInvariant()}"
            };

            saveFileDialog.ShowDialog();

            if (!string.IsNullOrWhiteSpace(saveFileDialog.FileName))
            {
                var bitmap = new Bitmap(this.mainPictureBox.Image);
                bitmap.Save(saveFileDialog.FileName, this.mainPictureBox.Image.RawFormat);

                MessageBox.Show($"Image saved to {saveFileDialog.FileName}", "Save complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void copyToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.mainPictureBox.Cursor = Cursors.WaitCursor;
                Clipboard.SetImage(this.mainPictureBox.Image);
                await Task.Delay(100); //allow cursor to change
            }
            finally
            {
                this.mainPictureBox.Cursor = Cursors.Default;
            }
        }

        public override void SetTheme(Theme theme)
        {
            if (DesignMode)
            {
                return;
            }

            base.SetTheme(theme);
            if (this.mainGridView is not null)
                this.mainGridView.GridTheme = theme;

            this.saveImageToFileButton.ForeColor = Color.Black;
            this.takeMeBackLinkLabel.LinkColor = theme.HyperlinkColor;
            this.takeMeBackLinkLabel.ActiveLinkColor = theme.TextColor;
        }
    }

    public class TakeMeBackEventArgs(Guid uniqueTag, int sourceRowIndex, int sourceColumnIndex) : EventArgs
    {
        public Guid UniqueTag { get; } = uniqueTag;
        public int SourceRowIndex { get; } = sourceRowIndex;
        public int SourceColumnIndex { get; } = sourceColumnIndex;
    }
}
