namespace ParquetFileViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickPeekForm));
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.takeMeBackLinkLabel = new System.Windows.Forms.LinkLabel();
            this.closeWindowButton = new System.Windows.Forms.Button();
            this.mainGridView = new ParquetFileViewer.Controls.ParquetGridView();
            this.mainTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.ColumnCount = 2;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 66.66666F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.mainTableLayoutPanel.Controls.Add(this.mainGridView, 0, 1);
            this.mainTableLayoutPanel.Controls.Add(this.takeMeBackLinkLabel, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.closeWindowButton, 1, 0);
            this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 2;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(278, 348);
            this.mainTableLayoutPanel.TabIndex = 0;
            // 
            // takeMeBackLinkLabel
            // 
            this.takeMeBackLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.takeMeBackLinkLabel.AutoSize = true;
            this.takeMeBackLinkLabel.Location = new System.Drawing.Point(3, 0);
            this.takeMeBackLinkLabel.Name = "takeMeBackLinkLabel";
            this.takeMeBackLinkLabel.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.takeMeBackLinkLabel.Size = new System.Drawing.Size(52, 20);
            this.takeMeBackLinkLabel.TabIndex = 1;
            this.takeMeBackLinkLabel.TabStop = true;
            this.takeMeBackLinkLabel.Text = "<<< back";
            this.takeMeBackLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.TakeMeBackLinkLabel_LinkClicked);
            // 
            // closeWindowButton
            // 
            this.closeWindowButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeWindowButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeWindowButton.Location = new System.Drawing.Point(274, 16);
            this.closeWindowButton.Name = "closeWindowButton";
            this.closeWindowButton.Size = new System.Drawing.Size(50, 25);
            this.closeWindowButton.TabIndex = 2;
            this.closeWindowButton.Text = "Close";
            this.closeWindowButton.UseVisualStyleBackColor = true;
            this.closeWindowButton.Click += new System.EventHandler(this.CloseWindowButton_Click);
            // 
            // mainGridView
            // 
            this.mainGridView.AllowUserToAddRows = false;
            this.mainGridView.AllowUserToDeleteRows = false;
            this.mainGridView.AllowUserToOrderColumns = true;
            this.mainGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.mainTableLayoutPanel.SetColumnSpan(this.mainGridView, 2);
            this.mainGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainGridView.Location = new System.Drawing.Point(3, 23);
            this.mainGridView.Name = "mainGridView";
            this.mainGridView.ReadOnly = true;
            this.mainGridView.Size = new System.Drawing.Size(272, 322);
            this.mainGridView.TabIndex = 0;
            // 
            // QuickPeekForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeWindowButton;
            this.ClientSize = new System.Drawing.Size(278, 348);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.DoubleBuffered = true;
            this.Icon = global::ParquetFileViewer.Properties.Resources.eye_with_logo;
            this.Name = "QuickPeekForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Quick Peek";
            this.Load += new System.EventHandler(this.QuickPeakForm_Load);
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.mainTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private Controls.ParquetGridView mainGridView;
        private System.Windows.Forms.LinkLabel takeMeBackLinkLabel;
        private System.Windows.Forms.Button closeWindowButton;
    }
}