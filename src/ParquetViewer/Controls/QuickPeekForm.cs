using System;
using System.Drawing;
using System.Windows.Forms;

namespace ParquetViewer.Controls
{
    public partial class QuickPeekForm : Form
    {
        private string originalTitle = string.Empty;

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

        private object data = new();
        public object Data
        {
            get => data;
            set
            {
                this.data = value;
                this.mainGridView.DataSource = value;
                this.mainGridView.ClearSelection();
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
        }

        public QuickPeekForm(string? titleSuffix, object data, Guid uniqueTag, int sourceRowIndex, int sourceColumnIndex) : this()
        {
            this.TitleSuffix = titleSuffix ?? string.Empty;
            this.Data = data ?? throw new ArgumentNullException(nameof(data));
            this.UniqueTag = uniqueTag;
            this.SourceRowIndex = sourceRowIndex;
            this.SourceColumnIndex = sourceColumnIndex;
        }

        private void QuickPeakForm_Load(object sender, EventArgs e)
        {
            var width = 0;
            if (this.mainGridView is not null)
            {
                width = this.mainGridView.RowHeadersWidth + 26; /* magic number? */
                foreach (DataGridViewColumn column in this.mainGridView.Columns)
                {
                    width += column.Width;
                }
            }
            this.Width = Math.Min(Math.Max(width, 280), 900); //900 pixel max seems reasonable, right?
            this.Location = new Point(Cursor.Position.X + 5, Cursor.Position.Y);
        }

        private void TakeMeBackLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                TakeMeBackEvent?.Invoke(this, new TakeMeBackEventArgs(this.UniqueTag, this.SourceRowIndex, this.SourceColumnIndex));
        }

        public void TakeMeBackLinkDisable()
        {
            this.takeMeBackLinkLabel.Text = "<<< can't go back";
        }

        private void CloseWindowButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    public class TakeMeBackEventArgs : EventArgs
    {
        public Guid UniqueTag { get; }
        public int SourceRowIndex { get; }
        public int SourceColumnIndex { get; }

        public TakeMeBackEventArgs(Guid uniqueTag, int sourceRowIndex, int sourceColumnIndex)
        {
            this.UniqueTag = uniqueTag;
            this.SourceRowIndex = sourceRowIndex;
            this.SourceColumnIndex = sourceColumnIndex;
        }
    }
}
