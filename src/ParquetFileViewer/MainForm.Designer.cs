
namespace ParquetFileViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.recordsToLabel = new System.Windows.Forms.Label();
            this.recordCountTextBox = new ParquetFileViewer.Controls.DelayedOnChangedTextBox();
            this.showRecordsFromLabel = new System.Windows.Forms.Label();
            this.offsetTextBox = new ParquetFileViewer.Controls.DelayedOnChangedTextBox();
            this.runQueryButton = new System.Windows.Forms.Button();
            this.searchFilterLabel = new System.Windows.Forms.LinkLabel();
            this.searchFilterTextBox = new ParquetFileViewer.Controls.DelayedOnChangedTextBox();
            this.clearFilterButton = new System.Windows.Forms.Button();
            this.mainGridView = new System.Windows.Forms.DataGridView();
            this.openParquetFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.excelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeFieldsMenuStripButton = new System.Windows.Forms.ToolStripMenuItem();
            this.changeDateFormatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iSO8601ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.columnSizingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defaultToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.columnHeadersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.columnHeadersContentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.parquetEngineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defaultParquetEngineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.multithreadedParquetEngineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.getSQLCreateTableScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.metadataViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.userGuideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileSchemaBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.ReadDataBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.showingRecordCountStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.actualShownRecordCountLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.recordsTextLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.springStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.showingStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.recordCountStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.outOfStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.totalRowCountStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainStatusStrip = new System.Windows.Forms.StatusStrip();
            this.exportFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.ExportFileBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.exportTimeWithCSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainGridView)).BeginInit();
            this.mainMenuStrip.SuspendLayout();
            this.mainStatusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.ColumnCount = 10;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 139F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 213F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 267F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 213F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 133F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 133F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.mainTableLayoutPanel.Controls.Add(this.recordsToLabel, 8, 0);
            this.mainTableLayoutPanel.Controls.Add(this.recordCountTextBox, 9, 0);
            this.mainTableLayoutPanel.Controls.Add(this.showRecordsFromLabel, 6, 0);
            this.mainTableLayoutPanel.Controls.Add(this.offsetTextBox, 7, 0);
            this.mainTableLayoutPanel.Controls.Add(this.runQueryButton, 4, 0);
            this.mainTableLayoutPanel.Controls.Add(this.searchFilterLabel, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.searchFilterTextBox, 2, 0);
            this.mainTableLayoutPanel.Controls.Add(this.clearFilterButton, 5, 0);
            this.mainTableLayoutPanel.Controls.Add(this.mainGridView, 0, 1);
            this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(0, 34);
            this.mainTableLayoutPanel.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 4;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(1924, 884);
            this.mainTableLayoutPanel.TabIndex = 0;
            // 
            // recordsToLabel
            // 
            this.recordsToLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.recordsToLabel.AutoSize = true;
            this.recordsToLabel.Location = new System.Drawing.Point(1651, 4);
            this.recordsToLabel.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.recordsToLabel.Name = "recordsToLabel";
            this.recordsToLabel.Size = new System.Drawing.Size(105, 64);
            this.recordsToLabel.TabIndex = 3;
            this.recordsToLabel.Text = "Record Count:";
            this.recordsToLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // recordCountTextBox
            // 
            this.recordCountTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.recordCountTextBox.DelayedTextChangedTimeout = 1000;
            this.recordCountTextBox.Location = new System.Drawing.Point(1772, 17);
            this.recordCountTextBox.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.recordCountTextBox.Name = "recordCountTextBox";
            this.recordCountTextBox.Size = new System.Drawing.Size(144, 38);
            this.recordCountTextBox.TabIndex = 5;
            this.recordCountTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.recordCountTextBox.DelayedTextChanged += new System.EventHandler(this.recordsToTextBox_TextChanged);
            this.recordCountTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.recordsToTextBox_KeyPress);
            // 
            // showRecordsFromLabel
            // 
            this.showRecordsFromLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.showRecordsFromLabel.AutoSize = true;
            this.showRecordsFromLabel.Location = new System.Drawing.Point(1358, 4);
            this.showRecordsFromLabel.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.showRecordsFromLabel.Name = "showRecordsFromLabel";
            this.showRecordsFromLabel.Size = new System.Drawing.Size(105, 64);
            this.showRecordsFromLabel.TabIndex = 1;
            this.showRecordsFromLabel.Text = "Record Offset:";
            this.showRecordsFromLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // offsetTextBox
            // 
            this.offsetTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.offsetTextBox.DelayedTextChangedTimeout = 1000;
            this.offsetTextBox.Location = new System.Drawing.Point(1479, 17);
            this.offsetTextBox.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.offsetTextBox.Name = "offsetTextBox";
            this.offsetTextBox.Size = new System.Drawing.Size(144, 38);
            this.offsetTextBox.TabIndex = 4;
            this.offsetTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.offsetTextBox.DelayedTextChanged += new System.EventHandler(this.offsetTextBox_TextChanged);
            this.offsetTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.offsetTextBox_KeyPress);
            // 
            // runQueryButton
            // 
            this.runQueryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.runQueryButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.runQueryButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.runQueryButton.ForeColor = System.Drawing.Color.DarkRed;
            this.runQueryButton.Image = global::ParquetFileViewer.Properties.Resources.exclamation_icon;
            this.runQueryButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.runQueryButton.Location = new System.Drawing.Point(866, 7);
            this.runQueryButton.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.runQueryButton.Name = "runQueryButton";
            this.runQueryButton.Size = new System.Drawing.Size(251, 58);
            this.runQueryButton.TabIndex = 2;
            this.runQueryButton.Text = "&Execute";
            this.runQueryButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.runQueryButton.UseVisualStyleBackColor = true;
            this.runQueryButton.Click += new System.EventHandler(this.runQueryButton_Click);
            // 
            // searchFilterLabel
            // 
            this.searchFilterLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.searchFilterLabel.AutoSize = true;
            this.mainTableLayoutPanel.SetColumnSpan(this.searchFilterLabel, 2);
            this.searchFilterLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchFilterLabel.LinkColor = System.Drawing.Color.Navy;
            this.searchFilterLabel.Location = new System.Drawing.Point(8, 27);
            this.searchFilterLabel.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.searchFilterLabel.Name = "searchFilterLabel";
            this.searchFilterLabel.Size = new System.Drawing.Size(203, 17);
            this.searchFilterLabel.TabIndex = 7;
            this.searchFilterLabel.TabStop = true;
            this.searchFilterLabel.Text = "Filter Query:";
            this.searchFilterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.searchFilterLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.searchFilterLabel_Click);
            // 
            // searchFilterTextBox
            // 
            this.searchFilterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTableLayoutPanel.SetColumnSpan(this.searchFilterTextBox, 2);
            this.searchFilterTextBox.DelayedTextChangedTimeout = 1000;
            this.searchFilterTextBox.Location = new System.Drawing.Point(227, 17);
            this.searchFilterTextBox.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.searchFilterTextBox.Name = "searchFilterTextBox";
            this.searchFilterTextBox.Size = new System.Drawing.Size(623, 38);
            this.searchFilterTextBox.TabIndex = 1;
            this.searchFilterTextBox.Text = "WHERE ";
            this.searchFilterTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.searchFilterTextBox_KeyPress);
            // 
            // clearFilterButton
            // 
            this.clearFilterButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clearFilterButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.clearFilterButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clearFilterButton.ForeColor = System.Drawing.Color.Black;
            this.clearFilterButton.Location = new System.Drawing.Point(1133, 7);
            this.clearFilterButton.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.clearFilterButton.Name = "clearFilterButton";
            this.clearFilterButton.Size = new System.Drawing.Size(197, 58);
            this.clearFilterButton.TabIndex = 3;
            this.clearFilterButton.Text = "Clear";
            this.clearFilterButton.UseVisualStyleBackColor = true;
            this.clearFilterButton.Click += new System.EventHandler(this.clearFilterButton_Click);
            // 
            // mainGridView
            // 
            this.mainGridView.AllowUserToAddRows = false;
            this.mainGridView.AllowUserToDeleteRows = false;
            this.mainGridView.AllowUserToOrderColumns = true;
            this.mainGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainGridView.ColumnHeadersHeight = 29;
            this.mainGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.mainTableLayoutPanel.SetColumnSpan(this.mainGridView, 10);
            this.mainGridView.Location = new System.Drawing.Point(8, 79);
            this.mainGridView.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.mainGridView.Name = "mainGridView";
            this.mainGridView.ReadOnly = true;
            this.mainGridView.RowHeadersWidth = 51;
            this.mainTableLayoutPanel.SetRowSpan(this.mainGridView, 2);
            this.mainGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.mainGridView.Size = new System.Drawing.Size(1908, 750);
            this.mainGridView.TabIndex = 6;
            this.mainGridView.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.MainGridView_CellPainting);
            this.mainGridView.ColumnAdded += new System.Windows.Forms.DataGridViewColumnEventHandler(this.MainGridView_ColumnAdded);
            this.mainGridView.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.mainGridView_DataBindingComplete);
            this.mainGridView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.mainGridView_MouseClick);
            // 
            // openParquetFileDialog
            // 
            this.openParquetFileDialog.Filter = "Parquet Files|*.parquet;*.snappy;*.gz";
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Padding = new System.Windows.Forms.Padding(16, 5, 0, 5);
            this.mainMenuStrip.Size = new System.Drawing.Size(1924, 34);
            this.mainMenuStrip.TabIndex = 1;
            this.mainMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.toolStripSeparator,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripMenuItem.Image")));
            this.newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.newToolStripMenuItem.Text = "&New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(221, 6);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Enabled = false;
            this.saveToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripMenuItem.Image")));
            this.saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Visible = false;
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cSVToolStripMenuItem,
            this.excelToolStripMenuItem});
            this.saveAsToolStripMenuItem.Enabled = false;
            this.saveAsToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveAsToolStripMenuItem.Image")));
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.saveAsToolStripMenuItem.Text = "Save Results As";
            // 
            // cSVToolStripMenuItem
            // 
            this.cSVToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
            this.cSVToolStripMenuItem.Name = "cSVToolStripMenuItem";
            this.cSVToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.cSVToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.cSVToolStripMenuItem.Text = "CSV";
            this.cSVToolStripMenuItem.Click += new System.EventHandler(this.cSVToolStripMenuItem_Click);
            // 
            // excelToolStripMenuItem
            // 
            this.excelToolStripMenuItem.Name = "excelToolStripMenuItem";
            this.excelToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.E)));
            this.excelToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.excelToolStripMenuItem.Text = "Excel";
            this.excelToolStripMenuItem.Click += new System.EventHandler(this.excelToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(221, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changeFieldsMenuStripButton,
            this.changeDateFormatToolStripMenuItem,
            this.columnSizingToolStripMenuItem,
            this.parquetEngineToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(49, 24);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // changeFieldsMenuStripButton
            // 
            this.changeFieldsMenuStripButton.Name = "changeFieldsMenuStripButton";
            this.changeFieldsMenuStripButton.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.changeFieldsMenuStripButton.Size = new System.Drawing.Size(271, 26);
            this.changeFieldsMenuStripButton.Text = "Add/Remove &Fields";
            this.changeFieldsMenuStripButton.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
            // 
            // changeDateFormatToolStripMenuItem
            // 
            this.changeDateFormatToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.defaultToolStripMenuItem,
            this.iSO8601ToolStripMenuItem,
            this.exportTimeWithCSVToolStripMenuItem});
            this.changeDateFormatToolStripMenuItem.Name = "changeDateFormatToolStripMenuItem";
            this.changeDateFormatToolStripMenuItem.Size = new System.Drawing.Size(271, 26);
            this.changeDateFormatToolStripMenuItem.Text = "Date Format";
            // 
            // defaultToolStripMenuItem
            // 
            this.defaultToolStripMenuItem.Name = "defaultToolStripMenuItem";
            this.defaultToolStripMenuItem.Size = new System.Drawing.Size(234, 26);
            this.defaultToolStripMenuItem.Text = "Default";
            this.defaultToolStripMenuItem.Click += new System.EventHandler(this.DefaultToolStripMenuItem_Click);
            // 
            // iSO8601ToolStripMenuItem
            // 
            this.iSO8601ToolStripMenuItem.Name = "iSO8601ToolStripMenuItem";
            this.iSO8601ToolStripMenuItem.Size = new System.Drawing.Size(234, 26);
            this.iSO8601ToolStripMenuItem.Text = "ISO 8601";
            this.iSO8601ToolStripMenuItem.Click += new System.EventHandler(this.ISO8601ToolStripMenuItem_Click);
            // 
            // columnSizingToolStripMenuItem
            // 
            this.columnSizingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.defaultToolStripMenuItem1,
            this.columnHeadersToolStripMenuItem,
            this.columnHeadersContentToolStripMenuItem});
            this.columnSizingToolStripMenuItem.Name = "columnSizingToolStripMenuItem";
            this.columnSizingToolStripMenuItem.Size = new System.Drawing.Size(271, 26);
            this.columnSizingToolStripMenuItem.Text = "Column Sizing";
            // 
            // defaultToolStripMenuItem1
            // 
            this.defaultToolStripMenuItem1.Name = "defaultToolStripMenuItem1";
            this.defaultToolStripMenuItem1.Size = new System.Drawing.Size(239, 26);
            this.defaultToolStripMenuItem1.Tag = "None";
            this.defaultToolStripMenuItem1.Text = "Default";
            this.defaultToolStripMenuItem1.Click += new System.EventHandler(this.changeColumnSizingToolStripMenuItem_Click);
            // 
            // columnHeadersToolStripMenuItem
            // 
            this.columnHeadersToolStripMenuItem.Name = "columnHeadersToolStripMenuItem";
            this.columnHeadersToolStripMenuItem.Size = new System.Drawing.Size(239, 26);
            this.columnHeadersToolStripMenuItem.Tag = "ColumnHeader";
            this.columnHeadersToolStripMenuItem.Text = "Fit Column Headers";
            this.columnHeadersToolStripMenuItem.Click += new System.EventHandler(this.changeColumnSizingToolStripMenuItem_Click);
            // 
            // columnHeadersContentToolStripMenuItem
            // 
            this.columnHeadersContentToolStripMenuItem.Name = "columnHeadersContentToolStripMenuItem";
            this.columnHeadersContentToolStripMenuItem.Size = new System.Drawing.Size(239, 26);
            this.columnHeadersContentToolStripMenuItem.Tag = "AllCells";
            this.columnHeadersContentToolStripMenuItem.Text = "Fit Headers && Content";
            this.columnHeadersContentToolStripMenuItem.Click += new System.EventHandler(this.changeColumnSizingToolStripMenuItem_Click);
            // 
            // parquetEngineToolStripMenuItem
            // 
            this.parquetEngineToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.defaultParquetEngineToolStripMenuItem,
            this.multithreadedParquetEngineToolStripMenuItem});
            this.parquetEngineToolStripMenuItem.Name = "parquetEngineToolStripMenuItem";
            this.parquetEngineToolStripMenuItem.Size = new System.Drawing.Size(271, 26);
            this.parquetEngineToolStripMenuItem.Text = "Parquet Engine";
            // 
            // defaultParquetEngineToolStripMenuItem
            // 
            this.defaultParquetEngineToolStripMenuItem.Name = "defaultParquetEngineToolStripMenuItem";
            this.defaultParquetEngineToolStripMenuItem.Size = new System.Drawing.Size(186, 26);
            this.defaultParquetEngineToolStripMenuItem.Text = "Default";
            this.defaultParquetEngineToolStripMenuItem.Click += new System.EventHandler(this.DefaultParquetEngineToolStripMenuItem_Click);
            // 
            // multithreadedParquetEngineToolStripMenuItem
            // 
            this.multithreadedParquetEngineToolStripMenuItem.Name = "multithreadedParquetEngineToolStripMenuItem";
            this.multithreadedParquetEngineToolStripMenuItem.Size = new System.Drawing.Size(186, 26);
            this.multithreadedParquetEngineToolStripMenuItem.Text = "Multithreaded";
            this.multithreadedParquetEngineToolStripMenuItem.Click += new System.EventHandler(this.MultithreadedParquetEngineToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.getSQLCreateTableScriptToolStripMenuItem,
            this.metadataViewerToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(58, 24);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // getSQLCreateTableScriptToolStripMenuItem
            // 
            this.getSQLCreateTableScriptToolStripMenuItem.Enabled = false;
            this.getSQLCreateTableScriptToolStripMenuItem.Name = "getSQLCreateTableScriptToolStripMenuItem";
            this.getSQLCreateTableScriptToolStripMenuItem.Size = new System.Drawing.Size(273, 26);
            this.getSQLCreateTableScriptToolStripMenuItem.Text = "Get SQL Create Table Script";
            this.getSQLCreateTableScriptToolStripMenuItem.Click += new System.EventHandler(this.GetSQLCreateTableScriptToolStripMenuItem_Click);
            // 
            // metadataViewerToolStripMenuItem
            // 
            this.metadataViewerToolStripMenuItem.Enabled = false;
            this.metadataViewerToolStripMenuItem.Name = "metadataViewerToolStripMenuItem";
            this.metadataViewerToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this.metadataViewerToolStripMenuItem.Size = new System.Drawing.Size(273, 26);
            this.metadataViewerToolStripMenuItem.Text = "Metadata Viewer";
            this.metadataViewerToolStripMenuItem.Click += new System.EventHandler(this.MetadataViewerToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.userGuideToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // userGuideToolStripMenuItem
            // 
            this.userGuideToolStripMenuItem.Name = "userGuideToolStripMenuItem";
            this.userGuideToolStripMenuItem.Size = new System.Drawing.Size(164, 26);
            this.userGuideToolStripMenuItem.Text = "User Guide";
            this.userGuideToolStripMenuItem.Click += new System.EventHandler(this.userGuideToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(164, 26);
            this.aboutToolStripMenuItem.Text = "&About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // FileSchemaBackgroundWorker
            // 
            this.FileSchemaBackgroundWorker.WorkerSupportsCancellation = true;
            this.FileSchemaBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.FileSchemaBackgroundWorker_DoWork);
            this.FileSchemaBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.FileSchemaBackgroundWorker_RunWorkerCompleted);
            // 
            // ReadDataBackgroundWorker
            // 
            this.ReadDataBackgroundWorker.WorkerSupportsCancellation = true;
            this.ReadDataBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.ReadDataBackgroundWorker_DoWork);
            this.ReadDataBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.ReadDataBackgroundWorker_RunWorkerCompleted);
            // 
            // showingRecordCountStatusBarLabel
            // 
            this.showingRecordCountStatusBarLabel.Name = "showingRecordCountStatusBarLabel";
            this.showingRecordCountStatusBarLabel.Size = new System.Drawing.Size(69, 20);
            this.showingRecordCountStatusBarLabel.Text = "Showing:";
            // 
            // actualShownRecordCountLabel
            // 
            this.actualShownRecordCountLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.actualShownRecordCountLabel.Name = "actualShownRecordCountLabel";
            this.actualShownRecordCountLabel.Size = new System.Drawing.Size(18, 20);
            this.actualShownRecordCountLabel.Text = "0";
            // 
            // recordsTextLabel
            // 
            this.recordsTextLabel.Name = "recordsTextLabel";
            this.recordsTextLabel.Size = new System.Drawing.Size(55, 20);
            this.recordsTextLabel.Text = "Results";
            // 
            // springStatusBarLabel
            // 
            this.springStatusBarLabel.Name = "springStatusBarLabel";
            this.springStatusBarLabel.Size = new System.Drawing.Size(1590, 20);
            this.springStatusBarLabel.Spring = true;
            // 
            // showingStatusBarLabel
            // 
            this.showingStatusBarLabel.Name = "showingStatusBarLabel";
            this.showingStatusBarLabel.Size = new System.Drawing.Size(62, 20);
            this.showingStatusBarLabel.Text = "Loaded:";
            // 
            // recordCountStatusBarLabel
            // 
            this.recordCountStatusBarLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.recordCountStatusBarLabel.Name = "recordCountStatusBarLabel";
            this.recordCountStatusBarLabel.Size = new System.Drawing.Size(18, 20);
            this.recordCountStatusBarLabel.Text = "0";
            // 
            // outOfStatusBarLabel
            // 
            this.outOfStatusBarLabel.Name = "outOfStatusBarLabel";
            this.outOfStatusBarLabel.Size = new System.Drawing.Size(54, 20);
            this.outOfStatusBarLabel.Text = "Out of:";
            // 
            // totalRowCountStatusBarLabel
            // 
            this.totalRowCountStatusBarLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.totalRowCountStatusBarLabel.Name = "totalRowCountStatusBarLabel";
            this.totalRowCountStatusBarLabel.Size = new System.Drawing.Size(18, 20);
            this.totalRowCountStatusBarLabel.Text = "0";
            // 
            // mainStatusStrip
            // 
            this.mainStatusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.mainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showingRecordCountStatusBarLabel,
            this.actualShownRecordCountLabel,
            this.recordsTextLabel,
            this.springStatusBarLabel,
            this.showingStatusBarLabel,
            this.recordCountStatusBarLabel,
            this.outOfStatusBarLabel,
            this.totalRowCountStatusBarLabel});
            this.mainStatusStrip.Location = new System.Drawing.Point(0, 892);
            this.mainStatusStrip.Name = "mainStatusStrip";
            this.mainStatusStrip.Padding = new System.Windows.Forms.Padding(3, 0, 37, 0);
            this.mainStatusStrip.Size = new System.Drawing.Size(1924, 26);
            this.mainStatusStrip.TabIndex = 2;
            this.mainStatusStrip.Text = "statusStrip1";
            // 
            // exportFileDialog
            // 
            this.exportFileDialog.DefaultExt = "csv";
            this.exportFileDialog.Filter = "CSV files|*.csv";
            this.exportFileDialog.RestoreDirectory = true;
            this.exportFileDialog.Title = "Choose Save Location";
            // 
            // ExportFileBackgroundWorker
            // 
            this.ExportFileBackgroundWorker.WorkerSupportsCancellation = true;
            this.ExportFileBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.ExportFileBackgroundWorker_DoWork);
            this.ExportFileBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.ExportFileBackgroundWorker_RunWorkerCompleted);
            // 
            // exportTimeWithCSVToolStripMenuItem
            // 
            this.exportTimeWithCSVToolStripMenuItem.Name = "exportTimeWithCSVToolStripMenuItem";
            this.exportTimeWithCSVToolStripMenuItem.Size = new System.Drawing.Size(234, 26);
            this.exportTimeWithCSVToolStripMenuItem.Text = "Export Time with CSV";
            this.exportTimeWithCSVToolStripMenuItem.Click += new System.EventHandler(this.exportTimeWithCSVToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1924, 918);
            this.Controls.Add(this.mainStatusStrip);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.Controls.Add(this.mainMenuStrip);
            this.Icon = global::ParquetFileViewer.Properties.Resources.parquet_icon_32x32;
            this.MainMenuStrip = this.mainMenuStrip;
            this.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.MinimumSize = new System.Drawing.Size(1783, 770);
            this.Name = "MainForm";
            this.Text = "New Parquet File";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.SizeChanged += new System.EventHandler(this.MainForm_SizeChanged);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.mainTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainGridView)).EndInit();
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.mainStatusStrip.ResumeLayout(false);
            this.mainStatusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.OpenFileDialog openParquetFileDialog;
        private System.Windows.Forms.Label recordsToLabel;
        private ParquetFileViewer.Controls.DelayedOnChangedTextBox recordCountTextBox;
        private System.Windows.Forms.Label showRecordsFromLabel;
        private ParquetFileViewer.Controls.DelayedOnChangedTextBox offsetTextBox;
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
        private System.Windows.Forms.ToolStripMenuItem cSVToolStripMenuItem;
        private ParquetFileViewer.Controls.DelayedOnChangedTextBox searchFilterTextBox;
        private System.ComponentModel.BackgroundWorker FileSchemaBackgroundWorker;
        private System.ComponentModel.BackgroundWorker ReadDataBackgroundWorker;
        private System.Windows.Forms.Button runQueryButton;
        private System.Windows.Forms.Button clearFilterButton;
        private System.Windows.Forms.ToolStripStatusLabel showingRecordCountStatusBarLabel;
        private System.Windows.Forms.ToolStripStatusLabel actualShownRecordCountLabel;
        private System.Windows.Forms.ToolStripStatusLabel recordsTextLabel;
        private System.Windows.Forms.ToolStripStatusLabel springStatusBarLabel;
        private System.Windows.Forms.ToolStripStatusLabel showingStatusBarLabel;
        private System.Windows.Forms.ToolStripStatusLabel recordCountStatusBarLabel;
        private System.Windows.Forms.ToolStripStatusLabel outOfStatusBarLabel;
        private System.Windows.Forms.ToolStripStatusLabel totalRowCountStatusBarLabel;
        private System.Windows.Forms.StatusStrip mainStatusStrip;
        private System.Windows.Forms.LinkLabel searchFilterLabel;
        private System.Windows.Forms.SaveFileDialog exportFileDialog;
        private System.ComponentModel.BackgroundWorker ExportFileBackgroundWorker;
        private System.Windows.Forms.ToolStripMenuItem userGuideToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeDateFormatToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defaultToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem iSO8601ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem parquetEngineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defaultParquetEngineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem multithreadedParquetEngineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem getSQLCreateTableScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem metadataViewerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem columnSizingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defaultToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem columnHeadersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem columnHeadersContentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem excelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportTimeWithCSVToolStripMenuItem;
    }
}

