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
            mainSplitContainer = new SplitContainer();
            closeButton = new Button();
            executeQueryKeyboardShortcutToolTip = new ToolTip(components);
            mainTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)queryRichTextBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)resultsGridView).BeginInit();
            statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).BeginInit();
            mainSplitContainer.Panel1.SuspendLayout();
            mainSplitContainer.Panel2.SuspendLayout();
            mainSplitContainer.SuspendLayout();
            SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            mainTableLayoutPanel.ColumnCount = 2;
            mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            mainTableLayoutPanel.Controls.Add(queryRichTextBox, 0, 0);
            mainTableLayoutPanel.Controls.Add(executeQueryButton, 0, 1);
            mainTableLayoutPanel.Controls.Add(querySyntaxDocsButton, 1, 1);
            mainTableLayoutPanel.Dock = DockStyle.Fill;
            mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            mainTableLayoutPanel.RowCount = 2;
            mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            mainTableLayoutPanel.Size = new System.Drawing.Size(1016, 176);
            mainTableLayoutPanel.TabIndex = 0;
            // 
            // queryRichTextBox
            // 
            queryRichTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
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
            queryRichTextBox.AutoScrollMinSize = new System.Drawing.Size(29, 16);
            queryRichTextBox.BackBrush = null;
            queryRichTextBox.CharHeight = 16;
            queryRichTextBox.CharWidth = 9;
            mainTableLayoutPanel.SetColumnSpan(queryRichTextBox, 2);
            queryRichTextBox.CommentPrefix = "--";
            queryRichTextBox.DisabledColor = System.Drawing.Color.FromArgb(100, 180, 180, 180);
            queryRichTextBox.Font = new System.Drawing.Font("Courier New", 11.25F);
            queryRichTextBox.Hotkeys = resources.GetString("queryRichTextBox.Hotkeys");
            queryRichTextBox.IsReplaceMode = false;
            queryRichTextBox.Language = Language.SQL;
            queryRichTextBox.LeftBracket = '(';
            queryRichTextBox.Location = new System.Drawing.Point(3, 3);
            queryRichTextBox.Name = "queryRichTextBox";
            queryRichTextBox.Paddings = new Padding(0);
            queryRichTextBox.RightBracket = ')';
            queryRichTextBox.SelectionColor = System.Drawing.Color.FromArgb(60, 0, 0, 255);
            queryRichTextBox.ServiceColors = (ServiceColors)resources.GetObject("queryRichTextBox.ServiceColors");
            queryRichTextBox.Size = new System.Drawing.Size(1010, 138);
            queryRichTextBox.TabIndex = 0;
            queryRichTextBox.Zoom = 100;
            queryRichTextBox.TextChanged += queryRichTextBox_TextChanged;
            queryRichTextBox.KeyDown += queryRichTextBox_KeyDown;
            // 
            // executeQueryButton
            // 
            executeQueryButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            executeQueryButton.Image = Resources.Icons.exclamation_icon;
            executeQueryButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            executeQueryButton.Location = new System.Drawing.Point(3, 147);
            executeQueryButton.Name = "executeQueryButton";
            executeQueryButton.Size = new System.Drawing.Size(890, 26);
            executeQueryButton.TabIndex = 1;
            executeQueryButton.Text = "Execute";
            executeQueryButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            executeQueryKeyboardShortcutToolTip.SetToolTip(executeQueryButton, "(Shift + Enter)");
            executeQueryButton.UseVisualStyleBackColor = true;
            executeQueryButton.Click += executeQueryButton_Click;
            // 
            // querySyntaxDocsButton
            // 
            querySyntaxDocsButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            querySyntaxDocsButton.Image = Resources.Icons.external_link_icon;
            querySyntaxDocsButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            querySyntaxDocsButton.Location = new System.Drawing.Point(899, 147);
            querySyntaxDocsButton.Name = "querySyntaxDocsButton";
            querySyntaxDocsButton.Size = new System.Drawing.Size(114, 26);
            querySyntaxDocsButton.TabIndex = 2;
            querySyntaxDocsButton.Text = "Query Syntax";
            querySyntaxDocsButton.TextImageRelation = TextImageRelation.ImageBeforeText;
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
            resultsGridView.Dock = DockStyle.Fill;
            resultsGridView.EnableHeadersVisualStyles = false;
            resultsGridView.Location = new System.Drawing.Point(0, 0);
            resultsGridView.Name = "resultsGridView";
            resultsGridView.ReadOnly = true;
            resultsGridView.RowHeadersWidth = 24;
            resultsGridView.ShowCellToolTips = false;
            resultsGridView.ShowCopyAsWhereContextMenuItem = false;
            resultsGridView.Size = new System.Drawing.Size(1016, 242);
            resultsGridView.TabIndex = 2;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1, showingCountLabel, toolStripStatusLabel2, toolStripStatusLabel4, zoomPercentageDropDown, queryExecutionStatusLabel, timeElapsedLabel });
            statusStrip.Location = new System.Drawing.Point(0, 428);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new System.Drawing.Size(1018, 22);
            statusStrip.TabIndex = 1;
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new System.Drawing.Size(56, 17);
            toolStripStatusLabel1.Text = "Showing:";
            // 
            // showingCountLabel
            // 
            showingCountLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            showingCountLabel.Name = "showingCountLabel";
            showingCountLabel.Size = new System.Drawing.Size(14, 17);
            showingCountLabel.Text = "0";
            // 
            // toolStripStatusLabel2
            // 
            toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            toolStripStatusLabel2.Size = new System.Drawing.Size(44, 17);
            toolStripStatusLabel2.Text = "Results";
            // 
            // toolStripStatusLabel4
            // 
            toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            toolStripStatusLabel4.Size = new System.Drawing.Size(675, 17);
            toolStripStatusLabel4.Spring = true;
            // 
            // zoomPercentageDropDown
            // 
            zoomPercentageDropDown.DisplayStyle = ToolStripItemDisplayStyle.Text;
            zoomPercentageDropDown.DropDownItems.AddRange(new ToolStripItem[] { percentage100, percentage110, percentage125, percentage140, percentage150 });
            zoomPercentageDropDown.Image = (System.Drawing.Image)resources.GetObject("zoomPercentageDropDown.Image");
            zoomPercentageDropDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            zoomPercentageDropDown.Name = "zoomPercentageDropDown";
            zoomPercentageDropDown.Size = new System.Drawing.Size(121, 20);
            zoomPercentageDropDown.Text = "Query Zoom: 100%";
            // 
            // percentage100
            // 
            percentage100.Checked = true;
            percentage100.CheckOnClick = true;
            percentage100.CheckState = CheckState.Checked;
            percentage100.Name = "percentage100";
            percentage100.Size = new System.Drawing.Size(102, 22);
            percentage100.Tag = "100";
            percentage100.Text = "100%";
            percentage100.Click += zoomPercentage_Click;
            // 
            // percentage110
            // 
            percentage110.Name = "percentage110";
            percentage110.Size = new System.Drawing.Size(102, 22);
            percentage110.Tag = "110";
            percentage110.Text = "110%";
            percentage110.Click += zoomPercentage_Click;
            // 
            // percentage125
            // 
            percentage125.CheckOnClick = true;
            percentage125.Name = "percentage125";
            percentage125.Size = new System.Drawing.Size(102, 22);
            percentage125.Tag = "125";
            percentage125.Text = "125%";
            percentage125.Click += zoomPercentage_Click;
            // 
            // percentage140
            // 
            percentage140.Name = "percentage140";
            percentage140.Size = new System.Drawing.Size(102, 22);
            percentage140.Tag = "140";
            percentage140.Text = "140%";
            percentage140.Click += zoomPercentage_Click;
            // 
            // percentage150
            // 
            percentage150.CheckOnClick = true;
            percentage150.Name = "percentage150";
            percentage150.Size = new System.Drawing.Size(102, 22);
            percentage150.Tag = "150";
            percentage150.Text = "150%";
            percentage150.Click += zoomPercentage_Click;
            // 
            // queryExecutionStatusLabel
            // 
            queryExecutionStatusLabel.Name = "queryExecutionStatusLabel";
            queryExecutionStatusLabel.Size = new System.Drawing.Size(55, 17);
            queryExecutionStatusLabel.Text = "Running:";
            // 
            // timeElapsedLabel
            // 
            timeElapsedLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            timeElapsedLabel.Name = "timeElapsedLabel";
            timeElapsedLabel.Size = new System.Drawing.Size(38, 17);
            timeElapsedLabel.Text = "00:00";
            // 
            // mainSplitContainer
            // 
            mainSplitContainer.BackColor = System.Drawing.SystemColors.Control;
            mainSplitContainer.BorderStyle = BorderStyle.FixedSingle;
            mainSplitContainer.Dock = DockStyle.Fill;
            mainSplitContainer.Location = new System.Drawing.Point(0, 0);
            mainSplitContainer.Name = "mainSplitContainer";
            mainSplitContainer.Orientation = Orientation.Horizontal;
            // 
            // mainSplitContainer.Panel1
            // 
            mainSplitContainer.Panel1.Controls.Add(mainTableLayoutPanel);
            mainSplitContainer.Panel1MinSize = 120;
            // 
            // mainSplitContainer.Panel2
            // 
            mainSplitContainer.Panel2.BackColor = System.Drawing.SystemColors.Control;
            mainSplitContainer.Panel2.Controls.Add(resultsGridView);
            mainSplitContainer.Panel2MinSize = 150;
            mainSplitContainer.Size = new System.Drawing.Size(1018, 428);
            mainSplitContainer.SplitterDistance = 178;
            mainSplitContainer.SplitterWidth = 6;
            mainSplitContainer.TabIndex = 2;
            // 
            // closeButton
            // 
            closeButton.Location = new System.Drawing.Point(942, 221);
            closeButton.Name = "closeButton";
            closeButton.Size = new System.Drawing.Size(0, 0);
            closeButton.TabIndex = 3;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            // 
            // QueryEditor
            // 
            AcceptButton = executeQueryButton;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = closeButton;
            ClientSize = new System.Drawing.Size(1018, 450);
            Controls.Add(closeButton);
            Controls.Add(mainSplitContainer);
            Controls.Add(statusStrip);
            Icon = Resources.Icons.sql_server_icon;
            Name = "QueryEditor";
            Text = "ParquetViewer - Query Editor (Powered by DuckDB)";
            Load += QueryEditor_Load;
            KeyUp += QueryEditor_KeyUp;
            mainTableLayoutPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)queryRichTextBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)resultsGridView).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            mainSplitContainer.Panel1.ResumeLayout(false);
            mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).EndInit();
            mainSplitContainer.ResumeLayout(false);
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