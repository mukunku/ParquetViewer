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
        }

        private void saveAsPngButton_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG image|*.png",
                Title = "Save image as PNG"
            };
            saveFileDialog.ShowDialog();

            if (!string.IsNullOrWhiteSpace(saveFileDialog.FileName))
            {
                this.mainPictureBox.Image.Save(saveFileDialog.FileName);

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
