namespace ParquetViewer
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
            mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            doneButton = new System.Windows.Forms.Button();
            fieldsPanel = new System.Windows.Forms.Panel();
            filterColumnsTextbox = new System.Windows.Forms.TextBox();
            clearfilterColumnsButton = new System.Windows.Forms.Button();
            showSelectedFieldsRadioButton = new System.Windows.Forms.RadioButton();
            allFieldsRadioButton = new System.Windows.Forms.RadioButton();
            rememberMyChoiceCheckBox = new System.Windows.Forms.CheckBox();
            mainTableLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            mainTableLayoutPanel.AutoSize = true;
            mainTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            mainTableLayoutPanel.ColumnCount = 4;
            mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            mainTableLayoutPanel.Controls.Add(doneButton, 2, 5);
            mainTableLayoutPanel.Controls.Add(fieldsPanel, 1, 3);
            mainTableLayoutPanel.Controls.Add(filterColumnsTextbox, 1, 2);
            mainTableLayoutPanel.Controls.Add(clearfilterColumnsButton, 3, 2);
            mainTableLayoutPanel.Controls.Add(showSelectedFieldsRadioButton, 0, 1);
            mainTableLayoutPanel.Controls.Add(allFieldsRadioButton, 0, 0);
            mainTableLayoutPanel.Controls.Add(rememberMyChoiceCheckBox, 2, 0);
            mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            mainTableLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            mainTableLayoutPanel.RowCount = 6;
            mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            mainTableLayoutPanel.Size = new System.Drawing.Size(585, 413);
            mainTableLayoutPanel.TabIndex = 1;
            // 
            // doneButton
            // 
            doneButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            mainTableLayoutPanel.SetColumnSpan(doneButton, 2);
            doneButton.Location = new System.Drawing.Point(468, 381);
            doneButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            doneButton.Name = "doneButton";
            doneButton.Size = new System.Drawing.Size(113, 29);
            doneButton.TabIndex = 3;
            doneButton.Text = "Done";
            doneButton.UseVisualStyleBackColor = true;
            doneButton.Click += doneButton_Click;
            // 
            // fieldsPanel
            // 
            fieldsPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            fieldsPanel.AutoScroll = true;
            mainTableLayoutPanel.SetColumnSpan(fieldsPanel, 3);
            fieldsPanel.Enabled = false;
            fieldsPanel.Location = new System.Drawing.Point(27, 105);
            fieldsPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            fieldsPanel.Name = "fieldsPanel";
            mainTableLayoutPanel.SetRowSpan(fieldsPanel, 2);
            fieldsPanel.Size = new System.Drawing.Size(554, 270);
            fieldsPanel.TabIndex = 2;
            // 
            // filterColumnsTextbox
            // 
            filterColumnsTextbox.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            mainTableLayoutPanel.SetColumnSpan(filterColumnsTextbox, 2);
            filterColumnsTextbox.Enabled = false;
            filterColumnsTextbox.Location = new System.Drawing.Point(27, 74);
            filterColumnsTextbox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            filterColumnsTextbox.Name = "filterColumnsTextbox";
            filterColumnsTextbox.PlaceholderText = "search by name";
            filterColumnsTextbox.Size = new System.Drawing.Size(520, 23);
            filterColumnsTextbox.TabIndex = 0;
            filterColumnsTextbox.TextChanged += filterColumnsTextbox_TextChanged;
            // 
            // clearfilterColumnsButton
            // 
            clearfilterColumnsButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            clearfilterColumnsButton.Enabled = false;
            clearfilterColumnsButton.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            clearfilterColumnsButton.Location = new System.Drawing.Point(555, 73);
            clearfilterColumnsButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            clearfilterColumnsButton.Name = "clearfilterColumnsButton";
            clearfilterColumnsButton.Size = new System.Drawing.Size(26, 26);
            clearfilterColumnsButton.TabIndex = 4;
            clearfilterColumnsButton.Text = "X";
            clearfilterColumnsButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            clearfilterColumnsButton.UseVisualStyleBackColor = true;
            // 
            // showSelectedFieldsRadioButton
            // 
            showSelectedFieldsRadioButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            showSelectedFieldsRadioButton.AutoSize = true;
            mainTableLayoutPanel.SetColumnSpan(showSelectedFieldsRadioButton, 3);
            showSelectedFieldsRadioButton.Location = new System.Drawing.Point(4, 38);
            showSelectedFieldsRadioButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            showSelectedFieldsRadioButton.Name = "showSelectedFieldsRadioButton";
            showSelectedFieldsRadioButton.Size = new System.Drawing.Size(169, 29);
            showSelectedFieldsRadioButton.TabIndex = 1;
            showSelectedFieldsRadioButton.Text = "Selected Fields (Count: {0}):";
            showSelectedFieldsRadioButton.UseVisualStyleBackColor = true;
            showSelectedFieldsRadioButton.CheckedChanged += showSelectedFieldsRadioButton_CheckedChanged;
            // 
            // allFieldsRadioButton
            // 
            allFieldsRadioButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            allFieldsRadioButton.AutoSize = true;
            allFieldsRadioButton.Checked = true;
            mainTableLayoutPanel.SetColumnSpan(allFieldsRadioButton, 2);
            allFieldsRadioButton.Location = new System.Drawing.Point(4, 8);
            allFieldsRadioButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            allFieldsRadioButton.Name = "allFieldsRadioButton";
            allFieldsRadioButton.Size = new System.Drawing.Size(72, 19);
            allFieldsRadioButton.TabIndex = 0;
            allFieldsRadioButton.TabStop = true;
            allFieldsRadioButton.Text = "All Fields";
            allFieldsRadioButton.UseVisualStyleBackColor = true;
            allFieldsRadioButton.CheckedChanged += allFieldsRadioButton_CheckedChanged;
            // 
            // rememberMyChoiceCheckBox
            // 
            rememberMyChoiceCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            rememberMyChoiceCheckBox.AutoSize = true;
            rememberMyChoiceCheckBox.Location = new System.Drawing.Point(86, 8);
            rememberMyChoiceCheckBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 3);
            rememberMyChoiceCheckBox.Name = "rememberMyChoiceCheckBox";
            rememberMyChoiceCheckBox.Size = new System.Drawing.Size(144, 19);
            rememberMyChoiceCheckBox.TabIndex = 6;
            rememberMyChoiceCheckBox.Text = "Remember My Choice";
            rememberMyChoiceCheckBox.UseVisualStyleBackColor = true;
            // 
            // FieldsToLoadForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(585, 413);
            Controls.Add(mainTableLayoutPanel);
            Icon = Properties.Resources.list_icon_32x32;
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MinimumSize = new System.Drawing.Size(284, 282);
            Name = "FieldsToLoadForm";
            Text = "Select Fields to Load";
            Load += FieldsToLoadForm_Load;
            KeyDown += FieldsToLoadForm_KeyDown;
            mainTableLayoutPanel.ResumeLayout(false);
            mainTableLayoutPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.GroupBox recordCountsGroupBox;
        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.Button doneButton;
        private System.Windows.Forms.Panel fieldsPanel;
        private System.Windows.Forms.TextBox filterColumnsTextbox;
        private System.Windows.Forms.Button clearfilterColumnsButton;
        private System.Windows.Forms.RadioButton showSelectedFieldsRadioButton;
        private System.Windows.Forms.RadioButton allFieldsRadioButton;
        private System.Windows.Forms.CheckBox rememberMyChoiceCheckBox;
    }
}