using ParquetViewer.Controls;
using ParquetViewer.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ParquetViewer;

public partial class FieldsToLoadForm : FormBase
{
    private const string SELECT_ALL_CHECKBOX_NAME = "checkbox_selectallfields";
    private const int DYNAMIC_FIELD_CHECKBOX_Y_INCREMENT = 30;
    private const int MAX_NUMBER_OF_FIELDS_WE_CAN_RENDER = 5000;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<string> PreSelectedFields { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<string> AvailableFields { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<string> NewSelectedFields { get; set; }

    private readonly string _selectedFieldsOnlyLabelTemplate;

    public FieldsToLoadForm()
    {
        InitializeComponent();
        AvailableFields ??= [];
        PreSelectedFields ??= [];
        NewSelectedFields ??= [];
        _selectedFieldsOnlyLabelTemplate = showSelectedFieldsRadioButton.Text;
        SetSelectedFieldCount();
    }

    public FieldsToLoadForm(IEnumerable<string> availableFields, IEnumerable<string> preSelectedFields) : this()
    {
        AvailableFields = availableFields?.ToList() ?? [];
        PreSelectedFields = preSelectedFields?.ToList() ?? [];
    }

    private void FieldsToLoadForm_Load(object sender, EventArgs e)
    {
        CenterToParent();
        RenderFieldsCheckboxes(AvailableFields, PreSelectedFields);
    }

    private void RenderFieldsCheckboxes(List<string> availableFields, List<string>? preSelectedFields)
    {
        fieldsPanel.SuspendLayout(); //Suspending the layout while dynamically adding controls adds significant performance improvement
        ClearAndDisposeCheckboxes();
        fieldsPanel.VerticalScroll.Value = 0; //Scroll to the top

        try
        {
            if (availableFields is null)
                return;

            if (availableFields.Count > MAX_NUMBER_OF_FIELDS_WE_CAN_RENDER)
            {
                showSelectedFieldsRadioButton.Enabled = false;
                filterColumnsTextbox.PlaceholderText = Resources.Strings.TooManyFieldsErrorFormat.Format(availableFields.Count);
                return;
            }

            int locationX = 0;
            int locationY = 5;
            bool isFirst = true;
            bool isClearingSelectAllCheckbox = false;

            var checkboxControls = new List<CheckBox>();
            foreach (string field in availableFields)
            {
                if (isFirst) //Add toggle all checkbox and some other setting changes
                {
                    isFirst = false;

                    if (preSelectedFields?.Count > 0)
                    {
                        showSelectedFieldsRadioButton.Checked = true;
                        SetSelectedFieldCount();
                    }

                    var totalFieldCount = availableFields.Count;
                    string selectAllCheckBoxText = Resources.Strings.SelectAllCheckmarkTextFormat.Format(totalFieldCount);
                    string deselectAllCheckBoxText = Resources.Strings.DeselectAllCheckmarkTextFormat.Format(totalFieldCount);
                    var selectAllCheckbox = new CheckboxWithTooltip(fieldsPanel)
                    {
                        Name = SELECT_ALL_CHECKBOX_NAME,
                        Text = selectAllCheckBoxText,
                        Tag = SELECT_ALL_CHECKBOX_NAME,
                        Checked = false,
                        DisabledForeColor = _disabledTextColor,
                        Location = new Point(locationX, locationY),
                        AutoSize = true
                    };

                    selectAllCheckbox.CheckedChanged += (object? checkboxSender, EventArgs checkboxEventArgs) =>
                    {
                        var selectAllCheckBox = checkboxSender as CheckBox ?? throw new ArgumentNullException(nameof(checkboxSender));
                        var isChecked = selectAllCheckBox.Enabled && selectAllCheckBox.Checked;
                        var showFilterControls = !(isChecked && string.IsNullOrWhiteSpace(filterColumnsTextbox.Text));
                        filterColumnsTextbox.Enabled = showFilterControls;
                        clearfilterColumnsButton.Enabled = showFilterControls;
                        selectAllCheckbox.Text = isChecked ? deselectAllCheckBoxText : selectAllCheckBoxText;

                        if (!isClearingSelectAllCheckbox)
                        {
                            foreach (Control control in fieldsPanel.Controls)
                            {
                                var isSelectAllCheckbox = control.Tag?.Equals(SELECT_ALL_CHECKBOX_NAME) == true;
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

                    fieldsPanel.Controls.Add(selectAllCheckbox);
                    locationY += DYNAMIC_FIELD_CHECKBOX_Y_INCREMENT;
                }

                var fieldCheckbox = new CheckboxWithTooltip(fieldsPanel)
                {
                    Name = string.Concat("checkbox_", field),
                    Text = field,
                    Tag = field,
                    Checked = preSelectedFields?.Contains(field) == true,
                    Location = new Point(locationX, locationY),
                    DisabledForeColor = _disabledTextColor,
                    AutoSize = true,
                    Enabled = true
                };
                fieldCheckbox.CheckedChanged += (object? checkboxSender, EventArgs checkboxEventArgs) =>
                {
                    if (checkboxSender is null)
                        return;

                    var fieldCheckBox = (CheckBox)checkboxSender;

                    if (fieldCheckBox.Checked)
                    {
                        PreSelectedFields.Add((string)fieldCheckBox.Tag!);
                        SetSelectedFieldCount();
                    }
                    else
                    {
                        PreSelectedFields.Remove((string)fieldCheckBox.Tag!);
                        SetSelectedFieldCount();
                    }

                    if (!fieldCheckBox.Checked)
                    {
                        foreach (Control control in fieldsPanel.Controls)
                        {
                            if (control.Tag!.Equals(SELECT_ALL_CHECKBOX_NAME) && control is CheckBox checkbox)
                            {
                                if (checkbox.Enabled && checkbox.Checked)
                                {
                                    isClearingSelectAllCheckbox = true;
                                    checkbox.Checked = false;
                                    PreSelectedFields.Remove((string)fieldCheckBox.Tag!);
                                    isClearingSelectAllCheckbox = false;
                                    SetSelectedFieldCount();
                                    break;
                                }
                            }
                        }
                    }
                };
                checkboxControls.Add(fieldCheckbox);

                locationY += DYNAMIC_FIELD_CHECKBOX_Y_INCREMENT;
            }

            //Disable fields with dupe names because we don't support case sensitive fields right now
            var duplicateFields = checkboxControls.GroupBy(f => f.Text.ToUpperInvariant()).Where(g => g.Count() > 1).SelectMany(g => g).ToList();
            foreach (var duplicateField in duplicateFields)
            {
                duplicateField.Enabled = false;
            }

            fieldsPanel.Controls.AddRange(checkboxControls.ToArray<Control>());
        }
        catch (Exception ex)
        {
            ShowError(ex, Resources.Errors.FieldListGenerationError, true);
        }
        finally
        {
            fieldsPanel.ResumeLayout();
        }
    }

    /// <summary>
    /// Makes sure we properly dispose our form controls before removing them from the panel
    /// to avoid memory leaks. Details: https://stackoverflow.com/a/310281/1458738
    /// </summary>
    private void ClearAndDisposeCheckboxes()
    {
        //Dispose each control
        foreach (var checkbox in fieldsPanel.Controls)
        {
            if (checkbox is Control c)
                c.DisposeSafely();
        }

        //Now we're safe to clear the panel
        fieldsPanel.Controls.Clear();
    }

    private void AllFieldsRadioButton_CheckedChanged(object sender, EventArgs e)
    {
        if (((RadioButton)sender).Checked)
        {
            fieldsPanel.Enabled = false;
            filterColumnsTextbox.Enabled = false;
            clearfilterColumnsButton.Enabled = false;
            rememberMyChoiceCheckBox.Enabled = true;
            showSelectedFieldsRadioButton.Checked = false;
        }
    }

    private void ShowSelectedFieldsRadioButton_CheckedChanged(object sender, EventArgs e)
    {
        if (((RadioButton)sender).Checked)
        {
            fieldsPanel.Enabled = true;
            filterColumnsTextbox.Enabled = true;
            clearfilterColumnsButton.Enabled = true;
            allFieldsRadioButton.Checked = false;
            rememberMyChoiceCheckBox.Enabled = false;
        }
    }

    private void DoneButton_Click(object sender, EventArgs e)
    {
        try
        {
            if (rememberMyChoiceCheckBox.Enabled && rememberMyChoiceCheckBox.Checked)
                AppSettings.AlwaysSelectAllFields = true;
            else
                AppSettings.AlwaysSelectAllFields = false;

            NewSelectedFields.Clear();
            if (allFieldsRadioButton.Checked)
            {
                NewSelectedFields.AddRange(AvailableFields);
            }
            else if (PreSelectedFields.Count > 0)
            {
                NewSelectedFields.AddRange(PreSelectedFields);
            }
            else
            {
                MessageBox.Show(this,
                    Resources.Errors.SelectAtLeastOneFieldErrorMessage,
                    Resources.Errors.SelectAtLeastOneFieldErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(string.Concat($"{Resources.Errors.GenericErrorMessage}:", Environment.NewLine, ex.ToString()), ex.Message);
        }
    }

    private static void ShowError(Exception ex, string? customMessage = null, bool showStackTrace = true)
    {
        MessageBox.Show(string.Concat(customMessage ?? $"{Resources.Errors.GenericErrorMessage}:", Environment.NewLine, showStackTrace ? ex.ToString() : ex.Message), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void FilterColumnsTextbox_DelayedTextChanged(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(filterColumnsTextbox.Text))
        {
            IEnumerable<string> filteredFields;
            var filteredColumnsNames = filterColumnsTextbox.Text.Split(',').ToList();

            if (filteredColumnsNames.Count == 1)
            {
                var filter = filteredColumnsNames[0];
                filteredFields = AvailableFields.Where(w => w.Contains(filter, StringComparison.InvariantCultureIgnoreCase));
            }
            else
            {
                char[] charsToTrim = ['"', ' ', '\''];
                filteredColumnsNames = filteredColumnsNames.Select(s => s.Trim(charsToTrim)).ToList();
                filteredFields = AvailableFields.Where(w => filteredColumnsNames.Contains(w));
            }

            RenderFieldsCheckboxes(filteredFields.ToList(), PreSelectedFields);
        }
        else
        {
            RenderFieldsCheckboxes(AvailableFields, PreSelectedFields);
        }
    }

    private void ClearfilterColumnsButton_Click(object? sender, EventArgs? e)
    {
        filterColumnsTextbox.Text = string.Empty;
    }

    private void FieldsToLoadForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            //We need to do this on key down because if there's a message box on screen and the user hits 'esc'
            //the message box is closed on the key down. So if we listen on the key up we will also close the main window.
            Close();
        }
    }

    private void SetSelectedFieldCount()
    {
        showSelectedFieldsRadioButton.Text = _selectedFieldsOnlyLabelTemplate
            .Format(PreSelectedFields?.Count ?? AvailableFields.Count);
    }

    private Color _disabledTextColor;
    public override void SetTheme(Theme theme)
    {
        if (DesignMode)
        {
            return;
        }

        base.SetTheme(theme);
        doneButton.ForeColor = Color.Black;
        clearfilterColumnsButton.ForeColor = Color.Black;
        _disabledTextColor = theme.DisabledTextColor;
        rememberMyChoiceCheckBox.DisabledForeColor = _disabledTextColor;
    }
}