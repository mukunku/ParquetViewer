using ParquetViewer.Helpers;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParquetViewer.Controls
{
    public partial class ImagePreviewForm : Form
    {
        public Image PreviewImage
        {
            get
            {
                return this.mainPictureBox.Image;
            }
            set
            {
                this.mainPictureBox.Image = value;
            }
        }

        public ImagePreviewForm()
        {
            InitializeComponent();
            MaximumSize = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        }

        private void ImagePreviewForm_Load(object sender, EventArgs e)
        {
            Location = new Point(Cursor.Position.X + 5, Cursor.Position.Y);

            this.Text += $" (Dimensions: {this.PreviewImage.PhysicalDimension.Width} x {this.PreviewImage.PhysicalDimension.Height})";
            this.Text += $" (Type: {this.PreviewImage.RawFormat})";

            this.Width = Math.Max(Math.Min((int)(Screen.PrimaryScreen.Bounds.Width / 1.8), this.mainPictureBox.Image.Width), 400);
            this.Height = Math.Max(Math.Min((int)(Screen.PrimaryScreen.Bounds.Height / 1.8), this.mainPictureBox.Image.Height), 400);

            this.Size = this.mainPictureBox.RenderedSize() + new Size(0, 60);

            this.saveAsPngButton.Text = $"Save as {this.PreviewImage.RawFormat}";
        }

        private void saveAsPngButton_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = $"{this.PreviewImage.RawFormat.ToString().ToUpperInvariant()} image|*.{this.PreviewImage.RawFormat.ToString().ToLowerInvariant()}",
                Title = $"Save image as {this.PreviewImage.RawFormat.ToString().ToUpperInvariant()}"
            };
            saveFileDialog.ShowDialog();

            if (!string.IsNullOrWhiteSpace(saveFileDialog.FileName))
            {
                var bitmap = new Bitmap(this.mainPictureBox.Image);
                bitmap.Save(saveFileDialog.FileName, this.PreviewImage.RawFormat);

                MessageBox.Show($"Image saved to {saveFileDialog.FileName}", "Save complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void copyToClipboardButton_Click(object sender, EventArgs e)
        {
            Clipboard.SetImage(this.mainPictureBox.Image);
            if (sender is Button button)
            {
                string buttonText = button.Text;
                button.Text = "Copied!";
                button.Enabled = false;
                await Task.Delay(2000);
                button.Enabled = true;
                button.Text = buttonText;
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
