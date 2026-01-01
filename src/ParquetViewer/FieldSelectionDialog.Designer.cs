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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FieldsToLoadForm));
            mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            doneButton = new System.Windows.Forms.Button();
            clearfilterColumnsButton = new System.Windows.Forms.Button();
            showSelectedFieldsRadioButton = new System.Windows.Forms.RadioButton();
            allFieldsRadioButton = new System.Windows.Forms.RadioButton();
            rememberMyChoiceCheckBox = new StylableCheckBox();
            filterColumnsTextbox = new System.Windows.Forms.TextBox();
            fieldsPanel = new System.Windows.Forms.Panel();
            mainTableLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            resources.ApplyResources(mainTableLayoutPanel, "mainTableLayoutPanel");
            mainTableLayoutPanel.Controls.Add(doneButton, 2, 5);
            mainTableLayoutPanel.Controls.Add(clearfilterColumnsButton, 3, 2);
            mainTableLayoutPanel.Controls.Add(showSelectedFieldsRadioButton, 0, 1);
            mainTableLayoutPanel.Controls.Add(allFieldsRadioButton, 0, 0);
            mainTableLayoutPanel.Controls.Add(rememberMyChoiceCheckBox, 2, 0);
            mainTableLayoutPanel.Controls.Add(filterColumnsTextbox, 0, 2);
            mainTableLayoutPanel.Controls.Add(fieldsPanel, 1, 3);
            mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            // 
            // doneButton
            // 
            resources.ApplyResources(doneButton, "doneButton");
            mainTableLayoutPanel.SetColumnSpan(doneButton, 2);
            doneButton.Name = "doneButton";
            doneButton.UseVisualStyleBackColor = true;
            doneButton.Click += doneButton_Click;
            // 
            // clearfilterColumnsButton
            // 
            resources.ApplyResources(clearfilterColumnsButton, "clearfilterColumnsButton");
            clearfilterColumnsButton.Name = "clearfilterColumnsButton";
            clearfilterColumnsButton.UseVisualStyleBackColor = true;
            clearfilterColumnsButton.Click += clearfilterColumnsButton_Click;
            // 
            // showSelectedFieldsRadioButton
            // 
            resources.ApplyResources(showSelectedFieldsRadioButton, "showSelectedFieldsRadioButton");
            mainTableLayoutPanel.SetColumnSpan(showSelectedFieldsRadioButton, 3);
            showSelectedFieldsRadioButton.Name = "showSelectedFieldsRadioButton";
            showSelectedFieldsRadioButton.UseVisualStyleBackColor = true;
            showSelectedFieldsRadioButton.CheckedChanged += showSelectedFieldsRadioButton_CheckedChanged;
            // 
            // allFieldsRadioButton
            // 
            resources.ApplyResources(allFieldsRadioButton, "allFieldsRadioButton");
            allFieldsRadioButton.Checked = true;
            mainTableLayoutPanel.SetColumnSpan(allFieldsRadioButton, 2);
            allFieldsRadioButton.Name = "allFieldsRadioButton";
            allFieldsRadioButton.TabStop = true;
            allFieldsRadioButton.UseVisualStyleBackColor = true;
            allFieldsRadioButton.CheckedChanged += allFieldsRadioButton_CheckedChanged;
            // 
            // rememberMyChoiceCheckBox
            // 
            resources.ApplyResources(rememberMyChoiceCheckBox, "rememberMyChoiceCheckBox");
            rememberMyChoiceCheckBox.DisabledForeColor = System.Drawing.Color.Empty;
            rememberMyChoiceCheckBox.Name = "rememberMyChoiceCheckBox";
            rememberMyChoiceCheckBox.UseVisualStyleBackColor = true;
            // 
            // filterColumnsTextbox
            // 
            resources.ApplyResources(filterColumnsTextbox, "filterColumnsTextbox");
            mainTableLayoutPanel.SetColumnSpan(filterColumnsTextbox, 3);
            filterColumnsTextbox.Name = "filterColumnsTextbox";
            filterColumnsTextbox.TextChanged += filterColumnsTextbox_TextChanged;
            // 
            // fieldsPanel
            // 
            resources.ApplyResources(fieldsPanel, "fieldsPanel");
            mainTableLayoutPanel.SetColumnSpan(fieldsPanel, 3);
            fieldsPanel.Name = "fieldsPanel";
            mainTableLayoutPanel.SetRowSpan(fieldsPanel, 2);
            // 
            // FieldsToLoadForm
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(mainTableLayoutPanel);
            Icon = Resources.Icons.list_icon_32x32;
            KeyPreview = true;
            Name = "FieldsToLoadForm";
            Load += FieldsToLoadForm_Load;
            KeyDown += FieldsToLoadForm_KeyDown;
            mainTableLayoutPanel.ResumeLayout(false);
            mainTableLayoutPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.Button doneButton;
        private System.Windows.Forms.Panel fieldsPanel;
        private System.Windows.Forms.TextBox filterColumnsTextbox;
        private System.Windows.Forms.Button clearfilterColumnsButton;
        private System.Windows.Forms.RadioButton showSelectedFieldsRadioButton;
        private System.Windows.Forms.RadioButton allFieldsRadioButton;
        private StylableCheckBox rememberMyChoiceCheckBox;
    }
}