using System.Drawing;
using System.Windows.Forms;

namespace ParquetViewer
{
    public partial class AboutBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tableLayoutPanel = new TableLayoutPanel();
            newVersionLabel = new LinkLabel();
            logoPictureBox = new PictureBox();
            labelProductName = new Label();
            labelVersion = new Label();
            labelCopyright = new Label();
            labelCompanyName = new Label();
            textBoxDescription = new TextBox();
            okButton = new Button();
            associateFileExtensionCheckBox = new CheckBox();
            tableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)logoPictureBox).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 3;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));
            tableLayoutPanel.Controls.Add(newVersionLabel, 1, 4);
            tableLayoutPanel.Controls.Add(logoPictureBox, 0, 0);
            tableLayoutPanel.Controls.Add(labelProductName, 1, 0);
            tableLayoutPanel.Controls.Add(labelVersion, 1, 1);
            tableLayoutPanel.Controls.Add(labelCopyright, 1, 2);
            tableLayoutPanel.Controls.Add(labelCompanyName, 1, 3);
            tableLayoutPanel.Controls.Add(textBoxDescription, 1, 5);
            tableLayoutPanel.Controls.Add(okButton, 2, 6);
            tableLayoutPanel.Controls.Add(associateFileExtensionCheckBox, 1, 6);
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(10, 10);
            tableLayoutPanel.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 7;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 9.090909F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 9.090909F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 9.090909F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 9.090909F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 9.090909F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 45.4545441F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 9.090909F));
            tableLayoutPanel.Size = new Size(487, 307);
            tableLayoutPanel.TabIndex = 0;
            // 
            // newVersionLabel
            // 
            tableLayoutPanel.SetColumnSpan(newVersionLabel, 2);
            newVersionLabel.Dock = DockStyle.Fill;
            newVersionLabel.Location = new Point(167, 108);
            newVersionLabel.Margin = new Padding(7, 0, 4, 0);
            newVersionLabel.MaximumSize = new Size(0, 20);
            newVersionLabel.Name = "newVersionLabel";
            newVersionLabel.Size = new Size(316, 20);
            newVersionLabel.TabIndex = 25;
            newVersionLabel.TabStop = true;
            newVersionLabel.Text = "New Version Available!";
            newVersionLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // logoPictureBox
            // 
            logoPictureBox.Dock = DockStyle.Fill;
            logoPictureBox.Image = Properties.Resources.coffee;
            logoPictureBox.Location = new Point(4, 3);
            logoPictureBox.Margin = new Padding(4, 3, 4, 3);
            logoPictureBox.Name = "logoPictureBox";
            tableLayoutPanel.SetRowSpan(logoPictureBox, 7);
            logoPictureBox.Size = new Size(152, 301);
            logoPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            logoPictureBox.TabIndex = 12;
            logoPictureBox.TabStop = false;
            // 
            // labelProductName
            // 
            tableLayoutPanel.SetColumnSpan(labelProductName, 2);
            labelProductName.Dock = DockStyle.Fill;
            labelProductName.Location = new Point(167, 0);
            labelProductName.Margin = new Padding(7, 0, 4, 0);
            labelProductName.MaximumSize = new Size(0, 20);
            labelProductName.Name = "labelProductName";
            labelProductName.Size = new Size(316, 20);
            labelProductName.TabIndex = 19;
            labelProductName.Text = "Product Name";
            labelProductName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelVersion
            // 
            tableLayoutPanel.SetColumnSpan(labelVersion, 2);
            labelVersion.Dock = DockStyle.Fill;
            labelVersion.Location = new Point(167, 27);
            labelVersion.Margin = new Padding(7, 0, 4, 0);
            labelVersion.MaximumSize = new Size(0, 20);
            labelVersion.Name = "labelVersion";
            labelVersion.Size = new Size(316, 20);
            labelVersion.TabIndex = 0;
            labelVersion.Text = "Version";
            labelVersion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelCopyright
            // 
            tableLayoutPanel.SetColumnSpan(labelCopyright, 2);
            labelCopyright.Dock = DockStyle.Fill;
            labelCopyright.Location = new Point(167, 54);
            labelCopyright.Margin = new Padding(7, 0, 4, 0);
            labelCopyright.MaximumSize = new Size(0, 20);
            labelCopyright.Name = "labelCopyright";
            labelCopyright.Size = new Size(316, 20);
            labelCopyright.TabIndex = 21;
            labelCopyright.Text = "Copyright";
            labelCopyright.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelCompanyName
            // 
            tableLayoutPanel.SetColumnSpan(labelCompanyName, 2);
            labelCompanyName.Dock = DockStyle.Fill;
            labelCompanyName.Location = new Point(167, 81);
            labelCompanyName.Margin = new Padding(7, 0, 4, 0);
            labelCompanyName.MaximumSize = new Size(0, 20);
            labelCompanyName.Name = "labelCompanyName";
            labelCompanyName.Size = new Size(316, 20);
            labelCompanyName.TabIndex = 22;
            labelCompanyName.Text = "Company Name";
            labelCompanyName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // textBoxDescription
            // 
            tableLayoutPanel.SetColumnSpan(textBoxDescription, 2);
            textBoxDescription.Dock = DockStyle.Fill;
            textBoxDescription.Location = new Point(167, 138);
            textBoxDescription.Margin = new Padding(7, 3, 4, 3);
            textBoxDescription.Multiline = true;
            textBoxDescription.Name = "textBoxDescription";
            textBoxDescription.ReadOnly = true;
            textBoxDescription.ScrollBars = ScrollBars.Both;
            textBoxDescription.Size = new Size(316, 133);
            textBoxDescription.TabIndex = 23;
            textBoxDescription.TabStop = false;
            // 
            // okButton
            // 
            okButton.Anchor = AnchorStyles.Right;
            okButton.DialogResult = DialogResult.Cancel;
            okButton.Location = new Point(395, 277);
            okButton.Margin = new Padding(4, 3, 4, 3);
            okButton.Name = "okButton";
            okButton.Size = new Size(88, 27);
            okButton.TabIndex = 24;
            okButton.Text = "&OK";
            // 
            // associateFileExtensionCheckBox
            // 
            associateFileExtensionCheckBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            associateFileExtensionCheckBox.Location = new Point(164, 277);
            associateFileExtensionCheckBox.Margin = new Padding(4, 3, 4, 3);
            associateFileExtensionCheckBox.Name = "associateFileExtensionCheckBox";
            associateFileExtensionCheckBox.Size = new Size(211, 27);
            associateFileExtensionCheckBox.TabIndex = 24;
            associateFileExtensionCheckBox.Text = "Associate with .parquet files";
            associateFileExtensionCheckBox.CheckedChanged += associateFileExtensionCheckBox_CheckedChanged;
            // 
            // AboutBox
            // 
            AcceptButton = okButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(507, 327);
            Controls.Add(tableLayoutPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            KeyPreview = true;
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutBox";
            Padding = new Padding(10);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "About";
            Load += AboutBox_Load;
            KeyUp += AboutBox_KeyUp;
            tableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)logoPictureBox).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.PictureBox logoPictureBox;
        private System.Windows.Forms.Label labelProductName;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelCopyright;
        private System.Windows.Forms.Label labelCompanyName;
        private System.Windows.Forms.TextBox textBoxDescription;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.CheckBox associateFileExtensionCheckBox;
        private System.Windows.Forms.LinkLabel newVersionLabel;
    }
}
