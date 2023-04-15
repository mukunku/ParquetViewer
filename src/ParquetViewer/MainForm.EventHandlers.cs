using ParquetViewer.Engine;
using ParquetViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ParquetViewer
{
    public partial class MainForm
    {
        private const string QueryUselessPartRegex = "^WHERE ";

        private void offsetTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void recordsToTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void offsetTextBox_TextChanged(object sender, EventArgs e)
        {
            var textbox = sender as TextBox;
            if (int.TryParse(textbox.Text, out var offset))
                this.CurrentOffset = offset;
            else
                textbox.Text = this.CurrentOffset.ToString();
        }

        private void recordsToTextBox_TextChanged(object sender, EventArgs e)
        {
            var textbox = sender as TextBox;
            if (int.TryParse(textbox.Text, out var recordCount))
                this.CurrentMaxRowCount = recordCount;
            else
                textbox.Text = this.CurrentMaxRowCount.ToString();
        }

        private void searchFilterTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Return))
            {
                this.runQueryButton_Click(this.runQueryButton, null);
            }
        }

        private async void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    this.Cursor = Cursors.WaitCursor;
                    await this.OpenNewFileOrFolder(files[0]);
                }
            }
            catch (Exception ex)
            {
                this.OpenFileOrFolderPath = null;
                ShowError(ex);
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void searchFilterLabel_Click(object sender, EventArgs e)
        {
            MessageBox.Show(@"NULL CHECK: 
    WHERE field_name IS NULL
    WHERE field_name IS NOT NULL
DATETIME:   
    WHERE field_name >= #01/01/2000#
NUMERIC:
    WHERE field_name <= 123.4
STRING:
    WHERE field_name LIKE '%value%' 
    WHERE field_name = 'equals value'
    WHERE field_name <> 'not equals'
MULTIPLE CONDITIONS: 
    WHERE (field_1 > #01/01/2000# AND field_1 < #01/01/2001#) OR field_2 <> 100 OR field_3 = 'string value'", "Filtering Query Syntax Examples");
        }

        private void mainGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.actualShownRecordCountLabel.Text = this.mainGridView.RowCount.ToString();
        }

        private void showingStatusBarLabel_Click(object sender, EventArgs e)
        {
            //This is just here in case I want to add debug info
        }
    }
}
