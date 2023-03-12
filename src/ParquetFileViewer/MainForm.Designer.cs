
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
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.recordsToLabel = new System.Windows.Forms.Label();
            this.recordCountTextBox = new ParquetViewer.Controls.DelayedOnChangedTextBox();
            this.showRecordsFromLabel = new System.Windows.Forms.Label();
            this.offsetTextBox = new ParquetViewer.Controls.DelayedOnChangedTextBox();
            this.runQueryButton = new System.Windows.Forms.Button();
            this.searchFilterLabel = new System.Windows.Forms.LinkLabel();
            this.searchFilterTextBox = new System.Windows.Forms.TextBox();
            this.clearFilterButton = new System.Windows.Forms.Button();
            this.mainGridView = new System.Windows.Forms.DataGridView();
            this.openParquetFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeFieldsMenuStripButton = new System.Windows.Forms.ToolStripMenuItem();
            this.changeDateFormatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defaultDateOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iSO8601ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iSO8601DateOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iSO8601Alt1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iSO8601Alt2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.columnSizingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defaultToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.columnHeadersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.columnHeadersContentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rememberRecordCountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.getSQLCreateTableScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.metadataViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.userGuideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showingRecordCountStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.actualShownRecordCountLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.recordsTextStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.springStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.showingStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.recordCountStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.outOfStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.totalRowCountStatusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainStatusStrip = new System.Windows.Forms.StatusStrip();
            this.exportFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.openFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.mainTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainGridView)).BeginInit();
            this.mainMenuStrip.SuspendLayout();
            this.mainStatusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.ColumnCount = 10;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 93F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 117F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 93F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
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
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(0, 24);
            this.mainTableLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 4;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(944, 420);
            this.mainTableLayoutPanel.TabIndex = 0;
            // 
            // recordsToLabel
            // 
            this.recordsToLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.recordsToLabel.Location = new System.Drawing.Point(820, 0);
            this.recordsToLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.recordsToLabel.Name = "recordsToLabel";
            this.recordsToLabel.Size = new System.Drawing.Size(50, 35);
            this.recordsToLabel.TabIndex = 3;
            this.recordsToLabel.Text = "Record Count:";
            this.recordsToLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // recordCountTextBox
            // 
            this.recordCountTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.recordCountTextBox.DelayedTextChangedTimeout = 1000;
            this.recordCountTextBox.Location = new System.Drawing.Point(878, 6);
            this.recordCountTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.recordCountTextBox.Name = "recordCountTextBox";
            this.recordCountTextBox.Size = new System.Drawing.Size(62, 23);
            this.recordCountTextBox.TabIndex = 5;
            this.recordCountTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.recordCountTextBox.DelayedTextChanged += new System.EventHandler(this.recordsToTextBox_TextChanged);
            this.recordCountTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.recordsToTextBox_KeyPress);
            // 
            // showRecordsFromLabel
            // 
            this.showRecordsFromLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.showRecordsFromLabel.Location = new System.Drawing.Point(692, 0);
            this.showRecordsFromLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.showRecordsFromLabel.Name = "showRecordsFromLabel";
            this.showRecordsFromLabel.Size = new System.Drawing.Size(50, 35);
            this.showRecordsFromLabel.TabIndex = 1;
            this.showRecordsFromLabel.Text = "Record Offset:";
            this.showRecordsFromLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // offsetTextBox
            // 
            this.offsetTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.offsetTextBox.DelayedTextChangedTimeout = 1000;
            this.offsetTextBox.Location = new System.Drawing.Point(750, 6);
            this.offsetTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.offsetTextBox.Name = "offsetTextBox";
            this.offsetTextBox.Size = new System.Drawing.Size(62, 23);
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
            this.runQueryButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.runQueryButton.ForeColor = System.Drawing.Color.DarkRed;
            this.runQueryButton.Image = global::ParquetViewer.Properties.Resources.exclamation_icon;
            this.runQueryButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.runQueryButton.Location = new System.Drawing.Point(482, 3);
            this.runQueryButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.runQueryButton.Name = "runQueryButton";
            this.runQueryButton.Size = new System.Drawing.Size(109, 29);
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
            this.searchFilterLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point);
            this.searchFilterLabel.LinkColor = System.Drawing.Color.Navy;
            this.searchFilterLabel.Location = new System.Drawing.Point(4, 9);
            this.searchFilterLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.searchFilterLabel.Name = "searchFilterLabel";
            this.searchFilterLabel.Size = new System.Drawing.Size(97, 16);
            this.searchFilterLabel.TabIndex = 7;
            this.searchFilterLabel.TabStop = true;
            this.searchFilterLabel.Text = "Filter Query (?):";
            this.searchFilterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.searchFilterLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.searchFilterLabel_Click);
            // 
            // searchFilterTextBox
            // 
            this.searchFilterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTableLayoutPanel.SetColumnSpan(this.searchFilterTextBox, 2);
            this.searchFilterTextBox.Location = new System.Drawing.Point(109, 6);
            this.searchFilterTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.searchFilterTextBox.Name = "searchFilterTextBox";
            this.searchFilterTextBox.Size = new System.Drawing.Size(365, 23);
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
            this.clearFilterButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.clearFilterButton.ForeColor = System.Drawing.Color.Black;
            this.clearFilterButton.Location = new System.Drawing.Point(599, 3);
            this.clearFilterButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.clearFilterButton.Name = "clearFilterButton";
            this.clearFilterButton.Size = new System.Drawing.Size(85, 29);
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
            this.mainGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.mainGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.mainGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.mainTableLayoutPanel.SetColumnSpan(this.mainGridView, 10);
            this.mainGridView.EnableHeadersVisualStyles = false;
            this.mainGridView.Location = new System.Drawing.Point(4, 38);
            this.mainGridView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.mainGridView.Name = "mainGridView";
            this.mainGridView.ReadOnly = true;
            this.mainGridView.RowHeadersWidth = 24;
            this.mainTableLayoutPanel.SetRowSpan(this.mainGridView, 2);
            this.mainGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.mainGridView.Size = new System.Drawing.Size(936, 356);
            this.mainGridView.TabIndex = 6;
            this.mainGridView.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.mainGridView_CellMouseEnter);
            this.mainGridView.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.mainGridView_CellMouseLeave);
            this.mainGridView.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.MainGridView_CellPainting);
            this.mainGridView.ColumnAdded += new System.Windows.Forms.DataGridViewColumnEventHandler(this.MainGridView_ColumnAdded);
            this.mainGridView.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.mainGridView_DataBindingComplete);
            this.mainGridView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.mainGridView_MouseClick);
            // 
            // openParquetFileDialog
            // 
            this.openParquetFileDialog.Filter = "Parquet Files|*.parquet;*.snappy;*.gz;*.gzip";
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.mainMenuStrip.Size = new System.Drawing.Size(944, 24);
            this.mainMenuStrip.TabIndex = 1;
            this.mainMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.openFolderToolStripMenuItem,
            this.toolStripSeparator,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripMenuItem.Image")));
            this.newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.newToolStripMenuItem.Text = "&New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.openToolStripMenuItem.Text = "&Open File";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // openFolderToolStripMenuItem
            // 
            this.openFolderToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openFolderToolStripMenuItem.Image")));
            this.openFolderToolStripMenuItem.Name = "openFolderToolStripMenuItem";
            this.openFolderToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.O)));
            this.openFolderToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.openFolderToolStripMenuItem.Text = "&Open Folder";
            this.openFolderToolStripMenuItem.Click += new System.EventHandler(this.openFolderToolStripMenuItem_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(223, 6);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Enabled = false;
            this.saveToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripMenuItem.Image")));
            this.saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Visible = false;
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.AutoSize = false;
            this.saveAsToolStripMenuItem.Enabled = false;
            this.saveAsToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveAsToolStripMenuItem.Image")));
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.saveAsToolStripMenuItem.Text = "Save Results As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(223, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changeFieldsMenuStripButton,
            this.changeDateFormatToolStripMenuItem,
            this.columnSizingToolStripMenuItem,
            this.rememberRecordCountToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // changeFieldsMenuStripButton
            // 
            this.changeFieldsMenuStripButton.Image = ((System.Drawing.Image)(resources.GetObject("changeFieldsMenuStripButton.Image")));
            this.changeFieldsMenuStripButton.Name = "changeFieldsMenuStripButton";
            this.changeFieldsMenuStripButton.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.changeFieldsMenuStripButton.Size = new System.Drawing.Size(217, 22);
            this.changeFieldsMenuStripButton.Text = "Add/Remove &Fields";
            this.changeFieldsMenuStripButton.Click += new System.EventHandler(this.changeFieldsMenuStripButton_Click);
            // 
            // changeDateFormatToolStripMenuItem
            // 
            this.changeDateFormatToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.defaultToolStripMenuItem,
            this.defaultDateOnlyToolStripMenuItem,
            this.iSO8601ToolStripMenuItem,
            this.iSO8601DateOnlyToolStripMenuItem,
            this.iSO8601Alt1ToolStripMenuItem,
            this.iSO8601Alt2ToolStripMenuItem});
            this.changeDateFormatToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("changeDateFormatToolStripMenuItem.Image")));
            this.changeDateFormatToolStripMenuItem.Name = "changeDateFormatToolStripMenuItem";
            this.changeDateFormatToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.changeDateFormatToolStripMenuItem.Text = "Date Format";
            // 
            // defaultToolStripMenuItem
            // 
            this.defaultToolStripMenuItem.Name = "defaultToolStripMenuItem";
            this.defaultToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.defaultToolStripMenuItem.Tag = "0";
            this.defaultToolStripMenuItem.Text = "Default";
            this.defaultToolStripMenuItem.Click += new System.EventHandler(this.DateFormatMenuItem_Click);
            // 
            // defaultDateOnlyToolStripMenuItem
            // 
            this.defaultDateOnlyToolStripMenuItem.Name = "defaultDateOnlyToolStripMenuItem";
            this.defaultDateOnlyToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.defaultDateOnlyToolStripMenuItem.Tag = "1";
            this.defaultDateOnlyToolStripMenuItem.Text = "Default (Date Only)";
            this.defaultDateOnlyToolStripMenuItem.Click += new System.EventHandler(this.DateFormatMenuItem_Click);
            // 
            // iSO8601ToolStripMenuItem
            // 
            this.iSO8601ToolStripMenuItem.Name = "iSO8601ToolStripMenuItem";
            this.iSO8601ToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.iSO8601ToolStripMenuItem.Tag = "2";
            this.iSO8601ToolStripMenuItem.Text = "ISO 8601";
            this.iSO8601ToolStripMenuItem.Click += new System.EventHandler(this.DateFormatMenuItem_Click);
            // 
            // iSO8601DateOnlyToolStripMenuItem
            // 
            this.iSO8601DateOnlyToolStripMenuItem.Name = "iSO8601DateOnlyToolStripMenuItem";
            this.iSO8601DateOnlyToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.iSO8601DateOnlyToolStripMenuItem.Tag = "3";
            this.iSO8601DateOnlyToolStripMenuItem.Text = "ISO 8601 (Date Only)";
            this.iSO8601DateOnlyToolStripMenuItem.Click += new System.EventHandler(this.DateFormatMenuItem_Click);
            // 
            // iSO8601Alt1ToolStripMenuItem
            // 
            this.iSO8601Alt1ToolStripMenuItem.Name = "iSO8601Alt1ToolStripMenuItem";
            this.iSO8601Alt1ToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.iSO8601Alt1ToolStripMenuItem.Tag = "4";
            this.iSO8601Alt1ToolStripMenuItem.Text = "ISO 8601 (Alt 1)";
            this.iSO8601Alt1ToolStripMenuItem.Click += new System.EventHandler(this.DateFormatMenuItem_Click);
            // 
            // iSO8601Alt2ToolStripMenuItem
            // 
            this.iSO8601Alt2ToolStripMenuItem.Name = "iSO8601Alt2ToolStripMenuItem";
            this.iSO8601Alt2ToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.iSO8601Alt2ToolStripMenuItem.Tag = "5";
            this.iSO8601Alt2ToolStripMenuItem.Text = "ISO 8601 (Alt 2)";
            this.iSO8601Alt2ToolStripMenuItem.Click += new System.EventHandler(this.DateFormatMenuItem_Click);
            // 
            // columnSizingToolStripMenuItem
            // 
            this.columnSizingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.defaultToolStripMenuItem1,
            this.columnHeadersToolStripMenuItem,
            this.columnHeadersContentToolStripMenuItem});
            this.columnSizingToolStripMenuItem.Name = "columnSizingToolStripMenuItem";
            this.columnSizingToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.columnSizingToolStripMenuItem.Text = "Column Sizing";
            // 
            // defaultToolStripMenuItem1
            // 
            this.defaultToolStripMenuItem1.Name = "defaultToolStripMenuItem1";
            this.defaultToolStripMenuItem1.Size = new System.Drawing.Size(192, 22);
            this.defaultToolStripMenuItem1.Tag = "None";
            this.defaultToolStripMenuItem1.Text = "Default";
            this.defaultToolStripMenuItem1.Click += new System.EventHandler(this.changeColumnSizingToolStripMenuItem_Click);
            // 
            // columnHeadersToolStripMenuItem
            // 
            this.columnHeadersToolStripMenuItem.Name = "columnHeadersToolStripMenuItem";
            this.columnHeadersToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.columnHeadersToolStripMenuItem.Tag = "ColumnHeader";
            this.columnHeadersToolStripMenuItem.Text = "Fit Column Headers";
            this.columnHeadersToolStripMenuItem.Click += new System.EventHandler(this.changeColumnSizingToolStripMenuItem_Click);
            // 
            // columnHeadersContentToolStripMenuItem
            // 
            this.columnHeadersContentToolStripMenuItem.Name = "columnHeadersContentToolStripMenuItem";
            this.columnHeadersContentToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.columnHeadersContentToolStripMenuItem.Tag = "AllCells";
            this.columnHeadersContentToolStripMenuItem.Text = "Fit Headers && Content";
            this.columnHeadersContentToolStripMenuItem.Click += new System.EventHandler(this.changeColumnSizingToolStripMenuItem_Click);
            // 
            // rememberRecordCountToolStripMenuItem
            // 
            this.rememberRecordCountToolStripMenuItem.Checked = true;
            this.rememberRecordCountToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.rememberRecordCountToolStripMenuItem.Name = "rememberRecordCountToolStripMenuItem";
            this.rememberRecordCountToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.rememberRecordCountToolStripMenuItem.Text = "Remember Record Count";
            this.rememberRecordCountToolStripMenuItem.Click += new System.EventHandler(this.rememberRecordCountToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.getSQLCreateTableScriptToolStripMenuItem,
            this.metadataViewerToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // getSQLCreateTableScriptToolStripMenuItem
            // 
            this.getSQLCreateTableScriptToolStripMenuItem.Enabled = false;
            this.getSQLCreateTableScriptToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("getSQLCreateTableScriptToolStripMenuItem.Image")));
            this.getSQLCreateTableScriptToolStripMenuItem.Name = "getSQLCreateTableScriptToolStripMenuItem";
            this.getSQLCreateTableScriptToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.getSQLCreateTableScriptToolStripMenuItem.Text = "Get SQL Create Table Script";
            this.getSQLCreateTableScriptToolStripMenuItem.Click += new System.EventHandler(this.GetSQLCreateTableScriptToolStripMenuItem_Click);
            // 
            // metadataViewerToolStripMenuItem
            // 
            this.metadataViewerToolStripMenuItem.Enabled = false;
            this.metadataViewerToolStripMenuItem.Name = "metadataViewerToolStripMenuItem";
            this.metadataViewerToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this.metadataViewerToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.metadataViewerToolStripMenuItem.Text = "Metadata Viewer";
            this.metadataViewerToolStripMenuItem.Click += new System.EventHandler(this.MetadataViewerToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.userGuideToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // userGuideToolStripMenuItem
            // 
            this.userGuideToolStripMenuItem.Name = "userGuideToolStripMenuItem";
            this.userGuideToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.userGuideToolStripMenuItem.Text = "User Guide";
            this.userGuideToolStripMenuItem.Click += new System.EventHandler(this.userGuideToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.aboutToolStripMenuItem.Text = "&About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // showingRecordCountStatusBarLabel
            // 
            this.showingRecordCountStatusBarLabel.Name = "showingRecordCountStatusBarLabel";
            this.showingRecordCountStatusBarLabel.Size = new System.Drawing.Size(56, 17);
            this.showingRecordCountStatusBarLabel.Text = "Showing:";
            // 
            // actualShownRecordCountLabel
            // 
            this.actualShownRecordCountLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.actualShownRecordCountLabel.Name = "actualShownRecordCountLabel";
            this.actualShownRecordCountLabel.Size = new System.Drawing.Size(14, 17);
            this.actualShownRecordCountLabel.Text = "0";
            // 
            // recordsTextStatusBarLabel
            // 
            this.recordsTextStatusBarLabel.Name = "recordsTextStatusBarLabel";
            this.recordsTextStatusBarLabel.Size = new System.Drawing.Size(44, 17);
            this.recordsTextStatusBarLabel.Text = "Results";
            // 
            // springStatusBarLabel
            // 
            this.springStatusBarLabel.Name = "springStatusBarLabel";
            this.springStatusBarLabel.Size = new System.Drawing.Size(692, 17);
            this.springStatusBarLabel.Spring = true;
            // 
            // showingStatusBarLabel
            // 
            this.showingStatusBarLabel.Name = "showingStatusBarLabel";
            this.showingStatusBarLabel.Size = new System.Drawing.Size(49, 17);
            this.showingStatusBarLabel.Text = "Loaded:";
            // 
            // recordCountStatusBarLabel
            // 
            this.recordCountStatusBarLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.recordCountStatusBarLabel.Name = "recordCountStatusBarLabel";
            this.recordCountStatusBarLabel.Size = new System.Drawing.Size(14, 17);
            this.recordCountStatusBarLabel.Text = "0";
            // 
            // outOfStatusBarLabel
            // 
            this.outOfStatusBarLabel.Name = "outOfStatusBarLabel";
            this.outOfStatusBarLabel.Size = new System.Drawing.Size(44, 17);
            this.outOfStatusBarLabel.Text = "Out of:";
            // 
            // totalRowCountStatusBarLabel
            // 
            this.totalRowCountStatusBarLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.totalRowCountStatusBarLabel.Name = "totalRowCountStatusBarLabel";
            this.totalRowCountStatusBarLabel.Size = new System.Drawing.Size(14, 17);
            this.totalRowCountStatusBarLabel.Text = "0";
            // 
            // mainStatusStrip
            // 
            this.mainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showingRecordCountStatusBarLabel,
            this.actualShownRecordCountLabel,
            this.recordsTextStatusBarLabel,
            this.springStatusBarLabel,
            this.showingStatusBarLabel,
            this.recordCountStatusBarLabel,
            this.outOfStatusBarLabel,
            this.totalRowCountStatusBarLabel});
            this.mainStatusStrip.Location = new System.Drawing.Point(0, 422);
            this.mainStatusStrip.Name = "mainStatusStrip";
            this.mainStatusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.mainStatusStrip.ShowItemToolTips = true;
            this.mainStatusStrip.Size = new System.Drawing.Size(944, 22);
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
            // openFolderDialog
            // 
            this.openFolderDialog.Description = "Select a folder with parquet files";
            this.openFolderDialog.ShowNewFolderButton = false;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(944, 444);
            this.Controls.Add(this.mainStatusStrip);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.Controls.Add(this.mainMenuStrip);
            this.Icon = global::ParquetViewer.Properties.Resources.parquet_icon_32x32;
            this.MainMenuStrip = this.mainMenuStrip;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MinimumSize = new System.Drawing.Size(791, 398);
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

