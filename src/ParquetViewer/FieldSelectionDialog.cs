﻿using Parquet.Schema;
using ParquetViewer.Controls;
using ParquetViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ParquetViewer
{
    public partial class FieldsToLoadForm : FormBase
    {
        private const string SelectAllCheckboxName = "checkbox_selectallfields";
        private const string UnsupportedFieldText = "(Unsupported)";
        private const int DynamicFieldCheckboxYIncrement = 30;
        private const int MaxNumberOfFieldsWeCanRender = 5000;

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
            this.AvailableFields = availableFields?.ToList() ?? new();
            this.PreSelectedFields = preSelectedFields?.ToList() ?? new();
        }

        private void FieldsToLoadForm_Load(object sender, EventArgs e)
        {
            this.CenterToParent();
            this.RenderFieldsCheckboxes(this.AvailableFields, this.PreSelectedFields);
        }

        private void RenderFieldsCheckboxes(List<Field> availableFields, List<string>? preSelectedFields)
        {
            this.fieldsPanel.SuspendLayout(); //Suspending the layout while dynamically adding controls adds significant performance improvement
            this.ClearAndDisposeCheckboxes();
            this.fieldsPanel.VerticalScroll.Value = 0; //Scroll to the top

            try
            {
                if (availableFields is null)
                    return;

                if (availableFields.Count > MaxNumberOfFieldsWeCanRender)
                {
                    this.showSelectedFieldsRadioButton.Enabled = false;
                    this.filterColumnsTextbox.PlaceholderText = $"Too many fields: {availableFields.Count}";
                    return;
                }

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

                        var totalFieldCount = availableFields.Count;
                        var supportedFieldCount = availableFields.Where(IsSupportedFieldType).Count();
                        var unsupportedFieldCount = totalFieldCount - supportedFieldCount;
                        var unsupportedFieldsText = unsupportedFieldCount > 0 ? $" - Unsupported: {unsupportedFieldCount}" : string.Empty;

                        string selectAllCheckBoxText = $"Select All (Count: {supportedFieldCount}{unsupportedFieldsText})";
                        string deselectAllCheckBoxText = $"Deselect All (Count: {supportedFieldCount}{unsupportedFieldsText})";
                        var selectAllCheckbox = new CheckBox()
                        {
                            Name = SelectAllCheckboxName,
                            Text = selectAllCheckBoxText,
                            Tag = SelectAllCheckboxName,
                            Checked = false,
                            Location = new Point(locationX, locationY),
                            AutoSize = true
                        };

                        selectAllCheckbox.CheckedChanged += (object? checkboxSender, EventArgs checkboxEventArgs) =>
                        {
                            var selectAllCheckBox = checkboxSender as CheckBox ?? throw new ArgumentNullException(nameof(checkboxSender));
                            var isChecked = selectAllCheckBox.Enabled && selectAllCheckBox.Checked;
                            var showFilterControls = !(isChecked && string.IsNullOrWhiteSpace(this.filterColumnsTextbox.Text));
                            this.filterColumnsTextbox.Enabled = showFilterControls;
                            this.clearfilterColumnsButton.Enabled = showFilterControls;
                            selectAllCheckbox.Text = isChecked ? deselectAllCheckBoxText : selectAllCheckBoxText;

                            if (!isClearingSelectAllCheckbox)
                            {
                                foreach (Control control in this.fieldsPanel.Controls)
                                {
                                    var isSelectAllCheckbox = control.Tag?.Equals(SelectAllCheckboxName) == true;
                                    if (!isSelectAllCheckbox && control is CheckBox checkbox)
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
                        Checked = preSelectedFields?.Contains(field.Name) == true,
                        Location = new Point(locationX, locationY),
                        AutoSize = true,
                        Enabled = !isUnsupportedFieldType
                    };
                    fieldCheckbox.CheckedChanged += (object? checkboxSender, EventArgs checkboxEventArgs) =>
                    {
                        if (checkboxSender is null)
                            return;

                        var fieldCheckBox = (CheckBox)checkboxSender;

                        if (fieldCheckBox.Checked)
                        {
                            this.PreSelectedFields.Add((string)fieldCheckBox.Tag!);
                            SetSelectedFieldCount();
                        }
                        else
                        {
                            this.PreSelectedFields.Remove((string)fieldCheckBox.Tag!);
                            SetSelectedFieldCount();
                        }

                        if (!fieldCheckBox.Checked)
                        {
                            foreach (Control control in this.fieldsPanel.Controls)
                            {
                                if (control.Tag!.Equals(SelectAllCheckboxName) && control is CheckBox checkbox)
                                {
                                    if (checkbox.Enabled && checkbox.Checked)
                                    {
                                        isClearingSelectAllCheckbox = true;
                                        checkbox.Checked = false;
                                        this.PreSelectedFields.Remove((string)fieldCheckBox.Tag!);
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
                var duplicateFields = checkboxControls.GroupBy(f => f.Text.ToUpperInvariant()).Where(g => g.Count() > 1).SelectMany(g => g).ToList();
                foreach (var duplicateField in duplicateFields)
                {
                    duplicateField.Enabled = false;
                }

                this.fieldsPanel.Controls.AddRange(checkboxControls.ToArray<Control>());
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

        /// <summary>
        /// Makes sure we properly dispose our form controls before removing them from the panel
        /// to avoid memory leaks. Details: https://stackoverflow.com/a/310281/1458738
        /// </summary>
        private void ClearAndDisposeCheckboxes()
        {
            //Dispose each control
            foreach (var checkbox in this.fieldsPanel.Controls)
            {
                try
                {
                    if (checkbox is Control c)
                        c.Dispose();
                }
                catch { /* swallow exception */ }
            }

            //Now we're safe to clear the panel
            this.fieldsPanel.Controls.Clear();
        }

        public static bool IsSupportedFieldType(Field field) =>
            field.SchemaType switch
            {
                SchemaType.Data => true,
                SchemaType.List when field is ListField lf && (lf.Item.SchemaType == SchemaType.Data || lf.Item.SchemaType == SchemaType.Struct) => true,
                SchemaType.Map when field is MapField mp && mp.Key.SchemaType == SchemaType.Data && mp.Value.SchemaType == SchemaType.Data => true,
                SchemaType.Struct when field is StructField sf => sf.Fields.All(IsSupportedFieldType),
                _ => false
            };

        private void allFieldsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.fieldsPanel.Enabled = false;
                this.filterColumnsTextbox.Enabled = false;
                this.clearfilterColumnsButton.Enabled = false;
                this.rememberMyChoiceCheckBox.Enabled = true;
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
                this.rememberMyChoiceCheckBox.Enabled = false;
            }
        }

        private void doneButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.rememberMyChoiceCheckBox.Enabled && this.rememberMyChoiceCheckBox.Checked)
                    AppSettings.AlwaysSelectAllFields = true;
                else
                    AppSettings.AlwaysSelectAllFields = false;

                this.NewSelectedFields.Clear();
                if (this.allFieldsRadioButton.Checked || (this.fieldsPanel.Controls.Find(SelectAllCheckboxName, true).FirstOrDefault() as CheckBox)?.Checked == true)
                {
                    this.NewSelectedFields.AddRange(this.AvailableFields.Where(IsSupportedFieldType).Select(f => f.Name));
                }
                else if (this.PreSelectedFields.Count > 0)
                {
                    this.NewSelectedFields.AddRange(this.PreSelectedFields);
                }
                else
                {
                    MessageBox.Show("Please select at least 1 field", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Concat("Something went wrong:", Environment.NewLine, ex.ToString()), ex.Message);
            }
        }

        private void ShowError(Exception ex, string? customMessage = null, bool showStackTrace = true)
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

        private void clearfilterColumnsButton_Click(object? sender, EventArgs? e)
        {
            this.filterColumnsTextbox.Text = string.Empty;
        }

        private void FieldsToLoadForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                //We need to do this on key down because if there's a message box on screen and the user hits 'esc'
                //the message box is closed on the key down. So if we listen on the key up we will also close the main window.
                this.Close();
            }
        }

        private void SetSelectedFieldCount()
        {
            this.showSelectedFieldsRadioButton.Text = string.Format(_selectedFieldsOnlyLabelTemplate, this.PreSelectedFields?.Count
                ?? this.AvailableFields.Count);
        }

        public override void SetTheme(Theme theme)
        {
            if (DesignMode)
            {
                return;
            }

            base.SetTheme(theme);
            this.doneButton.ForeColor = Color.Black;
            this.clearfilterColumnsButton.ForeColor = Color.Black;
        }
    }
}
