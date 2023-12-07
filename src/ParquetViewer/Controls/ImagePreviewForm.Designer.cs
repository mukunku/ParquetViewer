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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImagePreviewForm));
            mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            saveAsPngButton = new System.Windows.Forms.Button();
            mainPictureBox = new System.Windows.Forms.PictureBox();
            copyToClipboardButton = new System.Windows.Forms.Button();
            closeButton = new System.Windows.Forms.Button();
            mainTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mainPictureBox).BeginInit();
            SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            mainTableLayoutPanel.ColumnCount = 2;
            mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            mainTableLayoutPanel.Controls.Add(saveAsPngButton, 1, 1);
            mainTableLayoutPanel.Controls.Add(mainPictureBox, 0, 0);
            mainTableLayoutPanel.Controls.Add(copyToClipboardButton, 0, 1);
            mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            mainTableLayoutPanel.RowCount = 2;
            mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            mainTableLayoutPanel.Size = new System.Drawing.Size(246, 211);
            mainTableLayoutPanel.TabIndex = 0;
            // 
            // saveAsPngButton
            // 
            saveAsPngButton.Dock = System.Windows.Forms.DockStyle.Fill;
            saveAsPngButton.Location = new System.Drawing.Point(126, 182);
            saveAsPngButton.Name = "saveAsPngButton";
            saveAsPngButton.Size = new System.Drawing.Size(117, 26);
            saveAsPngButton.TabIndex = 2;
            saveAsPngButton.Text = "Save as PNG";
            saveAsPngButton.UseVisualStyleBackColor = true;
            saveAsPngButton.Click += saveAsPngButton_Click;
            // 
            // mainPictureBox
            // 
            mainTableLayoutPanel.SetColumnSpan(mainPictureBox, 2);
            mainPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            mainPictureBox.Location = new System.Drawing.Point(3, 3);
            mainPictureBox.Name = "mainPictureBox";
            mainPictureBox.Size = new System.Drawing.Size(240, 173);
            mainPictureBox.TabIndex = 0;
            mainPictureBox.TabStop = false;
            // 
            // copyToClipboardButton
            // 
            copyToClipboardButton.Dock = System.Windows.Forms.DockStyle.Fill;
            copyToClipboardButton.Location = new System.Drawing.Point(3, 182);
            copyToClipboardButton.Name = "copyToClipboardButton";
            copyToClipboardButton.Size = new System.Drawing.Size(117, 26);
            copyToClipboardButton.TabIndex = 1;
            copyToClipboardButton.Text = "Copy to clipboard";
            copyToClipboardButton.UseVisualStyleBackColor = true;
            copyToClipboardButton.Click += copyToClipboardButton_Click;
            // 
            // closeButton
            // 
            closeButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            closeButton.Location = new System.Drawing.Point(94, 0);
            closeButton.MaximumSize = new System.Drawing.Size(1, 1);
            closeButton.Name = "closeButton";
            closeButton.Size = new System.Drawing.Size(1, 0);
            closeButton.TabIndex = 0;
            closeButton.TabStop = false;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            // 
            // ImagePreviewForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.Control;
            CancelButton = closeButton;
            ClientSize = new System.Drawing.Size(246, 211);
            Controls.Add(closeButton);
            Controls.Add(mainTableLayoutPanel);
            ForeColor = System.Drawing.Color.Black;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "ImagePreviewForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Image Preview";
            Load += ImagePreviewForm_Load;
            mainTableLayoutPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainPictureBox).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.PictureBox mainPictureBox;
        private System.Windows.Forms.Button saveAsPngButton;
        private System.Windows.Forms.Button copyToClipboardButton;
        private System.Windows.Forms.Button closeButton;
    }
}