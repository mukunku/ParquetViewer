using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ParquetFileViewer
{
    public partial class FieldsToLoadForm : Form
    {
        private const string SelectAllCheckboxName = "checkbox_selectallfields";
        private const int DynamicFieldCheckboxYIncrement = 30;

        public IEnumerable<string> PreSelectedFields { get; set; }
        public IEnumerable<string> AvailableFields { get; set; }

        public List<string> NewSelectedFields { get; set; }

        public FieldsToLoadForm()
        {
            InitializeComponent();
            this.AvailableFields = new List<string>();
            this.PreSelectedFields = new List<string>();
            this.NewSelectedFields = new List<string>();
        }

        public FieldsToLoadForm(IEnumerable<string> availableFields, IEnumerable<string> preSelectedFields)
        {
            InitializeComponent();
            this.AvailableFields = availableFields;
            this.PreSelectedFields = preSelectedFields ?? new List<string>();
            this.NewSelectedFields = new List<string>();
        }

        private void FieldsToLoadForm_Load(object sender, EventArgs e)
        {
            this.CenterToParent();
            this.fieldsPanel.Controls.Clear();

            if (this.AvailableFields != null)
            {
                int locationX = 0;
                int locationY = 5;
                bool isFirst = true;
                Hashtable fieldNames = new Hashtable();

                foreach (string field in this.AvailableFields)
                {
                    if (isFirst) //Add toggle all checkbox and some other setting changes
                    {
                        isFirst = false;

                        if (this.PreSelectedFields != null)
                        {
                            foreach (string preSelectedField in this.PreSelectedFields)
                            {
                                this.showSelectedFieldsRadioButton.Checked = true;
                                break;
                            }
                        }

                        var selectAllCheckbox = new CheckBox()
                        {
                            Name = SelectAllCheckboxName,
                            Text = "Select All",
                            Tag = SelectAllCheckboxName,
                            Checked = false,
                            Location = new Point(locationX, locationY),
                            AutoSize = true
                        };

                        selectAllCheckbox.CheckedChanged += (object checkboxSender, EventArgs checkboxEventArgs) =>
                        {
                            var selectAllCheckBox = (CheckBox)checkboxSender;
                            foreach (Control control in this.fieldsPanel.Controls)
                            {
                                if (!control.Tag.Equals(SelectAllCheckboxName) && control is CheckBox)
                                {
                                    ((CheckBox)control).Checked = selectAllCheckBox.Checked;
                                }
                            }
                        };

                        this.fieldsPanel.Controls.Add(selectAllCheckbox);
                        locationY += DynamicFieldCheckboxYIncrement;
                    }

                    if (!fieldNames.ContainsKey(field)) //Normally two fields with the same name shouldn't exist but lets make sure
                    {
                        this.fieldsPanel.Controls.Add(
                            new CheckBox()
                            {
                                Name = string.Concat("checkbox_", field),
                                Text = field,
                                Tag = field,
                                Checked = this.PreSelectedFields.Contains(field),
                                Location = new Point(locationX, locationY),
                                AutoSize = true
                            });

                        locationY += DynamicFieldCheckboxYIncrement;
                        fieldNames.Add(field, field);
                    }
                }
            }
        }

        private void allFieldsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton eventSource = (RadioButton)sender;
            this.showSelectedFieldsRadioButton.Checked = !eventSource.Checked;

            if (eventSource.Checked)
            {
                this.fieldsPanel.Enabled = false;
            }
        }

        private void showSelectedFieldsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton eventSource = (RadioButton)sender;
            this.allFieldsRadioButton.Checked = !eventSource.Checked;

            if (eventSource.Checked)
            {
                this.fieldsPanel.Enabled = true;
            }
        }

        private void doneButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.NewSelectedFields.Clear();
                if (this.allFieldsRadioButton.Checked || ((CheckBox)(this.fieldsPanel.Controls.Find(SelectAllCheckboxName, true)[0])).Checked)
                {
                    foreach(Control control in this.fieldsPanel.Controls)
                    {
                        if (!control.Name.Equals(SelectAllCheckboxName))
                            this.NewSelectedFields.Add((string)control.Tag);
                    }
                }
                else
                {
                    foreach (Control control in this.fieldsPanel.Controls)
                    {
                        if (control is CheckBox && ((CheckBox)control).Checked && !control.Name.Equals(SelectAllCheckboxName))
                            this.NewSelectedFields.Add((string)control.Tag);
                    }

                    if (this.NewSelectedFields.Count == 0)
                    {
                        MessageBox.Show("Please select at least 1 field", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(string.Concat("Something went wrong:", Environment.NewLine, ex.ToString()), ex.Message);
            }
        }
    }
}
