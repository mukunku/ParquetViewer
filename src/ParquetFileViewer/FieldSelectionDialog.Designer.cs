namespace ParquetFileViewer
{
    partial class FieldsToLoadForm
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
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.allFieldsRadioButton = new System.Windows.Forms.RadioButton();
            this.showSelectedFieldsRadioButton = new System.Windows.Forms.RadioButton();
            this.fieldsPanel = new System.Windows.Forms.Panel();
            this.allFieldsRememberRadioButton = new System.Windows.Forms.RadioButton();
            this.doneButton = new System.Windows.Forms.Button();
            this.filterColumnsTextbox = new System.Windows.Forms.TextBox();
            this.clearfilterColumnsButton = new System.Windows.Forms.Button();
            this.mainTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.ColumnCount = 3;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.mainTableLayoutPanel.Controls.Add(this.allFieldsRadioButton, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.showSelectedFieldsRadioButton, 0, 2);
            this.mainTableLayoutPanel.Controls.Add(this.fieldsPanel, 1, 4);
            this.mainTableLayoutPanel.Controls.Add(this.allFieldsRememberRadioButton, 0, 1);
            this.mainTableLayoutPanel.Controls.Add(this.doneButton, 1, 5);
            this.mainTableLayoutPanel.Controls.Add(this.filterColumnsTextbox, 1, 3);
            this.mainTableLayoutPanel.Controls.Add(this.clearfilterColumnsButton, 2, 3);
            this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 5;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(429, 346);
            this.mainTableLayoutPanel.TabIndex = 0;
            // 
            // allFieldsRadioButton
            // 
            this.allFieldsRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.allFieldsRadioButton.AutoSize = true;
            this.allFieldsRadioButton.Checked = true;
            this.mainTableLayoutPanel.SetColumnSpan(this.allFieldsRadioButton, 2);
            this.allFieldsRadioButton.Location = new System.Drawing.Point(3, 3);
            this.allFieldsRadioButton.Name = "allFieldsRadioButton";
            this.allFieldsRadioButton.Size = new System.Drawing.Size(75, 24);
            this.allFieldsRadioButton.TabIndex = 0;
            this.allFieldsRadioButton.TabStop = true;
            this.allFieldsRadioButton.Text = "All Fields...";
            this.allFieldsRadioButton.UseVisualStyleBackColor = true;
            this.allFieldsRadioButton.CheckedChanged += new System.EventHandler(this.allFieldsRadioButton_CheckedChanged);
            // 
            // showSelectedFieldsRadioButton
            // 
            this.showSelectedFieldsRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.showSelectedFieldsRadioButton.AutoSize = true;
            this.mainTableLayoutPanel.SetColumnSpan(this.showSelectedFieldsRadioButton, 2);
            this.showSelectedFieldsRadioButton.Location = new System.Drawing.Point(3, 63);
            this.showSelectedFieldsRadioButton.Name = "showSelectedFieldsRadioButton";
            this.showSelectedFieldsRadioButton.Size = new System.Drawing.Size(100, 24);
            this.showSelectedFieldsRadioButton.TabIndex = 1;
            this.showSelectedFieldsRadioButton.Text = "Selected Fields:";
            this.showSelectedFieldsRadioButton.UseVisualStyleBackColor = true;
            this.showSelectedFieldsRadioButton.CheckedChanged += new System.EventHandler(this.showSelectedFieldsRadioButton_CheckedChanged);
            // 
            // fieldsPanel
            // 
            this.fieldsPanel.AutoScroll = true;
            this.mainTableLayoutPanel.SetColumnSpan(this.fieldsPanel, 2);
            this.fieldsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldsPanel.Enabled = false;
            this.fieldsPanel.Location = new System.Drawing.Point(23, 121);
            this.fieldsPanel.Name = "fieldsPanel";
            this.fieldsPanel.Size = new System.Drawing.Size(403, 192);
            this.fieldsPanel.TabIndex = 2;
            // 
            // allFieldsRememberRadioButton
            // 
            this.allFieldsRememberRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.allFieldsRememberRadioButton.AutoSize = true;
            this.mainTableLayoutPanel.SetColumnSpan(this.allFieldsRememberRadioButton, 2);
            this.allFieldsRememberRadioButton.Location = new System.Drawing.Point(3, 33);
            this.allFieldsRememberRadioButton.Name = "allFieldsRememberRadioButton";
            this.allFieldsRememberRadioButton.Size = new System.Drawing.Size(181, 24);
            this.allFieldsRememberRadioButton.TabIndex = 1;
            this.allFieldsRememberRadioButton.Text = "All Fields... (remember my choice)";
            this.allFieldsRememberRadioButton.UseVisualStyleBackColor = true;
            this.allFieldsRememberRadioButton.CheckedChanged += new System.EventHandler(this.AllFieldsRememberRadioButton_CheckedChanged);
            // 
            // doneButton
            // 
            this.doneButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTableLayoutPanel.SetColumnSpan(this.doneButton, 2);
            this.doneButton.Location = new System.Drawing.Point(329, 319);
            this.doneButton.Name = "doneButton";
            this.doneButton.Size = new System.Drawing.Size(97, 24);
            this.doneButton.TabIndex = 3;
            this.doneButton.Text = "Done";
            this.doneButton.UseVisualStyleBackColor = true;
            this.doneButton.Click += new System.EventHandler(this.doneButton_Click);
            // 
            // filterColumnsTextbox
            // 
            this.filterColumnsTextbox.Enabled = false;
            this.filterColumnsTextbox.Location = new System.Drawing.Point(23, 93);
            this.filterColumnsTextbox.Name = "filterColumnsTextbox";
            this.filterColumnsTextbox.Size = new System.Drawing.Size(373, 20);
            this.filterColumnsTextbox.TabIndex = 0;
            this.filterColumnsTextbox.TextChanged += new System.EventHandler(this.filterColumnsTextbox_TextChanged);
            // 
            // clearfilterColumnsButton
            // 
            this.clearfilterColumnsButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clearfilterColumnsButton.Enabled = false;
            this.clearfilterColumnsButton.Location = new System.Drawing.Point(402, 93);
            this.clearfilterColumnsButton.Name = "clearfilterColumnsButton";
            this.clearfilterColumnsButton.Size = new System.Drawing.Size(24, 22);
            this.clearfilterColumnsButton.TabIndex = 4;
            this.clearfilterColumnsButton.Text = "X";
            this.clearfilterColumnsButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.clearfilterColumnsButton.UseVisualStyleBackColor = true;
            this.clearfilterColumnsButton.Click += new System.EventHandler(this.clearfilterColumnsButton_Click);
            // 
            // FieldsToLoadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(429, 346);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.Icon = global::ParquetFileViewer.Properties.Resources.list_icon_32x32;
            this.MinimumSize = new System.Drawing.Size(215, 250);
            this.Name = "FieldsToLoadForm";
            this.Text = "Select Fields to Load";
            this.Load += new System.EventHandler(this.FieldsToLoadForm_Load);
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.mainTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.RadioButton allFieldsRadioButton;
        private System.Windows.Forms.RadioButton showSelectedFieldsRadioButton;
        private System.Windows.Forms.Panel fieldsPanel;
        private System.Windows.Forms.Button doneButton;
        private System.Windows.Forms.RadioButton allFieldsRememberRadioButton;
        private System.Windows.Forms.TextBox filterColumnsTextbox;
        private System.Windows.Forms.Button clearfilterColumnsButton;
    }
}