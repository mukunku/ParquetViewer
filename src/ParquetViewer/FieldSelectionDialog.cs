using Parquet.Schema;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ParquetViewer
{
    public partial class FieldsToLoadForm : Form
    {
        private const string SelectAllCheckboxName = "checkbox_selectallfields";
        private const string UnsupportedFieldText = "(Unsupported)";
        private const int DynamicFieldCheckboxYIncrement = 30;

        public List<string> PreSelectedFields { get; set; }
        public List<Field> AvailableFields { get; set; }
        public List<string> NewSelectedFields { get; set; }

        private string _selectedFieldsOnlyLabelTemplate;

        public FieldsToLoadForm()
        {
            InitializeComponent();
            this.AvailableFields ??= new List<Field>();
            this.PreSelectedFields ??= new List<string>();
            this.NewSelectedFields ??= new List<string>();
            this._selectedFieldsOnlyLabelTemplate = this.showSelectedFieldsRadioButton.Text;
            this.SetSelectedFieldCount();
        }

        public FieldsToLoadForm(IEnumerable<Field> availableFields, IEnumerable<string> preSelectedFields) : this()
        {
            this.AvailableFields = availableFields.ToList() ?? new();
            this.PreSelectedFields = preSelectedFields.ToList() ?? new();
        }

        private void FieldsToLoadForm_Load(object sender, EventArgs e)
        {
            this.CenterToParent();
            this.RenderFieldsCheckboxes(this.AvailableFields, this.PreSelectedFields);
        }

        private void RenderFieldsCheckboxes(List<Field> availableFields, List<string> preSelectedFields)
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
                    bool isClearingSelectAllCheckbox = false;

                    var checkboxControls = new List<CheckBox>();
                    foreach (Field field in availableFields)
                    {
                        if (isFirst) //Add toggle all checkbox and some other setting changes
                        {
                            isFirst = false;

                            if (preSelectedFields?.Count > 0)
                            {
                                this.showSelectedFieldsRadioButton.Checked = true;
                                this.SetSelectedFieldCount();
                            }

                            var selectAllCheckbox = new CheckBox()
                            {
                                Name = SelectAllCheckboxName,
                                Text = $"Select All (Count: {availableFields.Count})",
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

                        bool isUnsupportedFieldType = !IsSupportedFieldType(field);
                        var fieldCheckbox = new CheckBox()
                        {
                            Name = string.Concat("checkbox_", field.Name),
                            Text = string.Concat(field.Name, isUnsupportedFieldType ? $" {UnsupportedFieldText}" : string.Empty),
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
                                SetSelectedFieldCount();
                            }
                            else
                            {
                                this.PreSelectedFields.Remove((string)fieldCheckBox.Tag);
                                SetSelectedFieldCount();
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
                                            SetSelectedFieldCount();
                                            break;
                                        }
                                    }
                                }
                            }
                        };
                        checkboxControls.Add(fieldCheckbox);

                        locationY += DynamicFieldCheckboxYIncrement;
                    }

                    //Disable fields with dupe names because we don't support case sensitive fields right now
                    var duplicateFields = checkboxControls?.GroupBy(f => f.Text.ToUpperInvariant()).Where(g => g.Count() > 1).SelectMany(g => g).ToList();
                    foreach(var duplicateField in duplicateFields)
                    {
                        duplicateField.Enabled = false;
                    }

                    this.fieldsPanel.Controls.AddRange(checkboxControls.ToArray<Control>());
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

        public static bool IsSupportedFieldType(Field field) =>
            field.SchemaType switch
            {
                SchemaType.Data => true,
                SchemaType.List when field is ListField lf && lf.Item.SchemaType == SchemaType.Data => true, //we don't support nested lists
                SchemaType.Map when field is MapField mp && mp.Key.SchemaType == SchemaType.Data 
                    &&  mp.Value.SchemaType == SchemaType.Data => true, //we don't support nested maps
                _ => false
            };
         
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
                //Clear filter text so all checked fields are loaded
                clearfilterColumnsButton_Click(null, null); 

                if (this.allFieldsRememberRadioButton.Checked)
                    AppSettings.AlwaysSelectAllFields = true;
                else
                    AppSettings.AlwaysSelectAllFields = false;

                this.NewSelectedFields.Clear();
                if (this.allFieldsRadioButton.Checked || this.allFieldsRememberRadioButton.Checked || ((CheckBox)(this.fieldsPanel.Controls.Find(SelectAllCheckboxName, true)[0])).Checked)
                {
                    foreach (Control control in this.fieldsPanel.Controls)
                    {
                        if (!control.Name.Equals(SelectAllCheckboxName) && !control.Text.Contains(UnsupportedFieldText))
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
                    filteredFields = this.AvailableFields.Where(w => w.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase));
                }
                else
                {
                    char[] charsToTrim = { '"', ' ', '\'' };
                    filteredColumnsNames = filteredColumnsNames.Select(s => s.Trim(charsToTrim)).ToList();
                    filteredFields = this.AvailableFields.Where(w => filteredColumnsNames.Contains(w.Name));
                }

                this.RenderFieldsCheckboxes(filteredFields.ToList(), this.PreSelectedFields);
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

        private void FieldsToLoadForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void SetSelectedFieldCount()
        {
            this.showSelectedFieldsRadioButton.Text = string.Format(_selectedFieldsOnlyLabelTemplate, this.PreSelectedFields?.Count 
                ?? this.AvailableFields.Count);
        }
    }
}
