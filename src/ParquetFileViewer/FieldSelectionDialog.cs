using Parquet.Data;
using System;
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
        public static readonly List<SchemaType> UnsupportedSchemaTypes = new List<SchemaType>() { SchemaType.List, SchemaType.Map, SchemaType.Struct };

        public List<string> PreSelectedFields { get; set; }
        public IEnumerable<Field> AvailableFields { get; set; }
        public List<string> NewSelectedFields { get; set; }
        public List<string> PreserveSelectedFields { get; set; }

        public FieldsToLoadForm()
        {
            InitializeComponent();
            this.AvailableFields = new List<Field>();
            this.PreSelectedFields = new List<string>();
            this.NewSelectedFields = new List<string>();
        }

        public FieldsToLoadForm(IEnumerable<Field> availableFields, IEnumerable<string> preSelectedFields)
        {
            InitializeComponent();
            this.AvailableFields = availableFields;
            this.PreSelectedFields = preSelectedFields.ToList() ?? new List<string>();
            this.NewSelectedFields = new List<string>();
        }

        private void FieldsToLoadForm_Load(object sender, EventArgs e)
        {
            this.CenterToParent();
            this.Text = string.Concat(this.Text, this.AvailableFields?.Count() > 0 ? $" (count: {this.AvailableFields.Count()})" : string.Empty);
            this.RenderFieldsCheckboxes(this.AvailableFields, this.PreSelectedFields);
        }

        private void RenderFieldsCheckboxes(IEnumerable<Field> availableFields, IEnumerable<string> preSelectedFields)
        {
            this.fieldsPanel.SuspendLayout(); //Suspending the layout while dynamically adding controls adds significant performance improvement
            this.fieldsPanel.Controls.Clear();

            try
            {
                if (availableFields != null)
                {
                    int locationX = 0;
                    int locationY = 5;
                    bool isFirst = true;
                    HashSet<string> fieldNames = new HashSet<string>();
                    bool isClearingSelectAllCheckbox = false;

                    foreach (Field field in availableFields)
                    {
                        if (isFirst) //Add toggle all checkbox and some other setting changes
                        {
                            isFirst = false;

                            if (preSelectedFields != null)
                            {
                                foreach (string preSelectedField in preSelectedFields)
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
                                var showFilterControls = !(selectAllCheckBox.Enabled && selectAllCheckBox.Checked && string.IsNullOrWhiteSpace(this.filterColumnsTextbox.Text));
                                this.filterColumnsTextbox.Enabled = showFilterControls;
                                this.clearfilterColumnsButton.Enabled = showFilterControls;

                                if (!isClearingSelectAllCheckbox)
                                {
                                    foreach (Control control in this.fieldsPanel.Controls)
                                    {
                                        if (!control.Tag.Equals(SelectAllCheckboxName) && control is CheckBox checkbox)
                                        {
                                            if (checkbox.Enabled)
                                            {
                                                checkbox.Checked = selectAllCheckBox.Checked;
                                            }
                                        }
                                    }
                                }
                            };

                            this.fieldsPanel.Controls.Add(selectAllCheckbox);
                            locationY += DynamicFieldCheckboxYIncrement;
                        }

                        if (!fieldNames.Contains(field.Name.ToLowerInvariant())) //Normally two fields with the same name shouldn't exist but lets make sure
                        {
                            bool isUnsupportedFieldType = UnsupportedSchemaTypes.Contains(field.SchemaType);
                            var fieldCheckbox = new CheckBox()
                            {
                                Name = string.Concat("checkbox_", field.Name),
                                Text = string.Concat(field.Name, isUnsupportedFieldType ? " (Unsupported)" : string.Empty),
                                Tag = field.Name,
                                Checked = preSelectedFields.Contains(field.Name),
                                Location = new Point(locationX, locationY),
                                AutoSize = true,
                                Enabled = !isUnsupportedFieldType
                            };
                            fieldCheckbox.CheckedChanged += (object checkboxSender, EventArgs checkboxEventArgs) =>
                            {
                                var fieldCheckBox = (CheckBox)checkboxSender;

                                if (fieldCheckBox.Checked)
                                {
                                    this.PreSelectedFields.Add((string)fieldCheckBox.Tag);
                                }
                                else
                                {
                                    this.PreSelectedFields.Remove((string)fieldCheckBox.Tag);
                                }


                                if (!fieldCheckBox.Checked)
                                {
                                    foreach (Control control in this.fieldsPanel.Controls)
                                    {
                                        if (control.Tag.Equals(SelectAllCheckboxName) && control is CheckBox checkbox)
                                        {
                                            if (checkbox.Enabled && checkbox.Checked)
                                            {
                                                isClearingSelectAllCheckbox = true;
                                                checkbox.Checked = false;
                                                this.PreSelectedFields.Remove((string)fieldCheckBox.Tag);
                                                isClearingSelectAllCheckbox = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            };
                            this.fieldsPanel.Controls.Add(fieldCheckbox);

                            locationY += DynamicFieldCheckboxYIncrement;
                            fieldNames.Add(field.Name.ToLowerInvariant());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.ShowError(ex, "Something went wrong while generating the field list.", true);
            }
            finally
            {
                this.fieldsPanel.ResumeLayout();
            }
        }

        private void allFieldsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.fieldsPanel.Enabled = false;
                this.filterColumnsTextbox.Enabled = false;
                this.clearfilterColumnsButton.Enabled = false;
                this.allFieldsRememberRadioButton.Checked = false;
                this.showSelectedFieldsRadioButton.Checked = false;
            }
        }

        private void AllFieldsRememberRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.fieldsPanel.Enabled = false;
                this.filterColumnsTextbox.Enabled = false;
                this.clearfilterColumnsButton.Enabled = false;
                this.allFieldsRadioButton.Checked = false;
                this.showSelectedFieldsRadioButton.Checked = false;
            }
        }

        private void showSelectedFieldsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.fieldsPanel.Enabled = true;
                this.filterColumnsTextbox.Enabled = true;
                this.clearfilterColumnsButton.Enabled = true;
                this.allFieldsRadioButton.Checked = false;
                this.allFieldsRememberRadioButton.Checked = false;
            }
        }

        private void doneButton_Click(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    if (this.allFieldsRememberRadioButton.Checked)
                        AppSettings.AlwaysSelectAllFields = true;
                    else
                        AppSettings.AlwaysSelectAllFields = false;
                }
                catch { /* just in case */ }

                this.NewSelectedFields.Clear();
                if (this.allFieldsRadioButton.Checked || this.allFieldsRememberRadioButton.Checked || ((CheckBox)(this.fieldsPanel.Controls.Find(SelectAllCheckboxName, true)[0])).Checked)
                {
                    foreach (Control control in this.fieldsPanel.Controls)
                    {
                        if (!control.Name.Equals(SelectAllCheckboxName) && control.Enabled)
                            this.NewSelectedFields.Add((string)control.Tag);
                    }
                }
                else
                {
                    foreach (Control control in this.fieldsPanel.Controls)
                    {
                        if (control is CheckBox checkbox && checkbox.Checked && !checkbox.Name.Equals(SelectAllCheckboxName) && checkbox.Enabled)
                            this.NewSelectedFields.Add((string)control.Tag);
                    }

                    if (this.NewSelectedFields.Count == 0)
                    {
                        MessageBox.Show("Please select at least 1 field", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Concat("Something went wrong:", Environment.NewLine, ex.ToString()), ex.Message);
            }
        }

        private void ShowError(Exception ex, string customMessage = null, bool showStackTrace = true)
        {
            MessageBox.Show(string.Concat(customMessage ?? "Something went wrong:", Environment.NewLine, showStackTrace ? ex.ToString() : ex.Message), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        private void filterColumnsTextbox_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.filterColumnsTextbox.Text))
            {
                IEnumerable<Field> filteredFields;
                var filteredColumnsNames = this.filterColumnsTextbox.Text.Split(',').ToList();

                if (filteredColumnsNames.Count == 1)
                {
                    var filter = filteredColumnsNames[0];
                    filteredFields = this.AvailableFields.Where(w => w.Name.Contains(filter));
                }
                else
                {
                    char[] charsToTrim = { '"', ' ', '\'' };
                    filteredColumnsNames = filteredColumnsNames.Select(s => s.Trim(charsToTrim)).ToList();
                    filteredFields = this.AvailableFields.Where(w => filteredColumnsNames.Contains(w.Name));
                }

                this.RenderFieldsCheckboxes(filteredFields, this.PreSelectedFields);
            }
            else
            {
                this.RenderFieldsCheckboxes(this.AvailableFields, this.PreSelectedFields);
            }
        }

        private void clearfilterColumnsButton_Click(object sender, EventArgs e)
        {
            this.filterColumnsTextbox.Text = string.Empty;
        }
    }
}
