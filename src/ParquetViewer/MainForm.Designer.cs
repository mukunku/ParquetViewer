
using ParquetViewer.Controls;

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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            recordsToLabel = new System.Windows.Forms.Label();
            recordCountTextBox = new DelayedOnChangedTextBox();
            showRecordsFromLabel = new System.Windows.Forms.Label();
            offsetTextBox = new DelayedOnChangedTextBox();
            runQueryButton = new System.Windows.Forms.Button();
            searchFilterLabel = new System.Windows.Forms.LinkLabel();
            searchFilterTextBox = new System.Windows.Forms.TextBox();
            clearFilterButton = new System.Windows.Forms.Button();
            mainGridView = new System.Windows.Forms.DataGridView();
            openParquetFileDialog = new System.Windows.Forms.OpenFileDialog();
            mainMenuStrip = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            changeFieldsMenuStripButton = new System.Windows.Forms.ToolStripMenuItem();
            changeDateFormatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            defaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            defaultDateOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            iSO8601ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            iSO8601DateOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            iSO8601Alt1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            iSO8601Alt2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            columnSizingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            defaultToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            columnHeadersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            columnHeadersContentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            rememberRecordCountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            getSQLCreateTableScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            metadataViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            userGuideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            showingRecordCountStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            actualShownRecordCountLabel = new System.Windows.Forms.ToolStripStatusLabel();
            recordsTextStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            springStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            showingStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            recordCountStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            outOfStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            totalRowCountStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            mainStatusStrip = new System.Windows.Forms.StatusStrip();
            exportFileDialog = new System.Windows.Forms.SaveFileDialog();
            openFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            mainTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mainGridView).BeginInit();
            mainMenuStrip.SuspendLayout();
            mainStatusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            mainTableLayoutPanel.ColumnCount = 10;
            mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 93F));
            mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 117F));
            mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 93F));
            mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            mainTableLayoutPanel.Controls.Add(recordsToLabel, 8, 0);
            mainTableLayoutPanel.Controls.Add(recordCountTextBox, 9, 0);
            mainTableLayoutPanel.Controls.Add(showRecordsFromLabel, 6, 0);
            mainTableLayoutPanel.Controls.Add(offsetTextBox, 7, 0);
            mainTableLayoutPanel.Controls.Add(runQueryButton, 4, 0);
            mainTableLayoutPanel.Controls.Add(searchFilterLabel, 0, 0);
            mainTableLayoutPanel.Controls.Add(searchFilterTextBox, 2, 0);
            mainTableLayoutPanel.Controls.Add(clearFilterButton, 5, 0);
            mainTableLayoutPanel.Controls.Add(mainGridView, 0, 1);
            mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            mainTableLayoutPanel.Location = new System.Drawing.Point(0, 24);
            mainTableLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            mainTableLayoutPanel.RowCount = 4;
            mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            mainTableLayoutPanel.Size = new System.Drawing.Size(944, 420);
            mainTableLayoutPanel.TabIndex = 0;
            // 
            // recordsToLabel
            // 
            recordsToLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            recordsToLabel.Location = new System.Drawing.Point(820, 0);
            recordsToLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            recordsToLabel.Name = "recordsToLabel";
            recordsToLabel.Size = new System.Drawing.Size(50, 35);
            recordsToLabel.TabIndex = 3;
            recordsToLabel.Text = "Record Count:";
            recordsToLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // recordCountTextBox
            // 
            recordCountTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            recordCountTextBox.DelayedTextChangedTimeout = 1000;
            recordCountTextBox.Location = new System.Drawing.Point(878, 6);
            recordCountTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            recordCountTextBox.Name = "recordCountTextBox";
            recordCountTextBox.Size = new System.Drawing.Size(62, 23);
            recordCountTextBox.TabIndex = 5;
            recordCountTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            recordCountTextBox.DelayedTextChanged += recordsToTextBox_TextChanged;
            recordCountTextBox.KeyPress += recordsToTextBox_KeyPress;
            // 
            // showRecordsFromLabel
            // 
            showRecordsFromLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            showRecordsFromLabel.Location = new System.Drawing.Point(692, 0);
            showRecordsFromLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            showRecordsFromLabel.Name = "showRecordsFromLabel";
            showRecordsFromLabel.Size = new System.Drawing.Size(50, 35);
            showRecordsFromLabel.TabIndex = 1;
            showRecordsFromLabel.Text = "Record Offset:";
            showRecordsFromLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // offsetTextBox
            // 
            offsetTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            offsetTextBox.DelayedTextChangedTimeout = 1000;
            offsetTextBox.Location = new System.Drawing.Point(750, 6);
            offsetTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            offsetTextBox.Name = "offsetTextBox";
            offsetTextBox.Size = new System.Drawing.Size(62, 23);
            offsetTextBox.TabIndex = 4;
            offsetTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            offsetTextBox.DelayedTextChanged += offsetTextBox_TextChanged;
            offsetTextBox.KeyPress += offsetTextBox_KeyPress;
            // 
            // runQueryButton
            // 
            runQueryButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            runQueryButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            runQueryButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            runQueryButton.ForeColor = System.Drawing.Color.DarkRed;
            runQueryButton.Image = Properties.Resources.exclamation_icon;
            runQueryButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            runQueryButton.Location = new System.Drawing.Point(482, 3);
            runQueryButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            runQueryButton.Name = "runQueryButton";
            runQueryButton.Size = new System.Drawing.Size(109, 29);
            runQueryButton.TabIndex = 2;
            runQueryButton.Text = "&Execute";
            runQueryButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            runQueryButton.UseVisualStyleBackColor = true;
            runQueryButton.Click += runQueryButton_Click;
            // 
            // searchFilterLabel
            // 
            searchFilterLabel.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            searchFilterLabel.AutoSize = true;
            mainTableLayoutPanel.SetColumnSpan(searchFilterLabel, 2);
            searchFilterLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point);
            searchFilterLabel.LinkColor = System.Drawing.Color.Navy;
            searchFilterLabel.Location = new System.Drawing.Point(4, 9);
            searchFilterLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
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
            searchFilterTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            mainTableLayoutPanel.SetColumnSpan(searchFilterTextBox, 2);
            searchFilterTextBox.Location = new System.Drawing.Point(109, 6);
            searchFilterTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            searchFilterTextBox.Name = "searchFilterTextBox";
            searchFilterTextBox.Size = new System.Drawing.Size(365, 23);
            searchFilterTextBox.TabIndex = 1;
            searchFilterTextBox.Text = "WHERE ";
            searchFilterTextBox.KeyPress += searchFilterTextBox_KeyPress;
            // 
            // clearFilterButton
            // 
            clearFilterButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            clearFilterButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            clearFilterButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            clearFilterButton.ForeColor = System.Drawing.Color.Black;
            clearFilterButton.Location = new System.Drawing.Point(599, 3);
            clearFilterButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
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
            mainGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            mainGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            mainGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            mainGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            mainTableLayoutPanel.SetColumnSpan(mainGridView, 10);
            mainGridView.EnableHeadersVisualStyles = false;
            mainGridView.Location = new System.Drawing.Point(4, 38);
            mainGridView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            mainGridView.Name = "mainGridView";
            mainGridView.ReadOnly = true;
            mainGridView.RowHeadersWidth = 24;
            mainTableLayoutPanel.SetRowSpan(mainGridView, 2);
            mainGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            mainGridView.Size = new System.Drawing.Size(936, 356);
            mainGridView.TabIndex = 6;
            mainGridView.CellContentClick += mainGridView_CellContentClick;
            mainGridView.CellMouseEnter += mainGridView_CellMouseEnter;
            mainGridView.CellMouseLeave += mainGridView_CellMouseLeave;
            mainGridView.CellMouseMove += mainGridView_CellMouseMove;
            mainGridView.CellPainting += MainGridView_CellPainting;
            mainGridView.ColumnAdded += MainGridView_ColumnAdded;
            mainGridView.DataBindingComplete += mainGridView_DataBindingComplete;
            mainGridView.MouseClick += mainGridView_MouseClick;
            // 
            // openParquetFileDialog
            // 
            openParquetFileDialog.Filter = "Parquet Files|*.parquet;*.snappy;*.gz;*.gzip";
            // 
            // mainMenuStrip
            // 
            mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, toolsToolStripMenuItem, helpToolStripMenuItem });
            mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            mainMenuStrip.Name = "mainMenuStrip";
            mainMenuStrip.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            mainMenuStrip.Size = new System.Drawing.Size(944, 24);
            mainMenuStrip.TabIndex = 1;
            mainMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { newToolStripMenuItem, openToolStripMenuItem, openFolderToolStripMenuItem, toolStripSeparator, saveToolStripMenuItem, saveAsToolStripMenuItem, toolStripSeparator1, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            newToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("newToolStripMenuItem.Image");
            newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            newToolStripMenuItem.Name = "newToolStripMenuItem";
            newToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N;
            newToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            newToolStripMenuItem.Text = "&New";
            newToolStripMenuItem.Click += newToolStripMenuItem_Click;
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("openToolStripMenuItem.Image");
            openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
            openToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            openToolStripMenuItem.Text = "&Open File";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // openFolderToolStripMenuItem
            // 
            openFolderToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("openFolderToolStripMenuItem.Image");
            openFolderToolStripMenuItem.Name = "openFolderToolStripMenuItem";
            openFolderToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.O;
            openFolderToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            openFolderToolStripMenuItem.Text = "&Open Folder";
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
            saveToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
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
            saveAsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.S;
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
            exitToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W;
            exitToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { changeFieldsMenuStripButton, changeDateFormatToolStripMenuItem, columnSizingToolStripMenuItem, rememberRecordCountToolStripMenuItem });
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            editToolStripMenuItem.Text = "&Edit";
            // 
            // changeFieldsMenuStripButton
            // 
            changeFieldsMenuStripButton.Image = (System.Drawing.Image)resources.GetObject("changeFieldsMenuStripButton.Image");
            changeFieldsMenuStripButton.Name = "changeFieldsMenuStripButton";
            changeFieldsMenuStripButton.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F;
            changeFieldsMenuStripButton.Size = new System.Drawing.Size(217, 22);
            changeFieldsMenuStripButton.Text = "Add/Remove &Fields";
            changeFieldsMenuStripButton.Click += changeFieldsMenuStripButton_Click;
            // 
            // changeDateFormatToolStripMenuItem
            // 
            changeDateFormatToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { defaultToolStripMenuItem, defaultDateOnlyToolStripMenuItem, iSO8601ToolStripMenuItem, iSO8601DateOnlyToolStripMenuItem, iSO8601Alt1ToolStripMenuItem, iSO8601Alt2ToolStripMenuItem });
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
            defaultToolStripMenuItem.Click += DateFormatMenuItem_Click;
            // 
            // defaultDateOnlyToolStripMenuItem
            // 
            defaultDateOnlyToolStripMenuItem.Name = "defaultDateOnlyToolStripMenuItem";
            defaultDateOnlyToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            defaultDateOnlyToolStripMenuItem.Tag = "1";
            defaultDateOnlyToolStripMenuItem.Text = "Default (Date Only)";
            defaultDateOnlyToolStripMenuItem.Click += DateFormatMenuItem_Click;
            // 
            // iSO8601ToolStripMenuItem
            // 
            iSO8601ToolStripMenuItem.Name = "iSO8601ToolStripMenuItem";
            iSO8601ToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            iSO8601ToolStripMenuItem.Tag = "2";
            iSO8601ToolStripMenuItem.Text = "ISO 8601";
            iSO8601ToolStripMenuItem.Click += DateFormatMenuItem_Click;
            // 
            // iSO8601DateOnlyToolStripMenuItem
            // 
            iSO8601DateOnlyToolStripMenuItem.Name = "iSO8601DateOnlyToolStripMenuItem";
            iSO8601DateOnlyToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            iSO8601DateOnlyToolStripMenuItem.Tag = "3";
            iSO8601DateOnlyToolStripMenuItem.Text = "ISO 8601 (Date Only)";
            iSO8601DateOnlyToolStripMenuItem.Click += DateFormatMenuItem_Click;
            // 
            // iSO8601Alt1ToolStripMenuItem
            // 
            iSO8601Alt1ToolStripMenuItem.Name = "iSO8601Alt1ToolStripMenuItem";
            iSO8601Alt1ToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            iSO8601Alt1ToolStripMenuItem.Tag = "4";
            iSO8601Alt1ToolStripMenuItem.Text = "ISO 8601 (Alt 1)";
            iSO8601Alt1ToolStripMenuItem.Click += DateFormatMenuItem_Click;
            // 
            // iSO8601Alt2ToolStripMenuItem
            // 
            iSO8601Alt2ToolStripMenuItem.Name = "iSO8601Alt2ToolStripMenuItem";
            iSO8601Alt2ToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            iSO8601Alt2ToolStripMenuItem.Tag = "5";
            iSO8601Alt2ToolStripMenuItem.Text = "ISO 8601 (Alt 2)";
            iSO8601Alt2ToolStripMenuItem.Click += DateFormatMenuItem_Click;
            // 
            // columnSizingToolStripMenuItem
            // 
            columnSizingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { defaultToolStripMenuItem1, columnHeadersToolStripMenuItem, columnHeadersContentToolStripMenuItem });
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
            defaultToolStripMenuItem1.Click += changeColumnSizingToolStripMenuItem_Click;
            // 
            // columnHeadersToolStripMenuItem
            // 
            columnHeadersToolStripMenuItem.Name = "columnHeadersToolStripMenuItem";
            columnHeadersToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            columnHeadersToolStripMenuItem.Tag = "ColumnHeader";
            columnHeadersToolStripMenuItem.Text = "Fit Column Headers";
            columnHeadersToolStripMenuItem.Click += changeColumnSizingToolStripMenuItem_Click;
            // 
            // columnHeadersContentToolStripMenuItem
            // 
            columnHeadersContentToolStripMenuItem.Name = "columnHeadersContentToolStripMenuItem";
            columnHeadersContentToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            columnHeadersContentToolStripMenuItem.Tag = "AllCells";
            columnHeadersContentToolStripMenuItem.Text = "Fit Headers && Content";
            columnHeadersContentToolStripMenuItem.Click += changeColumnSizingToolStripMenuItem_Click;
            // 
            // rememberRecordCountToolStripMenuItem
            // 
            rememberRecordCountToolStripMenuItem.Checked = true;
            rememberRecordCountToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            rememberRecordCountToolStripMenuItem.Name = "rememberRecordCountToolStripMenuItem";
            rememberRecordCountToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            rememberRecordCountToolStripMenuItem.Text = "Remember Record Count";
            rememberRecordCountToolStripMenuItem.Click += rememberRecordCountToolStripMenuItem_Click;
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { getSQLCreateTableScriptToolStripMenuItem, metadataViewerToolStripMenuItem });
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
            getSQLCreateTableScriptToolStripMenuItem.Click += GetSQLCreateTableScriptToolStripMenuItem_Click;
            // 
            // metadataViewerToolStripMenuItem
            // 
            metadataViewerToolStripMenuItem.Enabled = false;
            metadataViewerToolStripMenuItem.Name = "metadataViewerToolStripMenuItem";
            metadataViewerToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M;
            metadataViewerToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            metadataViewerToolStripMenuItem.Text = "Metadata Viewer";
            metadataViewerToolStripMenuItem.Click += MetadataViewerToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { userGuideToolStripMenuItem, aboutToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            helpToolStripMenuItem.Text = "&Help";
            // 
            // userGuideToolStripMenuItem
            // 
            userGuideToolStripMenuItem.Name = "userGuideToolStripMenuItem";
            userGuideToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            userGuideToolStripMenuItem.Text = "User Guide";
            userGuideToolStripMenuItem.Click += userGuideToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
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
            mainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { showingRecordCountStatusBarLabel, actualShownRecordCountLabel, recordsTextStatusBarLabel, springStatusBarLabel, showingStatusBarLabel, recordCountStatusBarLabel, outOfStatusBarLabel, totalRowCountStatusBarLabel });
            mainStatusStrip.Location = new System.Drawing.Point(0, 422);
            mainStatusStrip.Name = "mainStatusStrip";
            mainStatusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
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
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(944, 444);
            Controls.Add(mainStatusStrip);
            Controls.Add(mainTableLayoutPanel);
            Controls.Add(mainMenuStrip);
            Icon = Properties.Resources.parquet_icon_32x32;
            MainMenuStrip = mainMenuStrip;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MinimumSize = new System.Drawing.Size(791, 398);
            Name = "MainForm";
            Text = "New Parquet File";
            Load += MainForm_Load;
            DragDrop += MainForm_DragDrop;
            DragEnter += MainForm_DragEnter;
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
        private System.Windows.Forms.DataGridView mainGridView;
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
        private System.Windows.Forms.ToolStripMenuItem rememberRecordCountToolStripMenuItem;
        private System.Windows.Forms.FolderBrowserDialog openFolderDialog;
        private System.Windows.Forms.ToolStripMenuItem openFolderToolStripMenuItem;
    }
}

