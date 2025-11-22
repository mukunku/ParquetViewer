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
                try
                {
                    this.mainPictureBox?.Image.Dispose();
                }
                catch { /*swallow*/ }
                
                if (components is not null)
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
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickPeekForm));
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
            mainTableLayoutPanel.ColumnCount = 2;
            mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66.6666641F));
            mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333359F));
            mainTableLayoutPanel.Controls.Add(mainGridView, 0, 1);
            mainTableLayoutPanel.Controls.Add(takeMeBackLinkLabel, 0, 0);
            mainTableLayoutPanel.Controls.Add(closeWindowButton, 1, 0);
            mainTableLayoutPanel.Controls.Add(saveImageToFileButton, 0, 2);
            mainTableLayoutPanel.Controls.Add(mainPictureBox, 1, 1);
            mainTableLayoutPanel.Dock = DockStyle.Fill;
            mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            mainTableLayoutPanel.Margin = new Padding(4, 3, 4, 3);
            mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            mainTableLayoutPanel.RowCount = 3;
            mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));
            mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            mainTableLayoutPanel.Size = new System.Drawing.Size(324, 275);
            mainTableLayoutPanel.TabIndex = 0;
            // 
            // mainGridView
            // 
            mainGridView.AllowUserToAddRows = false;
            mainGridView.AllowUserToDeleteRows = false;
            mainGridView.AllowUserToOrderColumns = true;
            mainGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
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
            mainGridView.EnableHeadersVisualStyles = false;
            mainGridView.Location = new System.Drawing.Point(4, 26);
            mainGridView.Margin = new Padding(4, 3, 4, 3);
            mainGridView.Name = "mainGridView";
            mainGridView.ReadOnly = true;
            mainGridView.RowHeadersWidth = 24;
            mainGridView.ShowCellToolTips = false;
            mainGridView.Size = new System.Drawing.Size(208, 214);
            mainGridView.TabIndex = 0;
            mainGridView.CopyToClipboardIcon = Properties.Resources.copy_clipboard_icon.ToBitmap();
            // 
            // takeMeBackLinkLabel
            // 
            takeMeBackLinkLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            takeMeBackLinkLabel.AutoSize = true;
            takeMeBackLinkLabel.Location = new System.Drawing.Point(4, 0);
            takeMeBackLinkLabel.Margin = new Padding(4, 0, 4, 0);
            takeMeBackLinkLabel.Name = "takeMeBackLinkLabel";
            takeMeBackLinkLabel.Padding = new Padding(0, 3, 0, 0);
            takeMeBackLinkLabel.Size = new System.Drawing.Size(59, 23);
            takeMeBackLinkLabel.TabIndex = 1;
            takeMeBackLinkLabel.TabStop = true;
            takeMeBackLinkLabel.Text = "<<< back";
            takeMeBackLinkLabel.LinkClicked += TakeMeBackLinkLabel_LinkClicked;
            // 
            // closeWindowButton
            // 
            closeWindowButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            closeWindowButton.DialogResult = DialogResult.Cancel;
            closeWindowButton.Location = new System.Drawing.Point(262, 4);
            closeWindowButton.Margin = new Padding(4, 3, 4, 3);
            closeWindowButton.Name = "closeWindowButton";
            closeWindowButton.Size = new System.Drawing.Size(58, 16);
            closeWindowButton.TabIndex = 2;
            closeWindowButton.Text = "Close";
            closeWindowButton.UseVisualStyleBackColor = true;
            closeWindowButton.Click += CloseWindowButton_Click;
            // 
            // saveImageToFileButton
            // 
            saveImageToFileButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mainTableLayoutPanel.SetColumnSpan(saveImageToFileButton, 2);
            saveImageToFileButton.Image = Properties.Resources.save_icon;
            saveImageToFileButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            saveImageToFileButton.Location = new System.Drawing.Point(3, 246);
            saveImageToFileButton.Name = "saveImageToFileButton";
            saveImageToFileButton.Size = new System.Drawing.Size(318, 26);
            saveImageToFileButton.TabIndex = 3;
            saveImageToFileButton.Text = "Save as PNG";
            saveImageToFileButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            saveImageToFileButton.UseVisualStyleBackColor = true;
            saveImageToFileButton.Click += saveImageToFileButton_Click;
            // 
            // mainPictureBox
            // 
            mainPictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mainPictureBox.ContextMenuStrip = imageRightClickMenu;
            mainPictureBox.Location = new System.Drawing.Point(219, 26);
            mainPictureBox.Name = "mainPictureBox";
            mainPictureBox.Size = new System.Drawing.Size(102, 214);
            mainPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            mainPictureBox.TabIndex = 4;
            mainPictureBox.TabStop = false;
            // 
            // imageRightClickMenu
            // 
            imageRightClickMenu.Items.AddRange(new ToolStripItem[] { copyToClipboardToolStripMenuItem });
            imageRightClickMenu.Name = "imageRightClickMenu";
            imageRightClickMenu.Size = new System.Drawing.Size(170, 26);
            // 
            // copyToClipboardToolStripMenuItem
            // 
            copyToClipboardToolStripMenuItem.Name = "copyToClipboardToolStripMenuItem";
            copyToClipboardToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            copyToClipboardToolStripMenuItem.Text = "Copy to clipboard";
            copyToClipboardToolStripMenuItem.Click += copyToClipboardToolStripMenuItem_Click;
            copyToClipboardToolStripMenuItem.Image = Properties.Resources.copy_clipboard_icon.ToBitmap();
            // 
            // QuickPeekForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = closeWindowButton;
            ClientSize = new System.Drawing.Size(324, 275);
            Controls.Add(mainTableLayoutPanel);
            DoubleBuffered = true;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            Name = "QuickPeekForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Quick Peek";
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