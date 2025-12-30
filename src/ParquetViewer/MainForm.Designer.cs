
using ParquetViewer.Controls;
using ParquetViewer.Helpers;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
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
            toolStripSeparator = new ThemableToolStripSeperator();
            saveAsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ThemableToolStripSeperator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            changeFieldsMenuStripButton = new ToolStripMenuItem();
            changeDateFormatToolStripMenuItem = new ToolStripMenuItem();
            defaultToolStripMenuItem = new ToolStripMenuItem();
            iSO8601ToolStripMenuItem = new ToolStripMenuItem();
            customDateFormatToolStripMenuItem = new ToolStripMenuItem();
            alwaysLoadAllRecordsToolStripMenuItem = new ToolStripMenuItem();
            darkModeToolStripMenuItem = new ToolStripMenuItem();
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
            resources.ApplyResources(mainTableLayoutPanel, "mainTableLayoutPanel");
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
            mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            loadAllRowsButtonTooltip.SetToolTip(mainTableLayoutPanel, resources.GetString("mainTableLayoutPanel.ToolTip"));
            // 
            // recordsToLabel
            // 
            resources.ApplyResources(recordsToLabel, "recordsToLabel");
            recordsToLabel.Name = "recordsToLabel";
            loadAllRowsButtonTooltip.SetToolTip(recordsToLabel, resources.GetString("recordsToLabel.ToolTip"));
            // 
            // recordCountTextBox
            // 
            resources.ApplyResources(recordCountTextBox, "recordCountTextBox");
            recordCountTextBox.DelayedTextChangedTimeout = 1000;
            recordCountTextBox.Name = "recordCountTextBox";
            loadAllRowsButtonTooltip.SetToolTip(recordCountTextBox, resources.GetString("recordCountTextBox.ToolTip"));
            recordCountTextBox.DelayedTextChanged += recordsToTextBox_TextChanged;
            recordCountTextBox.KeyPress += recordsToTextBox_KeyPress;
            // 
            // showRecordsFromLabel
            // 
            resources.ApplyResources(showRecordsFromLabel, "showRecordsFromLabel");
            showRecordsFromLabel.Name = "showRecordsFromLabel";
            loadAllRowsButtonTooltip.SetToolTip(showRecordsFromLabel, resources.GetString("showRecordsFromLabel.ToolTip"));
            // 
            // offsetTextBox
            // 
            resources.ApplyResources(offsetTextBox, "offsetTextBox");
            offsetTextBox.DelayedTextChangedTimeout = 1000;
            offsetTextBox.Name = "offsetTextBox";
            loadAllRowsButtonTooltip.SetToolTip(offsetTextBox, resources.GetString("offsetTextBox.ToolTip"));
            offsetTextBox.DelayedTextChanged += offsetTextBox_TextChanged;
            offsetTextBox.KeyPress += offsetTextBox_KeyPress;
            // 
            // runQueryButton
            // 
            resources.ApplyResources(runQueryButton, "runQueryButton");
            runQueryButton.ForeColor = System.Drawing.Color.DarkRed;
            runQueryButton.Image = Resources.Icons.exclamation_icon;
            runQueryButton.Name = "runQueryButton";
            loadAllRowsButtonTooltip.SetToolTip(runQueryButton, resources.GetString("runQueryButton.ToolTip"));
            runQueryButton.UseVisualStyleBackColor = true;
            runQueryButton.Click += runQueryButton_Click;
            // 
            // searchFilterLabel
            // 
            resources.ApplyResources(searchFilterLabel, "searchFilterLabel");
            mainTableLayoutPanel.SetColumnSpan(searchFilterLabel, 2);
            searchFilterLabel.LinkColor = System.Drawing.Color.Navy;
            searchFilterLabel.Name = "searchFilterLabel";
            searchFilterLabel.TabStop = true;
            loadAllRowsButtonTooltip.SetToolTip(searchFilterLabel, resources.GetString("searchFilterLabel.ToolTip"));
            searchFilterLabel.LinkClicked += searchFilterLabel_Click;
            // 
            // searchFilterTextBox
            // 
            resources.ApplyResources(searchFilterTextBox, "searchFilterTextBox");
            mainTableLayoutPanel.SetColumnSpan(searchFilterTextBox, 2);
            searchFilterTextBox.Name = "searchFilterTextBox";
            loadAllRowsButtonTooltip.SetToolTip(searchFilterTextBox, resources.GetString("searchFilterTextBox.ToolTip"));
            searchFilterTextBox.Enter += searchFilterTextBox_Enter;
            searchFilterTextBox.KeyPress += searchFilterTextBox_KeyPress;
            searchFilterTextBox.Leave += searchFilterTextBox_Leave;
            // 
            // clearFilterButton
            // 
            resources.ApplyResources(clearFilterButton, "clearFilterButton");
            clearFilterButton.ForeColor = System.Drawing.Color.Black;
            clearFilterButton.Name = "clearFilterButton";
            loadAllRowsButtonTooltip.SetToolTip(clearFilterButton, resources.GetString("clearFilterButton.ToolTip"));
            clearFilterButton.UseVisualStyleBackColor = true;
            clearFilterButton.Click += clearFilterButton_Click;
            // 
            // mainGridView
            // 
            resources.ApplyResources(mainGridView, "mainGridView");
            mainGridView.AllowUserToAddRows = false;
            mainGridView.AllowUserToDeleteRows = false;
            mainGridView.AllowUserToOrderColumns = true;
            mainGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            mainGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            mainGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            mainGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            mainTableLayoutPanel.SetColumnSpan(mainGridView, 11);
            mainGridView.CopyAsWhereIcon = (System.Drawing.Image)resources.GetObject("mainGridView.CopyAsWhereIcon");
            mainGridView.CopyToClipboardIcon = (System.Drawing.Image)resources.GetObject("mainGridView.CopyToClipboardIcon");
            mainGridView.EnableHeadersVisualStyles = false;
            mainGridView.Name = "mainGridView";
            mainGridView.ReadOnly = true;
            mainTableLayoutPanel.SetRowSpan(mainGridView, 2);
            mainGridView.ShowCellToolTips = false;
            mainGridView.ShowCopyAsWhereContextMenuItem = true;
            loadAllRowsButtonTooltip.SetToolTip(mainGridView, resources.GetString("mainGridView.ToolTip"));
            mainGridView.DataBindingComplete += mainGridView_DataBindingComplete;
            // 
            // loadAllRowsButton
            // 
            resources.ApplyResources(loadAllRowsButton, "loadAllRowsButton");
            loadAllRowsButton.Cursor = Cursors.Hand;
            loadAllRowsButton.FlatAppearance.BorderSize = 0;
            loadAllRowsButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            loadAllRowsButton.Image = Resources.Icons.next_blue;
            loadAllRowsButton.Name = "loadAllRowsButton";
            loadAllRowsButtonTooltip.SetToolTip(loadAllRowsButton, resources.GetString("loadAllRowsButton.ToolTip"));
            loadAllRowsButton.UseVisualStyleBackColor = true;
            loadAllRowsButton.EnabledChanged += loadAllRowsButton_EnabledChanged;
            loadAllRowsButton.Click += loadAllRowsButton_Click;
            // 
            // openParquetFileDialog
            // 
            resources.ApplyResources(openParquetFileDialog, "openParquetFileDialog");
            // 
            // mainMenuStrip
            // 
            resources.ApplyResources(mainMenuStrip, "mainMenuStrip");
            mainMenuStrip.BackColor = System.Drawing.SystemColors.Control;
            mainMenuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, toolsToolStripMenuItem, helpToolStripMenuItem });
            mainMenuStrip.Name = "mainMenuStrip";
            loadAllRowsButtonTooltip.SetToolTip(mainMenuStrip, resources.GetString("mainMenuStrip.ToolTip"));
            // 
            // fileToolStripMenuItem
            // 
            resources.ApplyResources(fileToolStripMenuItem, "fileToolStripMenuItem");
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newToolStripMenuItem, openToolStripMenuItem, openFolderToolStripMenuItem, toolStripSeparator, saveAsToolStripMenuItem, toolStripSeparator1, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            // 
            // newToolStripMenuItem
            // 
            resources.ApplyResources(newToolStripMenuItem, "newToolStripMenuItem");
            newToolStripMenuItem.Name = "newToolStripMenuItem";
            newToolStripMenuItem.Click += newToolStripMenuItem_Click;
            // 
            // openToolStripMenuItem
            // 
            resources.ApplyResources(openToolStripMenuItem, "openToolStripMenuItem");
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // openFolderToolStripMenuItem
            // 
            resources.ApplyResources(openFolderToolStripMenuItem, "openFolderToolStripMenuItem");
            openFolderToolStripMenuItem.Name = "openFolderToolStripMenuItem";
            openFolderToolStripMenuItem.Click += openFolderToolStripMenuItem_Click;
            // 
            // toolStripSeparator
            // 
            resources.ApplyResources(toolStripSeparator, "toolStripSeparator");
            toolStripSeparator.Name = "toolStripSeparator";
            // 
            // saveAsToolStripMenuItem
            // 
            resources.ApplyResources(saveAsToolStripMenuItem, "saveAsToolStripMenuItem");
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.Click += saveAsToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            resources.ApplyResources(toolStripSeparator1, "toolStripSeparator1");
            toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // exitToolStripMenuItem
            // 
            resources.ApplyResources(exitToolStripMenuItem, "exitToolStripMenuItem");
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // editToolStripMenuItem
            // 
            resources.ApplyResources(editToolStripMenuItem, "editToolStripMenuItem");
            editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { changeFieldsMenuStripButton, changeDateFormatToolStripMenuItem, alwaysLoadAllRecordsToolStripMenuItem, darkModeToolStripMenuItem });
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            // 
            // changeFieldsMenuStripButton
            // 
            resources.ApplyResources(changeFieldsMenuStripButton, "changeFieldsMenuStripButton");
            changeFieldsMenuStripButton.Name = "changeFieldsMenuStripButton";
            changeFieldsMenuStripButton.Click += changeFieldsMenuStripButton_Click;
            // 
            // changeDateFormatToolStripMenuItem
            // 
            resources.ApplyResources(changeDateFormatToolStripMenuItem, "changeDateFormatToolStripMenuItem");
            changeDateFormatToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { defaultToolStripMenuItem, iSO8601ToolStripMenuItem, customDateFormatToolStripMenuItem });
            changeDateFormatToolStripMenuItem.Name = "changeDateFormatToolStripMenuItem";
            // 
            // defaultToolStripMenuItem
            // 
            resources.ApplyResources(defaultToolStripMenuItem, "defaultToolStripMenuItem");
            defaultToolStripMenuItem.Name = "defaultToolStripMenuItem";
            defaultToolStripMenuItem.Tag = "0";
            defaultToolStripMenuItem.Click += DateFormatMenuItem_Click;
            // 
            // iSO8601ToolStripMenuItem
            // 
            resources.ApplyResources(iSO8601ToolStripMenuItem, "iSO8601ToolStripMenuItem");
            iSO8601ToolStripMenuItem.Name = "iSO8601ToolStripMenuItem";
            iSO8601ToolStripMenuItem.Tag = "2";
            iSO8601ToolStripMenuItem.Click += DateFormatMenuItem_Click;
            // 
            // customDateFormatToolStripMenuItem
            // 
            resources.ApplyResources(customDateFormatToolStripMenuItem, "customDateFormatToolStripMenuItem");
            customDateFormatToolStripMenuItem.Name = "customDateFormatToolStripMenuItem";
            customDateFormatToolStripMenuItem.Tag = "6";
            customDateFormatToolStripMenuItem.Click += DateFormatMenuItem_Click;
            // 
            // alwaysLoadAllRecordsToolStripMenuItem
            // 
            resources.ApplyResources(alwaysLoadAllRecordsToolStripMenuItem, "alwaysLoadAllRecordsToolStripMenuItem");
            alwaysLoadAllRecordsToolStripMenuItem.Checked = true;
            alwaysLoadAllRecordsToolStripMenuItem.CheckState = CheckState.Checked;
            alwaysLoadAllRecordsToolStripMenuItem.Name = "alwaysLoadAllRecordsToolStripMenuItem";
            alwaysLoadAllRecordsToolStripMenuItem.Click += alwaysLoadAllRecordsToolStripMenuItem_Click;
            // 
            // darkModeToolStripMenuItem
            // 
            resources.ApplyResources(darkModeToolStripMenuItem, "darkModeToolStripMenuItem");
            darkModeToolStripMenuItem.Name = "darkModeToolStripMenuItem";
            darkModeToolStripMenuItem.Click += darkModeToolStripMenuItem_Click;
            // 
            // toolsToolStripMenuItem
            // 
            resources.ApplyResources(toolsToolStripMenuItem, "toolsToolStripMenuItem");
            toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { getSQLCreateTableScriptToolStripMenuItem, metadataViewerToolStripMenuItem });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            // 
            // getSQLCreateTableScriptToolStripMenuItem
            // 
            resources.ApplyResources(getSQLCreateTableScriptToolStripMenuItem, "getSQLCreateTableScriptToolStripMenuItem");
            getSQLCreateTableScriptToolStripMenuItem.Name = "getSQLCreateTableScriptToolStripMenuItem";
            getSQLCreateTableScriptToolStripMenuItem.Click += GetSQLCreateTableScriptToolStripMenuItem_Click;
            getSQLCreateTableScriptToolStripMenuItem.MouseEnter += GetSQLCreateTableScriptToolStripMenuItem_MouseEnter;
            // 
            // metadataViewerToolStripMenuItem
            // 
            resources.ApplyResources(metadataViewerToolStripMenuItem, "metadataViewerToolStripMenuItem");
            metadataViewerToolStripMenuItem.Name = "metadataViewerToolStripMenuItem";
            metadataViewerToolStripMenuItem.Click += MetadataViewerToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            resources.ApplyResources(helpToolStripMenuItem, "helpToolStripMenuItem");
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { userGuideToolStripMenuItem, shareAnonymousUsageDataToolStripMenuItem, aboutToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            // 
            // userGuideToolStripMenuItem
            // 
            resources.ApplyResources(userGuideToolStripMenuItem, "userGuideToolStripMenuItem");
            userGuideToolStripMenuItem.Image = Resources.Icons.external_link_icon;
            userGuideToolStripMenuItem.Name = "userGuideToolStripMenuItem";
            userGuideToolStripMenuItem.Click += userGuideToolStripMenuItem_Click;
            // 
            // shareAnonymousUsageDataToolStripMenuItem
            // 
            resources.ApplyResources(shareAnonymousUsageDataToolStripMenuItem, "shareAnonymousUsageDataToolStripMenuItem");
            shareAnonymousUsageDataToolStripMenuItem.Name = "shareAnonymousUsageDataToolStripMenuItem";
            shareAnonymousUsageDataToolStripMenuItem.CheckedChanged += shareAnonymousUsageDataToolStripMenuItem_CheckedChanged;
            shareAnonymousUsageDataToolStripMenuItem.Click += shareAnonymousUsageDataToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            resources.ApplyResources(aboutToolStripMenuItem, "aboutToolStripMenuItem");
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // showingRecordCountStatusBarLabel
            // 
            resources.ApplyResources(showingRecordCountStatusBarLabel, "showingRecordCountStatusBarLabel");
            showingRecordCountStatusBarLabel.Name = "showingRecordCountStatusBarLabel";
            // 
            // actualShownRecordCountLabel
            // 
            resources.ApplyResources(actualShownRecordCountLabel, "actualShownRecordCountLabel");
            actualShownRecordCountLabel.Name = "actualShownRecordCountLabel";
            // 
            // recordsTextStatusBarLabel
            // 
            resources.ApplyResources(recordsTextStatusBarLabel, "recordsTextStatusBarLabel");
            recordsTextStatusBarLabel.Name = "recordsTextStatusBarLabel";
            // 
            // springStatusBarLabel
            // 
            resources.ApplyResources(springStatusBarLabel, "springStatusBarLabel");
            springStatusBarLabel.Name = "springStatusBarLabel";
            springStatusBarLabel.Spring = true;
            // 
            // showingStatusBarLabel
            // 
            resources.ApplyResources(showingStatusBarLabel, "showingStatusBarLabel");
            showingStatusBarLabel.Name = "showingStatusBarLabel";
            showingStatusBarLabel.Click += showingStatusBarLabel_Click;
            // 
            // recordCountStatusBarLabel
            // 
            resources.ApplyResources(recordCountStatusBarLabel, "recordCountStatusBarLabel");
            recordCountStatusBarLabel.Name = "recordCountStatusBarLabel";
            // 
            // outOfStatusBarLabel
            // 
            resources.ApplyResources(outOfStatusBarLabel, "outOfStatusBarLabel");
            outOfStatusBarLabel.Name = "outOfStatusBarLabel";
            // 
            // totalRowCountStatusBarLabel
            // 
            resources.ApplyResources(totalRowCountStatusBarLabel, "totalRowCountStatusBarLabel");
            totalRowCountStatusBarLabel.Name = "totalRowCountStatusBarLabel";
            // 
            // mainStatusStrip
            // 
            resources.ApplyResources(mainStatusStrip, "mainStatusStrip");
            mainStatusStrip.Items.AddRange(new ToolStripItem[] { showingRecordCountStatusBarLabel, actualShownRecordCountLabel, recordsTextStatusBarLabel, springStatusBarLabel, showingStatusBarLabel, recordCountStatusBarLabel, outOfStatusBarLabel, totalRowCountStatusBarLabel });
            mainStatusStrip.Name = "mainStatusStrip";
            mainStatusStrip.ShowItemToolTips = true;
            loadAllRowsButtonTooltip.SetToolTip(mainStatusStrip, resources.GetString("mainStatusStrip.ToolTip"));
            // 
            // exportFileDialog
            // 
            exportFileDialog.DefaultExt = "csv";
            resources.ApplyResources(exportFileDialog, "exportFileDialog");
            exportFileDialog.RestoreDirectory = true;
            // 
            // openFolderDialog
            // 
            resources.ApplyResources(openFolderDialog, "openFolderDialog");
            openFolderDialog.ShowNewFolderButton = false;
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            AllowDrop = true;
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(mainStatusStrip);
            Controls.Add(mainTableLayoutPanel);
            Controls.Add(mainMenuStrip);
            Icon = Resources.Icons.parquet_icon_32x32;
            KeyPreview = true;
            MainMenuStrip = mainMenuStrip;
            Name = "MainForm";
            loadAllRowsButtonTooltip.SetToolTip(this, resources.GetString("$this.ToolTip"));
            Load += MainForm_Load;
            DragDrop += MainForm_DragDrop;
            DragEnter += MainForm_DragEnter;
            KeyDown += MainForm_KeyDown;
            Resize += MainForm_Resize;
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
        private ThemableToolStripSeperator toolStripSeparator;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private ThemableToolStripSeperator toolStripSeparator1;
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
        private System.Windows.Forms.ToolStripMenuItem alwaysLoadAllRecordsToolStripMenuItem;
        private System.Windows.Forms.FolderBrowserDialog openFolderDialog;
        private System.Windows.Forms.ToolStripMenuItem openFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem shareAnonymousUsageDataToolStripMenuItem;
        private System.Windows.Forms.Button loadAllRowsButton;
        private ToolTip loadAllRowsButtonTooltip;
        private ToolStripMenuItem customDateFormatToolStripMenuItem;
        private ToolStripMenuItem darkModeToolStripMenuItem;
    }
}

