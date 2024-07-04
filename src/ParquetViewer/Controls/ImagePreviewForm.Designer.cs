namespace ParquetViewer.Controls
{
    partial class ImagePreviewForm
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImagePreviewForm));
            mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            saveAsPngButton = new System.Windows.Forms.Button();
            closeButton = new System.Windows.Forms.Button();
            mainPictureBox = new System.Windows.Forms.PictureBox();
            imageRightClickMenu = new System.Windows.Forms.ContextMenuStrip(components);
            copyToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            mainTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mainPictureBox).BeginInit();
            imageRightClickMenu.SuspendLayout();
            SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            mainTableLayoutPanel.ColumnCount = 2;
            mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            mainTableLayoutPanel.Controls.Add(saveAsPngButton, 0, 1);
            mainTableLayoutPanel.Controls.Add(closeButton, 1, 1);
            mainTableLayoutPanel.Controls.Add(mainPictureBox, 0, 0);
            mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            mainTableLayoutPanel.RowCount = 2;
            mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            mainTableLayoutPanel.Size = new System.Drawing.Size(539, 326);
            mainTableLayoutPanel.TabIndex = 0;
            // 
            // saveAsPngButton
            // 
            saveAsPngButton.Dock = System.Windows.Forms.DockStyle.Fill;
            saveAsPngButton.Location = new System.Drawing.Point(3, 297);
            saveAsPngButton.Name = "saveAsPngButton";
            saveAsPngButton.Size = new System.Drawing.Size(263, 26);
            saveAsPngButton.TabIndex = 3;
            saveAsPngButton.Text = "Save as PNG";
            saveAsPngButton.UseVisualStyleBackColor = true;
            saveAsPngButton.Click += saveAsPngButton_Click;
            // 
            // closeButton
            // 
            closeButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            closeButton.Location = new System.Drawing.Point(273, 297);
            closeButton.Name = "closeButton";
            closeButton.Size = new System.Drawing.Size(263, 26);
            closeButton.TabIndex = 4;
            closeButton.TabStop = false;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            // 
            // mainPictureBox
            // 
            mainPictureBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            mainTableLayoutPanel.SetColumnSpan(mainPictureBox, 2);
            mainPictureBox.ContextMenuStrip = imageRightClickMenu;
            mainPictureBox.Location = new System.Drawing.Point(3, 3);
            mainPictureBox.Name = "mainPictureBox";
            mainPictureBox.Size = new System.Drawing.Size(533, 288);
            mainPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            mainPictureBox.TabIndex = 0;
            mainPictureBox.TabStop = false;
            // 
            // imageRightClickMenu
            // 
            imageRightClickMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { copyToClipboardToolStripMenuItem });
            imageRightClickMenu.Name = "imageRightClickMenu";
            imageRightClickMenu.Size = new System.Drawing.Size(170, 26);
            // 
            // copyToClipboardToolStripMenuItem
            // 
            copyToClipboardToolStripMenuItem.Name = "copyToClipboardToolStripMenuItem";
            copyToClipboardToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            copyToClipboardToolStripMenuItem.Text = "Copy to clipboard";
            copyToClipboardToolStripMenuItem.Click += copyToClipboardToolStripMenuItem_Click;
            // 
            // ImagePreviewForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.Control;
            CancelButton = closeButton;
            ClientSize = new System.Drawing.Size(539, 326);
            Controls.Add(mainTableLayoutPanel);
            ForeColor = System.Drawing.Color.Black;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "ImagePreviewForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Image Preview";
            Load += ImagePreviewForm_Load;
            mainTableLayoutPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainPictureBox).EndInit();
            imageRightClickMenu.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.PictureBox mainPictureBox;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.ContextMenuStrip imageRightClickMenu;
        private System.Windows.Forms.ToolStripMenuItem copyToClipboardToolStripMenuItem;
        private System.Windows.Forms.Button saveAsPngButton;
    }
}