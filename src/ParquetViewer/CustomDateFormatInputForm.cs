﻿using ParquetViewer.Controls;
using ParquetViewer.Helpers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ParquetViewer
{
    public partial class CustomDateFormatInputForm : FormBase
    {
        public string UserEnteredDateFormat => this.desiredDateFormatTextBox.Text;

        public CustomDateFormatInputForm()
        {
            InitializeComponent();
        }

        public CustomDateFormatInputForm(string? customDateFormat) : this()
        {
            this.desiredDateFormatTextBox.Text = customDateFormat ?? string.Empty;
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
                if (string.IsNullOrWhiteSpace(this.desiredDateFormatTextBox.Text))
                {
                    this.livePreviewTextBox.Text = string.Empty;
                    this.saveDateFormatButton.Enabled = false;
                }
                else
                {
                    this.livePreviewTextBox.Text = DateTime.Now.ToString(this.desiredDateFormatTextBox.Text);
                    this.saveDateFormatButton.Enabled = true;
                }
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
            this.saveDateFormatButton.Enabled = false; //always start disabled
        }

        //This timer exists to deal with visual bugs
        private void timer_Tick(object sender, EventArgs e)
        {
            //We only wanted to run this once
            this.timer.Enabled = false;

            //HACK: need to widen the form a tiny bit to get rid of the horizontal scrollbar :shrug:
            this.Width += 20;

            //HACK: For some reason resetting the auto scroll position doesn't work in the Load event.
            this.instructionsTableLayoutPanel.AutoScrollPosition = new System.Drawing.Point(0, 0);
        }

        private void saveDateFormatButton_Click(object sender, EventArgs e)
        {
            if (UtilityMethods.IsValidDateFormat(this.desiredDateFormatTextBox.Text))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid date format. Please refer to the documentation for valid date format specifiers.", "Invalid Date Format", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public override void SetTheme(Theme theme)
        {
            if (DesignMode)
            {
                return;
            }

            base.SetTheme(theme);
            this.saveDateFormatButton.ForeColor = Color.Black;
            this.dateFormatDocsLinkLabel.LinkColor = theme.HyperlinkColor;
            this.dateFormatDocsLinkLabel.ActiveLinkColor = theme.ActiveHyperlinkColor;
        }
    }
}
