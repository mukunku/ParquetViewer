
using ParquetViewer.Controls;
using System.Windows.Forms;

namespace ParquetViewer
{
    partial class MainForm
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
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            mainTableLayoutPanel = new TableLayoutPanel();
            recordsToLabel = new Label();
            recordCountTextBox = new DelayedOnChangedTextBox();
            showRecordsFromLabel = new Label();
            offsetTextBox = new DelayedOnChangedTextBox();
            runQueryButton = new Button();
            searchFilterLabel = new LinkLabel();
            searchFilterTextBox = new TextBox();
            clearFilterButton = new Button();
            mainGridView = new ParquetGridView();
            loadAllRowsButton = new Button();
            openParquetFileDialog = new OpenFileDialog();
            mainMenuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            newToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            openFolderToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator = new ToolStripSeparator();
            saveToolStripMenuItem = new ToolStripMenuItem();
            saveAsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            changeFieldsMenuStripButton = new ToolStripMenuItem();
            changeDateFormatToolStripMenuItem = new ToolStripMenuItem();
            defaultToolStripMenuItem = new ToolStripMenuItem();
            defaultDateOnlyToolStripMenuItem = new ToolStripMenuItem();
            iSO8601ToolStripMenuItem = new ToolStripMenuItem();
            iSO8601DateOnlyToolStripMenuItem = new ToolStripMenuItem();
            iSO8601Alt1ToolStripMenuItem = new ToolStripMenuItem();
            iSO8601Alt2ToolStripMenuItem = new ToolStripMenuItem();
            columnSizingToolStripMenuItem = new ToolStripMenuItem();
            defaultToolStripMenuItem1 = new ToolStripMenuItem();
            columnHeadersToolStripMenuItem = new ToolStripMenuItem();
            columnHeadersContentToolStripMenuItem = new ToolStripMenuItem();
            alwaysLoadAllRecordsToolStripMenuItem = new ToolStripMenuItem();
            toolsToolStripMenuItem = new ToolStripMenuItem();
            getSQLCreateTableScriptToolStripMenuItem = new ToolStripMenuItem();
            metadataViewerToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            userGuideToolStripMenuItem = new ToolStripMenuItem();
            shareAnonymousUsageDataToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            showingRecordCountStatusBarLabel = new ToolStripStatusLabel();
            actualShownRecordCountLabel = new ToolStripStatusLabel();
            recordsTextStatusBarLabel = new ToolStripStatusLabel();
            springStatusBarLabel = new ToolStripStatusLabel();
            showingStatusBarLabel = new ToolStripStatusLabel();
            recordCountStatusBarLabel = new ToolStripStatusLabel();
            outOfStatusBarLabel = new ToolStripStatusLabel();
            totalRowCountStatusBarLabel = new ToolStripStatusLabel();
            mainStatusStrip = new StatusStrip();
            exportFileDialog = new SaveFileDialog();
            openFolderDialog = new FolderBrowserDialog();
            loadAllRowsButtonTooltip = new ToolTip(components);
            mainTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mainGridView).BeginInit();
            mainMenuStrip.SuspendLayout();
            mainStatusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            mainTableLayoutPanel.ColumnCount = 11;
            mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 35F));
            mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 93F));
            mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 117F));
            mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 93F));
            mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 54F));
            mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 54F));
            mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            mainTableLayoutPanel.Controls.Add(recordsToLabel, 8, 0);
            mainTableLayoutPanel.Controls.Add(recordCountTextBox, 9, 0);
            mainTableLayoutPanel.Controls.Add(showRecordsFromLabel, 6, 0);
            mainTableLayoutPanel.Controls.Add(offsetTextBox, 7, 0);
            mainTableLayoutPanel.Controls.Add(runQueryButton, 4, 0);
            mainTableLayoutPanel.Controls.Add(searchFilterLabel, 0, 0);
            mainTableLayoutPanel.Controls.Add(searchFilterTextBox, 2, 0);
            mainTableLayoutPanel.Controls.Add(clearFilterButton, 5, 0);
            mainTableLayoutPanel.Controls.Add(mainGridView, 0, 1);
            mainTableLayoutPanel.Controls.Add(loadAllRowsButton, 10, 0);
            mainTableLayoutPanel.Dock = DockStyle.Fill;
            mainTableLayoutPanel.Location = new System.Drawing.Point(0, 24);
            mainTableLayoutPanel.Margin = new Padding(4, 3, 4, 3);
            mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            mainTableLayoutPanel.RowCount = 4;
            mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));
            mainTableLayoutPanel.Size = new System.Drawing.Size(944, 420);
            mainTableLayoutPanel.TabIndex = 0;
            // 
            // recordsToLabel
            // 
            recordsToLabel.Anchor = AnchorStyles.Right;
            recordsToLabel.Location = new System.Drawing.Point(804, 0);
            recordsToLabel.Margin = new Padding(4, 0, 4, 0);
            recordsToLabel.Name = "recordsToLabel";
            recordsToLabel.Size = new System.Drawing.Size(46, 35);
            recordsToLabel.TabIndex = 3;
            recordsToLabel.Text = "Record Count:";
            recordsToLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // recordCountTextBox
            // 
            recordCountTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            recordCountTextBox.DelayedTextChangedTimeout = 1000;
            recordCountTextBox.Location = new System.Drawing.Point(858, 6);
            recordCountTextBox.Margin = new Padding(4, 3, 4, 3);
            recordCountTextBox.Name = "recordCountTextBox";
            recordCountTextBox.Size = new System.Drawing.Size(62, 23);
            recordCountTextBox.TabIndex = 5;
            recordCountTextBox.TextAlign = HorizontalAlignment.Right;
            recordCountTextBox.DelayedTextChanged += recordsToTextBox_TextChanged;
            recordCountTextBox.KeyPress += recordsToTextBox_KeyPress;
            // 
            // showRecordsFromLabel
            // 
            showRecordsFromLabel.Anchor = AnchorStyles.Right;
            showRecordsFromLabel.Location = new System.Drawing.Point(681, 0);
            showRecordsFromLabel.Margin = new Padding(0, 0, 4, 0);
            showRecordsFromLabel.Name = "showRecordsFromLabel";
            showRecordsFromLabel.Size = new System.Drawing.Size(45, 35);
            showRecordsFromLabel.TabIndex = 1;
            showRecordsFromLabel.Text = "Record Offset:";
            showRecordsFromLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // offsetTextBox
            // 
            offsetTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            offsetTextBox.DelayedTextChangedTimeout = 1000;
            offsetTextBox.Location = new System.Drawing.Point(734, 6);
            offsetTextBox.Margin = new Padding(4, 3, 4, 3);
            offsetTextBox.Name = "offsetTextBox";
            offsetTextBox.Size = new System.Drawing.Size(62, 23);
            offsetTextBox.TabIndex = 4;
            offsetTextBox.TextAlign = HorizontalAlignment.Right;
            offsetTextBox.DelayedTextChanged += offsetTextBox_TextChanged;
            offsetTextBox.KeyPress += offsetTextBox_KeyPress;
            // 
            // runQueryButton
            // 
            runQueryButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            runQueryButton.FlatStyle = FlatStyle.Popup;
            runQueryButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            runQueryButton.ForeColor = System.Drawing.Color.DarkRed;
            runQueryButton.Image = Properties.Resources.exclamation_icon;
            runQueryButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            runQueryButton.Location = new System.Drawing.Point(470, 3);
            runQueryButton.Margin = new Padding(4, 3, 4, 3);
            runQueryButton.Name = "runQueryButton";
            runQueryButton.Size = new System.Drawing.Size(109, 29);
            runQueryButton.TabIndex = 2;
            runQueryButton.Text = "&Execute";
            runQueryButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            runQueryButton.UseVisualStyleBackColor = true;
            runQueryButton.Click += runQueryButton_Click;
            // 
            // searchFilterLabel
            // 
            searchFilterLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            searchFilterLabel.AutoSize = true;
            mainTableLayoutPanel.SetColumnSpan(searchFilterLabel, 2);
            searchFilterLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point);
            searchFilterLabel.LinkColor = System.Drawing.Color.Navy;
            searchFilterLabel.Location = new System.Drawing.Point(4, 9);
            searchFilterLabel.Margin = new Padding(4, 0, 4, 0);
            searchFilterLabel.Name = "searchFilterLabel";
            searchFilterLabel.Size = new System.Drawing.Size(97, 16);
            searchFilterLabel.TabIndex = 7;
            searchFilterLabel.TabStop = true;
            searchFilterLabel.Text = "Filter Query (?):";
            searchFilterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            searchFilterLabel.LinkClicked += searchFilterLabel_Click;
            // 
            // searchFilterTextBox
            // 
            searchFilterTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            mainTableLayoutPanel.SetColumnSpan(searchFilterTextBox, 2);
            searchFilterTextBox.Location = new System.Drawing.Point(109, 6);
            searchFilterTextBox.Margin = new Padding(4, 3, 4, 3);
            searchFilterTextBox.Name = "searchFilterTextBox";
            searchFilterTextBox.PlaceholderText = "WHERE ";
            searchFilterTextBox.Size = new System.Drawing.Size(353, 23);
            searchFilterTextBox.TabIndex = 1;
            searchFilterTextBox.Enter += searchFilterTextBox_Enter;
            searchFilterTextBox.KeyPress += searchFilterTextBox_KeyPress;
            searchFilterTextBox.Leave += searchFilterTextBox_Leave;
            // 
            // clearFilterButton
            // 
            clearFilterButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            clearFilterButton.FlatStyle = FlatStyle.Popup;
            clearFilterButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            clearFilterButton.ForeColor = System.Drawing.Color.Black;
            clearFilterButton.Location = new System.Drawing.Point(587, 3);
            clearFilterButton.Margin = new Padding(4, 3, 4, 3);
            clearFilterButton.Name = "clearFilterButton";
            clearFilterButton.Size = new System.Drawing.Size(85, 29);
            clearFilterButton.TabIndex = 3;
            clearFilterButton.Text = "Clear";
            clearFilterButton.UseVisualStyleBackColor = true;
            clearFilterButton.Click += clearFilterButton_Click;
            // 
            // mainGridView
            // 
            mainGridView.AllowUserToAddRows = false;
            mainGridView.AllowUserToDeleteRows = false;
            mainGridView.AllowUserToOrderColumns = true;
            mainGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mainGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            mainGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            mainGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            mainGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            mainTableLayoutPanel.SetColumnSpan(mainGridView, 11);
            mainGridView.EnableHeadersVisualStyles = false;
            mainGridView.Location = new System.Drawing.Point(4, 38);
            mainGridView.Margin = new Padding(4, 3, 4, 3);
            mainGridView.Name = "mainGridView";
            mainGridView.ReadOnly = true;
            mainGridView.RowHeadersWidth = 24;
            mainTableLayoutPanel.SetRowSpan(mainGridView, 2);
            mainGridView.ShowCellToolTips = false;
            mainGridView.Size = new System.Drawing.Size(936, 356);
            mainGridView.TabIndex = 6;
            mainGridView.DataBindingComplete += mainGridView_DataBindingComplete;
            mainGridView.DataError += MainGridView_DataError;
            // 
            // loadAllRowsButton
            // 
            loadAllRowsButton.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            loadAllRowsButton.BackgroundImageLayout = ImageLayout.Center;
            loadAllRowsButton.Cursor = Cursors.Hand;
            loadAllRowsButton.Enabled = false;
            loadAllRowsButton.FlatAppearance.BorderSize = 0;
            loadAllRowsButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            loadAllRowsButton.FlatStyle = FlatStyle.Flat;
            loadAllRowsButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            loadAllRowsButton.Image = Properties.Resources.next_blue;
            loadAllRowsButton.Location = new System.Drawing.Point(926, 9);
            loadAllRowsButton.Margin = new Padding(2, 0, 5, 0);
            loadAllRowsButton.Name = "loadAllRowsButton";
            loadAllRowsButton.Size = new System.Drawing.Size(13, 16);
            loadAllRowsButton.TabIndex = 8;
            loadAllRowsButtonTooltip.SetToolTip(loadAllRowsButton, "Load all records (CTRL+E)");
            loadAllRowsButton.UseVisualStyleBackColor = true;
            loadAllRowsButton.EnabledChanged += loadAllRowsButton_EnabledChanged;
            loadAllRowsButton.Click += loadAllRowsButton_Click;
            // 
            // openParquetFileDialog
            // 
            openParquetFileDialog.Filter = "Parquet Files|*.parquet;*.snappy;*.gz;*.gzip";
            // 
            // mainMenuStrip
            // 
            mainMenuStrip.BackColor = System.Drawing.SystemColors.Control;
            mainMenuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, toolsToolStripMenuItem, helpToolStripMenuItem });
            mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            mainMenuStrip.Name = "mainMenuStrip";
            mainMenuStrip.Padding = new Padding(7, 2, 0, 2);
            mainMenuStrip.Size = new System.Drawing.Size(944, 24);
            mainMenuStrip.TabIndex = 1;
            mainMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newToolStripMenuItem, openToolStripMenuItem, openFolderToolStripMenuItem, toolStripSeparator, saveToolStripMenuItem, saveAsToolStripMenuItem, toolStripSeparator1, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            newToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("newToolStripMenuItem.Image");
            newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            newToolStripMenuItem.Name = "newToolStripMenuItem";
            newToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            newToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            newToolStripMenuItem.Text = "&New";
            newToolStripMenuItem.Click += newToolStripMenuItem_Click;
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("openToolStripMenuItem.Image");
            openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            openToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            openToolStripMenuItem.Text = "&Open File";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // openFolderToolStripMenuItem
            // 
            openFolderToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("openFolderToolStripMenuItem.Image");
            openFolderToolStripMenuItem.Name = "openFolderToolStripMenuItem";
            openFolderToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.O;
            openFolderToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            openFolderToolStripMenuItem.Text = "&Open Folder";
            openFolderToolStripMenuItem.ToolTipText = "All parquet files in the folder must have the same schema";
            openFolderToolStripMenuItem.Click += openFolderToolStripMenuItem_Click;
            // 
            // toolStripSeparator
            // 
            toolStripSeparator.Name = "toolStripSeparator";
            toolStripSeparator.Size = new System.Drawing.Size(223, 6);
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Enabled = false;
            saveToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("saveToolStripMenuItem.Image");
            saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            saveToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            saveToolStripMenuItem.Text = "&Save";
            saveToolStripMenuItem.Visible = false;
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.AutoSize = false;
            saveAsToolStripMenuItem.Enabled = false;
            saveAsToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("saveAsToolStripMenuItem.Image");
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            saveAsToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            saveAsToolStripMenuItem.Text = "Save Results As";
            saveAsToolStripMenuItem.Click += saveAsToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(223, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.W;
            exitToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { changeFieldsMenuStripButton, changeDateFormatToolStripMenuItem, columnSizingToolStripMenuItem, alwaysLoadAllRecordsToolStripMenuItem });
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            editToolStripMenuItem.Text = "&Edit";
            // 
            // changeFieldsMenuStripButton
            // 
            changeFieldsMenuStripButton.Image = (System.Drawing.Image)resources.GetObject("changeFieldsMenuStripButton.Image");
            changeFieldsMenuStripButton.Name = "changeFieldsMenuStripButton";
            changeFieldsMenuStripButton.ShortcutKeys = Keys.Control | Keys.F;
            changeFieldsMenuStripButton.Size = new System.Drawing.Size(217, 22);
            changeFieldsMenuStripButton.Text = "Add/Remove &Fields";
            changeFieldsMenuStripButton.Click += changeFieldsMenuStripButton_Click;
            // 
            // changeDateFormatToolStripMenuItem
            // 
            changeDateFormatToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { defaultToolStripMenuItem, defaultDateOnlyToolStripMenuItem, iSO8601ToolStripMenuItem, iSO8601DateOnlyToolStripMenuItem, iSO8601Alt1ToolStripMenuItem, iSO8601Alt2ToolStripMenuItem });
            changeDateFormatToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("changeDateFormatToolStripMenuItem.Image");
            changeDateFormatToolStripMenuItem.Name = "changeDateFormatToolStripMenuItem";
            changeDateFormatToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            changeDateFormatToolStripMenuItem.Text = "Date Format";
            // 
            // defaultToolStripMenuItem
            // 
            defaultToolStripMenuItem.Name = "defaultToolStripMenuItem";
            defaultToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            defaultToolStripMenuItem.Tag = "0";
            defaultToolStripMenuItem.Text = "Default";
            defaultToolStripMenuItem.ToolTipText = "Local date format";
            defaultToolStripMenuItem.Click += DateFormatMenuItem_Click;
            // 
            // defaultDateOnlyToolStripMenuItem
            // 
            defaultDateOnlyToolStripMenuItem.Name = "defaultDateOnlyToolStripMenuItem";
            defaultDateOnlyToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            defaultDateOnlyToolStripMenuItem.Tag = "1";
            defaultDateOnlyToolStripMenuItem.Text = "Default (Date Only)";
            defaultDateOnlyToolStripMenuItem.ToolTipText = "Local date format (date only)";
            defaultDateOnlyToolStripMenuItem.Click += DateFormatMenuItem_Click;
            // 
            // iSO8601ToolStripMenuItem
            // 
            iSO8601ToolStripMenuItem.Name = "iSO8601ToolStripMenuItem";
            iSO8601ToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            iSO8601ToolStripMenuItem.Tag = "2";
            iSO8601ToolStripMenuItem.Text = "ISO 8601";
            iSO8601ToolStripMenuItem.ToolTipText = "yyyy-MM-ddTHH:mm:ss.fffZ";
            iSO8601ToolStripMenuItem.Click += DateFormatMenuItem_Click;
            // 
            // iSO8601DateOnlyToolStripMenuItem
            // 
            iSO8601DateOnlyToolStripMenuItem.Name = "iSO8601DateOnlyToolStripMenuItem";
            iSO8601DateOnlyToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            iSO8601DateOnlyToolStripMenuItem.Tag = "3";
            iSO8601DateOnlyToolStripMenuItem.Text = "ISO 8601 (Date Only)";
            iSO8601DateOnlyToolStripMenuItem.ToolTipText = "yyyy-MM-dd";
            iSO8601DateOnlyToolStripMenuItem.Click += DateFormatMenuItem_Click;
            // 
            // iSO8601Alt1ToolStripMenuItem
            // 
            iSO8601Alt1ToolStripMenuItem.Name = "iSO8601Alt1ToolStripMenuItem";
            iSO8601Alt1ToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            iSO8601Alt1ToolStripMenuItem.Tag = "4";
            iSO8601Alt1ToolStripMenuItem.Text = "ISO 8601 (Alt 1)";
            iSO8601Alt1ToolStripMenuItem.ToolTipText = "yyyy-MM-dd HH:mm:ss.fff";
            iSO8601Alt1ToolStripMenuItem.Click += DateFormatMenuItem_Click;
            // 
            // iSO8601Alt2ToolStripMenuItem
            // 
            iSO8601Alt2ToolStripMenuItem.Name = "iSO8601Alt2ToolStripMenuItem";
            iSO8601Alt2ToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            iSO8601Alt2ToolStripMenuItem.Tag = "5";
            iSO8601Alt2ToolStripMenuItem.Text = "ISO 8601 (Alt 2)";
            iSO8601Alt2ToolStripMenuItem.ToolTipText = "yyyy-MM-dd HH:mm:ss";
            iSO8601Alt2ToolStripMenuItem.Click += DateFormatMenuItem_Click;
            // 
            // columnSizingToolStripMenuItem
            // 
            columnSizingToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { defaultToolStripMenuItem1, columnHeadersToolStripMenuItem, columnHeadersContentToolStripMenuItem });
            columnSizingToolStripMenuItem.Name = "columnSizingToolStripMenuItem";
            columnSizingToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            columnSizingToolStripMenuItem.Text = "Column Sizing";
            // 
            // defaultToolStripMenuItem1
            // 
            defaultToolStripMenuItem1.Name = "defaultToolStripMenuItem1";
            defaultToolStripMenuItem1.Size = new System.Drawing.Size(192, 22);
            defaultToolStripMenuItem1.Tag = "None";
            defaultToolStripMenuItem1.Text = "Default";
            defaultToolStripMenuItem1.ToolTipText = "All columns will start with the same size, regardless of their contents";
            defaultToolStripMenuItem1.Click += changeColumnSizingToolStripMenuItem_Click;
            // 
            // columnHeadersToolStripMenuItem
            // 
            columnHeadersToolStripMenuItem.Name = "columnHeadersToolStripMenuItem";
            columnHeadersToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            columnHeadersToolStripMenuItem.Tag = "ColumnHeader";
            columnHeadersToolStripMenuItem.Text = "Fit Column Headers";
            columnHeadersToolStripMenuItem.ToolTipText = "Columns will be as wide as their name requires";
            columnHeadersToolStripMenuItem.Click += changeColumnSizingToolStripMenuItem_Click;
            // 
            // columnHeadersContentToolStripMenuItem
            // 
            columnHeadersContentToolStripMenuItem.Name = "columnHeadersContentToolStripMenuItem";
            columnHeadersContentToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            columnHeadersContentToolStripMenuItem.Tag = "AllCells";
            columnHeadersContentToolStripMenuItem.Text = "Fit Headers && Content";
            columnHeadersContentToolStripMenuItem.ToolTipText = "Column widths will be adjusted to fit all cell contents";
            columnHeadersContentToolStripMenuItem.Click += changeColumnSizingToolStripMenuItem_Click;
            // 
            // alwaysLoadAllRecordsToolStripMenuItem
            // 
            alwaysLoadAllRecordsToolStripMenuItem.Checked = true;
            alwaysLoadAllRecordsToolStripMenuItem.CheckState = CheckState.Checked;
            alwaysLoadAllRecordsToolStripMenuItem.Name = "alwaysLoadAllRecordsToolStripMenuItem";
            alwaysLoadAllRecordsToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            alwaysLoadAllRecordsToolStripMenuItem.Text = "Always Load All Records";
            alwaysLoadAllRecordsToolStripMenuItem.ToolTipText = "When opening a new file, ParquetViewer will load all rows";
            alwaysLoadAllRecordsToolStripMenuItem.Click += alwaysLoadAllRecordsToolStripMenuItem_Click;
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { getSQLCreateTableScriptToolStripMenuItem, metadataViewerToolStripMenuItem });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            toolsToolStripMenuItem.Text = "Tools";
            // 
            // getSQLCreateTableScriptToolStripMenuItem
            // 
            getSQLCreateTableScriptToolStripMenuItem.Enabled = false;
            getSQLCreateTableScriptToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("getSQLCreateTableScriptToolStripMenuItem.Image");
            getSQLCreateTableScriptToolStripMenuItem.Name = "getSQLCreateTableScriptToolStripMenuItem";
            getSQLCreateTableScriptToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            getSQLCreateTableScriptToolStripMenuItem.Text = "Get SQL Create Table Script";
            getSQLCreateTableScriptToolStripMenuItem.ToolTipText = "The schema will match the fields you've loaded";
            getSQLCreateTableScriptToolStripMenuItem.Click += GetSQLCreateTableScriptToolStripMenuItem_Click;
            // 
            // metadataViewerToolStripMenuItem
            // 
            metadataViewerToolStripMenuItem.Enabled = false;
            metadataViewerToolStripMenuItem.Name = "metadataViewerToolStripMenuItem";
            metadataViewerToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.M;
            metadataViewerToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            metadataViewerToolStripMenuItem.Text = "Metadata Viewer";
            metadataViewerToolStripMenuItem.Click += MetadataViewerToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { userGuideToolStripMenuItem, shareAnonymousUsageDataToolStripMenuItem, aboutToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            helpToolStripMenuItem.Text = "&Help";
            // 
            // userGuideToolStripMenuItem
            // 
            userGuideToolStripMenuItem.Image = Properties.Resources.external_link_icon;
            userGuideToolStripMenuItem.Name = "userGuideToolStripMenuItem";
            userGuideToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            userGuideToolStripMenuItem.Text = "User Guide";
            userGuideToolStripMenuItem.Click += userGuideToolStripMenuItem_Click;
            // 
            // shareAnonymousUsageDataToolStripMenuItem
            // 
            shareAnonymousUsageDataToolStripMenuItem.Name = "shareAnonymousUsageDataToolStripMenuItem";
            shareAnonymousUsageDataToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            shareAnonymousUsageDataToolStripMenuItem.Text = "Share Usage Data";
            shareAnonymousUsageDataToolStripMenuItem.ToolTipText = "See About page for link to privacy policy";
            shareAnonymousUsageDataToolStripMenuItem.Click += shareAnonymousUsageDataToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            aboutToolStripMenuItem.Text = "&About...";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // showingRecordCountStatusBarLabel
            // 
            showingRecordCountStatusBarLabel.Name = "showingRecordCountStatusBarLabel";
            showingRecordCountStatusBarLabel.Size = new System.Drawing.Size(56, 17);
            showingRecordCountStatusBarLabel.Text = "Showing:";
            // 
            // actualShownRecordCountLabel
            // 
            actualShownRecordCountLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            actualShownRecordCountLabel.Name = "actualShownRecordCountLabel";
            actualShownRecordCountLabel.Size = new System.Drawing.Size(14, 17);
            actualShownRecordCountLabel.Text = "0";
            // 
            // recordsTextStatusBarLabel
            // 
            recordsTextStatusBarLabel.Name = "recordsTextStatusBarLabel";
            recordsTextStatusBarLabel.Size = new System.Drawing.Size(44, 17);
            recordsTextStatusBarLabel.Text = "Results";
            // 
            // springStatusBarLabel
            // 
            springStatusBarLabel.Name = "springStatusBarLabel";
            springStatusBarLabel.Size = new System.Drawing.Size(692, 17);
            springStatusBarLabel.Spring = true;
            // 
            // showingStatusBarLabel
            // 
            showingStatusBarLabel.Name = "showingStatusBarLabel";
            showingStatusBarLabel.Size = new System.Drawing.Size(49, 17);
            showingStatusBarLabel.Text = "Loaded:";
            showingStatusBarLabel.Click += showingStatusBarLabel_Click;
            // 
            // recordCountStatusBarLabel
            // 
            recordCountStatusBarLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            recordCountStatusBarLabel.Name = "recordCountStatusBarLabel";
            recordCountStatusBarLabel.Size = new System.Drawing.Size(14, 17);
            recordCountStatusBarLabel.Text = "0";
            // 
            // outOfStatusBarLabel
            // 
            outOfStatusBarLabel.Name = "outOfStatusBarLabel";
            outOfStatusBarLabel.Size = new System.Drawing.Size(44, 17);
            outOfStatusBarLabel.Text = "Out of:";
            // 
            // totalRowCountStatusBarLabel
            // 
            totalRowCountStatusBarLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            totalRowCountStatusBarLabel.Name = "totalRowCountStatusBarLabel";
            totalRowCountStatusBarLabel.Size = new System.Drawing.Size(14, 17);
            totalRowCountStatusBarLabel.Text = "0";
            // 
            // mainStatusStrip
            // 
            mainStatusStrip.Items.AddRange(new ToolStripItem[] { showingRecordCountStatusBarLabel, actualShownRecordCountLabel, recordsTextStatusBarLabel, springStatusBarLabel, showingStatusBarLabel, recordCountStatusBarLabel, outOfStatusBarLabel, totalRowCountStatusBarLabel });
            mainStatusStrip.Location = new System.Drawing.Point(0, 422);
            mainStatusStrip.Name = "mainStatusStrip";
            mainStatusStrip.Padding = new Padding(1, 0, 16, 0);
            mainStatusStrip.ShowItemToolTips = true;
            mainStatusStrip.Size = new System.Drawing.Size(944, 22);
            mainStatusStrip.TabIndex = 2;
            mainStatusStrip.Text = "statusStrip1";
            // 
            // exportFileDialog
            // 
            exportFileDialog.DefaultExt = "csv";
            exportFileDialog.Filter = "CSV files|*.csv";
            exportFileDialog.RestoreDirectory = true;
            exportFileDialog.Title = "Choose Save Location";
            // 
            // openFolderDialog
            // 
            openFolderDialog.Description = "Select a folder with parquet files";
            openFolderDialog.ShowNewFolderButton = false;
            // 
            // MainForm
            // 
            AllowDrop = true;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(944, 444);
            Controls.Add(mainStatusStrip);
            Controls.Add(mainTableLayoutPanel);
            Controls.Add(mainMenuStrip);
            Icon = Properties.Resources.parquet_icon_32x32;
            KeyPreview = true;
            MainMenuStrip = mainMenuStrip;
            Margin = new Padding(4, 3, 4, 3);
            MinimumSize = new System.Drawing.Size(791, 398);
            Name = "MainForm";
            Text = "New Parquet File";
            Load += MainForm_Load;
            DragDrop += MainForm_DragDrop;
            DragEnter += MainForm_DragEnter;
            KeyDown += MainForm_KeyDown;
            mainTableLayoutPanel.ResumeLayout(false);
            mainTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)mainGridView).EndInit();
            mainMenuStrip.ResumeLayout(false);
            mainMenuStrip.PerformLayout();
            mainStatusStrip.ResumeLayout(false);
            mainStatusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.OpenFileDialog openParquetFileDialog;
        private System.Windows.Forms.Label recordsToLabel;
        private DelayedOnChangedTextBox recordCountTextBox;
        private System.Windows.Forms.Label showRecordsFromLabel;
        private DelayedOnChangedTextBox offsetTextBox;
        private ParquetGridView mainGridView;
        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeFieldsMenuStripButton;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.TextBox searchFilterTextBox;
        private System.Windows.Forms.Button runQueryButton;
        private System.Windows.Forms.Button clearFilterButton;
        private System.Windows.Forms.ToolStripStatusLabel showingRecordCountStatusBarLabel;
        private System.Windows.Forms.ToolStripStatusLabel actualShownRecordCountLabel;
        private System.Windows.Forms.ToolStripStatusLabel recordsTextStatusBarLabel;
        private System.Windows.Forms.ToolStripStatusLabel springStatusBarLabel;
        private System.Windows.Forms.ToolStripStatusLabel showingStatusBarLabel;
        private System.Windows.Forms.ToolStripStatusLabel recordCountStatusBarLabel;
        private System.Windows.Forms.ToolStripStatusLabel outOfStatusBarLabel;
        private System.Windows.Forms.ToolStripStatusLabel totalRowCountStatusBarLabel;
        private System.Windows.Forms.StatusStrip mainStatusStrip;
        private System.Windows.Forms.LinkLabel searchFilterLabel;
        private System.Windows.Forms.SaveFileDialog exportFileDialog;
        private System.Windows.Forms.ToolStripMenuItem userGuideToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeDateFormatToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defaultToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem iSO8601ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem getSQLCreateTableScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem metadataViewerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem columnSizingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defaultToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem columnHeadersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem columnHeadersContentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defaultDateOnlyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem iSO8601DateOnlyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem iSO8601Alt1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem iSO8601Alt2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem alwaysLoadAllRecordsToolStripMenuItem;
        private System.Windows.Forms.FolderBrowserDialog openFolderDialog;
        private System.Windows.Forms.ToolStripMenuItem openFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem shareAnonymousUsageDataToolStripMenuItem;
        private System.Windows.Forms.Button loadAllRowsButton;
        private ToolTip loadAllRowsButtonTooltip;
    }
}

