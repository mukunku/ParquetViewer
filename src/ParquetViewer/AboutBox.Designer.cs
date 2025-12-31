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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
            tableLayoutPanel = new TableLayoutPanel();
            logoPictureBox = new PictureBox();
            labelProductName = new Label();
            labelCopyright = new Label();
            labelCompanyName = new Label();
            textBoxDescription = new TextBox();
            okButton = new Button();
            associateFileExtensionCheckBox = new CheckBox();
            flowLayoutPanel2 = new FlowLayoutPanel();
            labelVersion = new Label();
            newVersionLabel = new LinkLabel();
            tableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)logoPictureBox).BeginInit();
            flowLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            resources.ApplyResources(tableLayoutPanel, "tableLayoutPanel");
            tableLayoutPanel.Controls.Add(logoPictureBox, 0, 0);
            tableLayoutPanel.Controls.Add(labelProductName, 1, 0);
            tableLayoutPanel.Controls.Add(labelCopyright, 1, 2);
            tableLayoutPanel.Controls.Add(labelCompanyName, 1, 3);
            tableLayoutPanel.Controls.Add(textBoxDescription, 1, 4);
            tableLayoutPanel.Controls.Add(okButton, 2, 5);
            tableLayoutPanel.Controls.Add(associateFileExtensionCheckBox, 1, 5);
            tableLayoutPanel.Controls.Add(flowLayoutPanel2, 1, 1);
            tableLayoutPanel.Name = "tableLayoutPanel";
            // 
            // logoPictureBox
            // 
            resources.ApplyResources(logoPictureBox, "logoPictureBox");
            logoPictureBox.Image = Resources.Icons.coffee;
            logoPictureBox.Name = "logoPictureBox";
            tableLayoutPanel.SetRowSpan(logoPictureBox, 6);
            logoPictureBox.TabStop = false;
            // 
            // labelProductName
            // 
            tableLayoutPanel.SetColumnSpan(labelProductName, 2);
            resources.ApplyResources(labelProductName, "labelProductName");
            labelProductName.Name = "labelProductName";
            // 
            // labelCopyright
            // 
            tableLayoutPanel.SetColumnSpan(labelCopyright, 2);
            resources.ApplyResources(labelCopyright, "labelCopyright");
            labelCopyright.Name = "labelCopyright";
            // 
            // labelCompanyName
            // 
            tableLayoutPanel.SetColumnSpan(labelCompanyName, 2);
            resources.ApplyResources(labelCompanyName, "labelCompanyName");
            labelCompanyName.Name = "labelCompanyName";
            // 
            // textBoxDescription
            // 
            tableLayoutPanel.SetColumnSpan(textBoxDescription, 2);
            resources.ApplyResources(textBoxDescription, "textBoxDescription");
            textBoxDescription.Name = "textBoxDescription";
            textBoxDescription.ReadOnly = true;
            textBoxDescription.TabStop = false;
            // 
            // okButton
            // 
            resources.ApplyResources(okButton, "okButton");
            okButton.DialogResult = DialogResult.Cancel;
            okButton.Name = "okButton";
            // 
            // associateFileExtensionCheckBox
            // 
            resources.ApplyResources(associateFileExtensionCheckBox, "associateFileExtensionCheckBox");
            associateFileExtensionCheckBox.Name = "associateFileExtensionCheckBox";
            associateFileExtensionCheckBox.CheckedChanged += associateFileExtensionCheckBox_CheckedChanged;
            // 
            // flowLayoutPanel2
            // 
            tableLayoutPanel.SetColumnSpan(flowLayoutPanel2, 2);
            flowLayoutPanel2.Controls.Add(labelVersion);
            flowLayoutPanel2.Controls.Add(newVersionLabel);
            resources.ApplyResources(flowLayoutPanel2, "flowLayoutPanel2");
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            // 
            // labelVersion
            // 
            resources.ApplyResources(labelVersion, "labelVersion");
            labelVersion.Name = "labelVersion";
            // 
            // newVersionLabel
            // 
            resources.ApplyResources(newVersionLabel, "newVersionLabel");
            newVersionLabel.Image = Resources.Icons.external_link_icon;
            newVersionLabel.Name = "newVersionLabel";
            newVersionLabel.TabStop = true;
            newVersionLabel.LinkClicked += newVersionLabel_LinkClicked;
            // 
            // AboutBox
            // 
            AcceptButton = okButton;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutBox";
            ShowIcon = false;
            ShowInTaskbar = false;
            Load += AboutBox_Load;
            KeyUp += AboutBox_KeyUp;
            tableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)logoPictureBox).EndInit();
            flowLayoutPanel2.ResumeLayout(false);
            flowLayoutPanel2.PerformLayout();
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
        private FlowLayoutPanel flowLayoutPanel2;
    }
}
