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
            try
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
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $"Something went wrong. Details:{Environment.NewLine}{ex}",
                    "Save as PNG error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void copyToClipboardButton_Click(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $"Could not copy the image to your clipboard. Details:{Environment.NewLine}{ex}",
                    "Copy error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
