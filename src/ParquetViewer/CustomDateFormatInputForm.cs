using ParquetViewer.Controls;
using ParquetViewer.Helpers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ParquetViewer;

public partial class CustomDateFormatInputForm : FormBase
{
    public string UserEnteredDateFormat => desiredDateFormatTextBox.Text;

    public CustomDateFormatInputForm()
    {
        InitializeComponent();
    }

    public CustomDateFormatInputForm(string? customDateFormat) : this()
    {
        desiredDateFormatTextBox.Text = customDateFormat ?? string.Empty;
    }

    public void dateFormatDocsLinkLabel_Clicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        Process.Start(new ProcessStartInfo(Resources.Strings.DotNetDateFormatHelpUrl) { UseShellExecute = true });
    }

    private void cancelButton_Clicked(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void desiredDateFormatTextBox_TextChanged(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(desiredDateFormatTextBox.Text))
            {
                livePreviewTextBox.Text = string.Empty;
                saveDateFormatButton.Enabled = false;
            }
            else
            {
                livePreviewTextBox.Text = DateTime.Now.ToString(desiredDateFormatTextBox.Text);
                saveDateFormatButton.Enabled = true;
            }
        }
        catch (Exception)
        {
            livePreviewTextBox.Text = Resources.Strings.InvalidDateFormatErrorText;
            saveDateFormatButton.Enabled = false;
        }
    }

    private void CustomDateFormatInputForm_Load(object sender, EventArgs e)
    {
        timer.Enabled = true;
        saveDateFormatButton.Enabled = false; //always start disabled
    }

    //This timer exists to deal with visual bugs
    private void timer_Tick(object sender, EventArgs e)
    {
        //We only wanted to run this once
        timer.Enabled = false;

        //HACK: need to widen the form a tiny bit to get rid of the horizontal scrollbar :shrug:
        Width += 20;

        //HACK: For some reason resetting the auto scroll position doesn't work in the Load event.
        instructionsTableLayoutPanel.AutoScrollPosition = new System.Drawing.Point(0, 0);
    }

    private void saveDateFormatButton_Click(object sender, EventArgs e)
    {
        if (UtilityMethods.IsValidDateFormat(desiredDateFormatTextBox.Text))
        {
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            MessageBox.Show(this,
                Resources.Errors.InvalidDateFormatErrorMessage,
                Resources.Errors.InvalidDateFormatErrorTitle,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public override void SetTheme(Theme theme)
    {
        if (DesignMode)
        {
            return;
        }

        base.SetTheme(theme);
        saveDateFormatButton.ForeColor = Color.Black;
        dateFormatDocsLinkLabel.LinkColor = theme.HyperlinkColor;
        dateFormatDocsLinkLabel.ActiveLinkColor = theme.ActiveHyperlinkColor;
    }
}