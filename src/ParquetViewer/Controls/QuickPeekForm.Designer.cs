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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickPeekForm));
            mainTableLayoutPanel = new TableLayoutPanel();
            mainGridView = new ParquetGridView();
            takeMeBackLinkLabel = new LinkLabel();
            closeWindowButton = new Button();
            mainTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mainGridView).BeginInit();
            SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            mainTableLayoutPanel.ColumnCount = 2;
            mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66.66666F));
            mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
            mainTableLayoutPanel.Controls.Add(mainGridView, 0, 1);
            mainTableLayoutPanel.Controls.Add(takeMeBackLinkLabel, 0, 0);
            mainTableLayoutPanel.Controls.Add(closeWindowButton, 1, 0);
            mainTableLayoutPanel.Dock = DockStyle.Fill;
            mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            mainTableLayoutPanel.Margin = new Padding(4, 3, 4, 3);
            mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            mainTableLayoutPanel.RowCount = 2;
            mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));
            mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTableLayoutPanel.Size = new System.Drawing.Size(324, 402);
            mainTableLayoutPanel.TabIndex = 0;
            // 
            // mainGridView
            // 
            mainGridView.AllowUserToAddRows = false;
            mainGridView.AllowUserToDeleteRows = false;
            mainGridView.AllowUserToOrderColumns = true;
            mainGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            mainGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            mainGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            mainTableLayoutPanel.SetColumnSpan(mainGridView, 2);
            mainGridView.Dock = DockStyle.Fill;
            mainGridView.EnableHeadersVisualStyles = false;
            mainGridView.Location = new System.Drawing.Point(4, 26);
            mainGridView.Margin = new Padding(4, 3, 4, 3);
            mainGridView.Name = "mainGridView";
            mainGridView.ReadOnly = true;
            mainGridView.RowHeadersWidth = 24;
            mainGridView.Size = new System.Drawing.Size(316, 373);
            mainGridView.TabIndex = 0;
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
            // QuickPeekForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = closeWindowButton;
            ClientSize = new System.Drawing.Size(324, 402);
            Controls.Add(mainTableLayoutPanel);
            DoubleBuffered = true;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            Name = "QuickPeekForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Quick Peek";
            Load += QuickPeakForm_Load;
            mainTableLayoutPanel.ResumeLayout(false);
            mainTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)mainGridView).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private ParquetGridView mainGridView;
        private System.Windows.Forms.LinkLabel takeMeBackLinkLabel;
        private System.Windows.Forms.Button closeWindowButton;
    }
}