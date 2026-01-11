using FastColoredTextBoxNS;
using ParquetViewer.Controls;
using System.Windows.Forms;

namespace ParquetViewer
{
    partial class QueryEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QueryEditor));
            mainSplitContainer = new SplitContainer();
            mainTableLayoutPanel = new TableLayoutPanel();
            queryRichTextBox = new FastColoredTextBox();
            executeQueryButton = new Button();
            querySyntaxDocsButton = new Button();
            resultsGridView = new ParquetGridView();
            statusStrip = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            showingCountLabel = new ToolStripStatusLabel();
            toolStripStatusLabel2 = new ToolStripStatusLabel();
            toolStripStatusLabel4 = new ToolStripStatusLabel();
            zoomPercentageDropDown = new ToolStripDropDownButton();
            percentage100 = new ToolStripMenuItem();
            percentage110 = new ToolStripMenuItem();
            percentage125 = new ToolStripMenuItem();
            percentage140 = new ToolStripMenuItem();
            percentage150 = new ToolStripMenuItem();
            queryExecutionStatusLabel = new ToolStripStatusLabel();
            timeElapsedLabel = new ToolStripStatusLabel();
            closeButton = new Button();
            executeQueryKeyboardShortcutToolTip = new ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).BeginInit();
            mainSplitContainer.Panel1.SuspendLayout();
            mainSplitContainer.Panel2.SuspendLayout();
            mainSplitContainer.SuspendLayout();
            mainTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)queryRichTextBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)resultsGridView).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // mainSplitContainer
            // 
            mainSplitContainer.BackColor = System.Drawing.SystemColors.Control;
            mainSplitContainer.BorderStyle = BorderStyle.FixedSingle;
            resources.ApplyResources(mainSplitContainer, "mainSplitContainer");
            mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            mainSplitContainer.Panel1.Controls.Add(mainTableLayoutPanel);
            // 
            // mainSplitContainer.Panel2
            // 
            mainSplitContainer.Panel2.BackColor = System.Drawing.SystemColors.Control;
            mainSplitContainer.Panel2.Controls.Add(resultsGridView);
            // 
            // mainTableLayoutPanel
            // 
            resources.ApplyResources(mainTableLayoutPanel, "mainTableLayoutPanel");
            mainTableLayoutPanel.Controls.Add(queryRichTextBox, 0, 0);
            mainTableLayoutPanel.Controls.Add(executeQueryButton, 0, 1);
            mainTableLayoutPanel.Controls.Add(querySyntaxDocsButton, 1, 1);
            mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            // 
            // queryRichTextBox
            // 
            resources.ApplyResources(queryRichTextBox, "queryRichTextBox");
            queryRichTextBox.AutoCompleteBracketsList = new char[]
    {
    '(',
    ')',
    '{',
    '}',
    '[',
    ']',
    '"',
    '"',
    '\'',
    '\''
    };
            queryRichTextBox.AutoIndentCharsPatterns = "";
            queryRichTextBox.BackBrush = null;
            queryRichTextBox.CharHeight = 16;
            queryRichTextBox.CharWidth = 9;
            mainTableLayoutPanel.SetColumnSpan(queryRichTextBox, 2);
            queryRichTextBox.CommentPrefix = "--";
            queryRichTextBox.DisabledColor = System.Drawing.Color.FromArgb(100, 180, 180, 180);
            queryRichTextBox.Hotkeys = resources.GetString("queryRichTextBox.Hotkeys");
            queryRichTextBox.IsReplaceMode = false;
            queryRichTextBox.Language = Language.SQL;
            queryRichTextBox.LeftBracket = '(';
            queryRichTextBox.Name = "queryRichTextBox";
            queryRichTextBox.Paddings = new Padding(0);
            queryRichTextBox.RightBracket = ')';
            queryRichTextBox.SelectionColor = System.Drawing.Color.FromArgb(60, 0, 0, 255);
            queryRichTextBox.ServiceColors = (ServiceColors)resources.GetObject("queryRichTextBox.ServiceColors");
            queryRichTextBox.Zoom = 100;
            queryRichTextBox.TextChanged += queryRichTextBox_TextChanged;
            queryRichTextBox.KeyDown += queryRichTextBox_KeyDown;
            // 
            // executeQueryButton
            // 
            resources.ApplyResources(executeQueryButton, "executeQueryButton");
            executeQueryButton.Image = Resources.Icons.exclamation_icon;
            executeQueryButton.Name = "executeQueryButton";
            executeQueryKeyboardShortcutToolTip.SetToolTip(executeQueryButton, resources.GetString("executeQueryButton.ToolTip"));
            executeQueryButton.UseVisualStyleBackColor = true;
            executeQueryButton.Click += executeQueryButton_Click;
            // 
            // querySyntaxDocsButton
            // 
            resources.ApplyResources(querySyntaxDocsButton, "querySyntaxDocsButton");
            querySyntaxDocsButton.Image = Resources.Icons.external_link_icon;
            querySyntaxDocsButton.Name = "querySyntaxDocsButton";
            querySyntaxDocsButton.UseVisualStyleBackColor = true;
            querySyntaxDocsButton.Click += querySyntaxDocsButton_Click;
            // 
            // resultsGridView
            // 
            resultsGridView.AllowUserToAddRows = false;
            resultsGridView.AllowUserToDeleteRows = false;
            resultsGridView.AllowUserToOrderColumns = true;
            resultsGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            resultsGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            resultsGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            resultsGridView.ColumnNameEscapeFormat = "[{0}]";
            resultsGridView.CopyAsWhereIcon = null;
            resultsGridView.CopyToClipboardIcon = null;
            resources.ApplyResources(resultsGridView, "resultsGridView");
            resultsGridView.EnableHeadersVisualStyles = false;
            resultsGridView.Name = "resultsGridView";
            resultsGridView.ReadOnly = true;
            resultsGridView.ShowCellToolTips = false;
            resultsGridView.ShowCopyAsWhereContextMenuItem = false;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1, showingCountLabel, toolStripStatusLabel2, toolStripStatusLabel4, zoomPercentageDropDown, queryExecutionStatusLabel, timeElapsedLabel });
            resources.ApplyResources(statusStrip, "statusStrip");
            statusStrip.Name = "statusStrip";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            resources.ApplyResources(toolStripStatusLabel1, "toolStripStatusLabel1");
            // 
            // showingCountLabel
            // 
            resources.ApplyResources(showingCountLabel, "showingCountLabel");
            showingCountLabel.Name = "showingCountLabel";
            // 
            // toolStripStatusLabel2
            // 
            toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            resources.ApplyResources(toolStripStatusLabel2, "toolStripStatusLabel2");
            // 
            // toolStripStatusLabel4
            // 
            toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            resources.ApplyResources(toolStripStatusLabel4, "toolStripStatusLabel4");
            toolStripStatusLabel4.Spring = true;
            // 
            // zoomPercentageDropDown
            // 
            zoomPercentageDropDown.DisplayStyle = ToolStripItemDisplayStyle.Text;
            zoomPercentageDropDown.DropDownItems.AddRange(new ToolStripItem[] { percentage100, percentage110, percentage125, percentage140, percentage150 });
            resources.ApplyResources(zoomPercentageDropDown, "zoomPercentageDropDown");
            zoomPercentageDropDown.Name = "zoomPercentageDropDown";
            // 
            // percentage100
            // 
            percentage100.Checked = true;
            percentage100.CheckOnClick = true;
            percentage100.CheckState = CheckState.Checked;
            percentage100.Name = "percentage100";
            resources.ApplyResources(percentage100, "percentage100");
            percentage100.Tag = "100";
            percentage100.Click += zoomPercentage_Click;
            // 
            // percentage110
            // 
            percentage110.Name = "percentage110";
            resources.ApplyResources(percentage110, "percentage110");
            percentage110.Tag = "110";
            percentage110.Click += zoomPercentage_Click;
            // 
            // percentage125
            // 
            percentage125.CheckOnClick = true;
            percentage125.Name = "percentage125";
            resources.ApplyResources(percentage125, "percentage125");
            percentage125.Tag = "125";
            percentage125.Click += zoomPercentage_Click;
            // 
            // percentage140
            // 
            percentage140.Name = "percentage140";
            resources.ApplyResources(percentage140, "percentage140");
            percentage140.Tag = "140";
            percentage140.Click += zoomPercentage_Click;
            // 
            // percentage150
            // 
            percentage150.CheckOnClick = true;
            percentage150.Name = "percentage150";
            resources.ApplyResources(percentage150, "percentage150");
            percentage150.Tag = "150";
            percentage150.Click += zoomPercentage_Click;
            // 
            // queryExecutionStatusLabel
            // 
            queryExecutionStatusLabel.Name = "queryExecutionStatusLabel";
            resources.ApplyResources(queryExecutionStatusLabel, "queryExecutionStatusLabel");
            // 
            // timeElapsedLabel
            // 
            resources.ApplyResources(timeElapsedLabel, "timeElapsedLabel");
            timeElapsedLabel.Name = "timeElapsedLabel";
            // 
            // closeButton
            // 
            resources.ApplyResources(closeButton, "closeButton");
            closeButton.Name = "closeButton";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            // 
            // QueryEditor
            // 
            AcceptButton = executeQueryButton;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = closeButton;
            Controls.Add(closeButton);
            Controls.Add(mainSplitContainer);
            Controls.Add(statusStrip);
            Icon = Resources.Icons.sql_server_icon;
            Name = "QueryEditor";
            Load += QueryEditor_Load;
            KeyUp += QueryEditor_KeyUp;
            mainSplitContainer.Panel1.ResumeLayout(false);
            mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).EndInit();
            mainSplitContainer.ResumeLayout(false);
            mainTableLayoutPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)queryRichTextBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)resultsGridView).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private FastColoredTextBox queryRichTextBox;
        private System.Windows.Forms.Button executeQueryButton;
        private System.Windows.Forms.StatusStrip statusStrip;
        private Controls.ParquetGridView resultsGridView;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel showingCountLabel;
        private ToolStripStatusLabel toolStripStatusLabel2;
        private ToolStripStatusLabel queryExecutionStatusLabel;
        private ToolStripStatusLabel timeElapsedLabel;
        private SplitContainer mainSplitContainer;
        private ToolStripDropDownButton zoomPercentageDropDown;
        private ToolStripMenuItem percentage100;
        private ToolStripMenuItem percentage125;
        private ToolStripMenuItem percentage150;
        private ToolStripStatusLabel toolStripStatusLabel4;
        private ToolStripMenuItem percentage110;
        private ToolStripMenuItem percentage140;
        private Button querySyntaxDocsButton;
        private Button closeButton;
        private ToolTip executeQueryKeyboardShortcutToolTip;
    }
}