using ParquetViewer.Helpers;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ParquetViewer
{
    public partial class CustomDateFormatInputForm : Form
    {
        public CustomDateFormatInputForm()
        {
            InitializeComponent();
        }

        public void dateFormatDocsLinkLabel_Clicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(Constants.DateFormatDocsURL) { UseShellExecute = true });
        }

        private void cancelButton_Clicked(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void desiredDateFormatTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                this.livePreviewTextBox.Text = DateTime.Now.ToString(this.desiredDateFormatTextBox.Text);
                this.saveDateFormatButton.Enabled = true;
            }
            catch (Exception)
            {
                this.livePreviewTextBox.Text = "invalid date format";
                this.saveDateFormatButton.Enabled = false;
            }
        }

        private void CustomDateFormatInputForm_Load(object sender, EventArgs e)
        {
            this.timer.Enabled = true;
        }

        //HACK: For some reason resetting the auto scroll position doesn't work in the Load event.
        //So I'm using this timer as a once-off setTimeout() to reset the scroll position.
        private void timer_Tick(object sender, EventArgs e)
        {
            this.timer.Enabled = false;

            

            //Also need to widen the form a tiny bit to get rid of the horizontal scrollbar :shrug:
            this.Width += 20;

            //reset scroll position
            this.instructionsTableLayoutPanel.AutoScrollPosition = new System.Drawing.Point(0, 0);
        }
    }
}
