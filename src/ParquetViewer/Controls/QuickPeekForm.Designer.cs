using ParquetViewer.Helpers;
using System.Windows.Forms;

namespace ParquetViewer.Controls
{
    partial class QuickPeekForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.mainPictureBox?.Image.DisposeSafely();
                this.mainGridView?.DisposeSafely();

                if (components is not null)
                    components.DisposeSafely();
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickPeekForm));
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            mainTableLayoutPanel = new TableLayoutPanel();
            mainGridView = new ParquetGridView();
            takeMeBackLinkLabel = new LinkLabel();
            closeWindowButton = new Button();
            saveImageToFileButton = new Button();
            mainPictureBox = new PictureBox();
            imageRightClickMenu = new ContextMenuStrip(components);
            copyToClipboardToolStripMenuItem = new ToolStripMenuItem();
            mainTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mainGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)mainPictureBox).BeginInit();
            imageRightClickMenu.SuspendLayout();
            SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            resources.ApplyResources(mainTableLayoutPanel, "mainTableLayoutPanel");
            mainTableLayoutPanel.Controls.Add(mainGridView, 0, 1);
            mainTableLayoutPanel.Controls.Add(takeMeBackLinkLabel, 0, 0);
            mainTableLayoutPanel.Controls.Add(closeWindowButton, 1, 0);
            mainTableLayoutPanel.Controls.Add(saveImageToFileButton, 0, 2);
            mainTableLayoutPanel.Controls.Add(mainPictureBox, 1, 1);
            mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            // 
            // mainGridView
            // 
            resources.ApplyResources(mainGridView, "mainGridView");
            mainGridView.AllowUserToAddRows = false;
            mainGridView.AllowUserToDeleteRows = false;
            mainGridView.AllowUserToOrderColumns = true;
            mainGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            mainGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            mainGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            mainGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            mainGridView.CopyAsWhereConditionText = "Copy as WHERE...";
            mainGridView.CopyAsWhereIcon = null;
            mainGridView.CopyToClipboardIcon = (System.Drawing.Image)resources.GetObject("mainGridView.CopyToClipboardIcon");
            mainGridView.CopyToClipboardText = "Copy";
            mainGridView.CopyToClipboardWithHeadersText = "Copy with headers";
            mainGridView.EnableHeadersVisualStyles = false;
            mainGridView.Name = "mainGridView";
            mainGridView.ReadOnly = true;
            mainGridView.ScientificFormattingText = "Scientific";
            mainGridView.ShowCellToolTips = false;
            mainGridView.ShowCopyAsWhereContextMenuItem = false;
            // 
            // takeMeBackLinkLabel
            // 
            resources.ApplyResources(takeMeBackLinkLabel, "takeMeBackLinkLabel");
            takeMeBackLinkLabel.Name = "takeMeBackLinkLabel";
            takeMeBackLinkLabel.TabStop = true;
            takeMeBackLinkLabel.LinkClicked += TakeMeBackLinkLabel_LinkClicked;
            // 
            // closeWindowButton
            // 
            resources.ApplyResources(closeWindowButton, "closeWindowButton");
            closeWindowButton.DialogResult = DialogResult.Cancel;
            closeWindowButton.Name = "closeWindowButton";
            closeWindowButton.UseVisualStyleBackColor = true;
            closeWindowButton.Click += CloseWindowButton_Click;
            // 
            // saveImageToFileButton
            // 
            resources.ApplyResources(saveImageToFileButton, "saveImageToFileButton");
            mainTableLayoutPanel.SetColumnSpan(saveImageToFileButton, 2);
            saveImageToFileButton.Image = Resources.Icons.save_icon;
            saveImageToFileButton.Name = "saveImageToFileButton";
            saveImageToFileButton.UseVisualStyleBackColor = true;
            saveImageToFileButton.Click += saveImageToFileButton_Click;
            // 
            // mainPictureBox
            // 
            resources.ApplyResources(mainPictureBox, "mainPictureBox");
            mainPictureBox.ContextMenuStrip = imageRightClickMenu;
            mainPictureBox.Name = "mainPictureBox";
            mainPictureBox.TabStop = false;
            // 
            // imageRightClickMenu
            // 
            resources.ApplyResources(imageRightClickMenu, "imageRightClickMenu");
            imageRightClickMenu.Items.AddRange(new ToolStripItem[] { copyToClipboardToolStripMenuItem });
            imageRightClickMenu.Name = "imageRightClickMenu";
            // 
            // copyToClipboardToolStripMenuItem
            // 
            resources.ApplyResources(copyToClipboardToolStripMenuItem, "copyToClipboardToolStripMenuItem");
            copyToClipboardToolStripMenuItem.Name = "copyToClipboardToolStripMenuItem";
            copyToClipboardToolStripMenuItem.Click += copyToClipboardToolStripMenuItem_Click;
            // 
            // QuickPeekForm
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = closeWindowButton;
            Controls.Add(mainTableLayoutPanel);
            DoubleBuffered = true;
            Name = "QuickPeekForm";
            Load += QuickPeekForm_Load;
            mainTableLayoutPanel.ResumeLayout(false);
            mainTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)mainGridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)mainPictureBox).EndInit();
            imageRightClickMenu.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private ParquetGridView mainGridView;
        private System.Windows.Forms.LinkLabel takeMeBackLinkLabel;
        private System.Windows.Forms.Button closeWindowButton;
        private Button saveImageToFileButton;
        private PictureBox mainPictureBox;
        private ContextMenuStrip imageRightClickMenu;
        private ToolStripMenuItem copyToClipboardToolStripMenuItem;
    }
}