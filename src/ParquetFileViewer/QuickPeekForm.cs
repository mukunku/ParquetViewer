using System;
using System.Drawing;
using System.Windows.Forms;

namespace ParquetFileViewer
{
    public partial class QuickPeekForm : Form
    {
        private string originalTitle;

        private string titleSuffix;
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

        private object data;
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

        public int UniqueTag { get; set; }
        public int SourceRowIndex { get; set; }
        public int SourceColumnIndex { get; set; }

        public event EventHandler<TakeMeBackEventArgs> TakeMeBackEvent;

        public QuickPeekForm()
        {
            InitializeComponent();
            this.originalTitle = this.Text;
            this.closeWindowButton.Size = new Size(1, 1); //hide the close button. We only use it as the form's `CloseButton` so the user can close the window by hitting ESC.
        }

        public QuickPeekForm(string titleSuffix, object data, int uniqueTag, int sourceRowIndex, int sourceColumnIndex) : this()
        {
            this.TitleSuffix = titleSuffix;
            this.Data = data;
            this.UniqueTag = uniqueTag;
            this.SourceRowIndex = sourceRowIndex;
            this.SourceColumnIndex = sourceColumnIndex;
        }

        private void QuickPeakForm_Load(object sender, EventArgs e)
        {
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
        public int UniqueTag { get; }
        public int SourceRowIndex { get; }
        public int SourceColumnIndex { get; }

        public TakeMeBackEventArgs(int uniqueTag, int sourceRowIndex, int sourceColumnIndex)
        {
            this.UniqueTag = uniqueTag;
            this.SourceRowIndex = sourceRowIndex;
            this.SourceColumnIndex = sourceColumnIndex;
        }
    }
}
