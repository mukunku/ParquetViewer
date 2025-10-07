namespace ParquetViewer
{
    partial class MetadataViewer
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
            tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            tabControl = new System.Windows.Forms.TabControl();
            loadingTab = new System.Windows.Forms.TabPage();
            closeButton = new System.Windows.Forms.Button();
            copyRawThriftMetadataButton = new System.Windows.Forms.Button();
            mainBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            copyRawMetadataToolTip = new System.Windows.Forms.ToolTip(components);
            tableLayoutPanel.SuspendLayout();
            tabControl.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 66.6666641F));
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.3333321F));
            tableLayoutPanel.Controls.Add(tabControl, 0, 0);
            tableLayoutPanel.Controls.Add(closeButton, 1, 1);
            tableLayoutPanel.Controls.Add(copyRawThriftMetadataButton, 0, 1);
            tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 2;
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            tableLayoutPanel.Size = new System.Drawing.Size(584, 583);
            tableLayoutPanel.TabIndex = 1;
            // 
            // tabControl
            // 
            tabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(tabControl, 2);
            tabControl.Controls.Add(loadingTab);
            tabControl.Location = new System.Drawing.Point(4, 3);
            tabControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new System.Drawing.Size(576, 542);
            tabControl.TabIndex = 0;
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            // 
            // loadingTab
            // 
            loadingTab.BackColor = System.Drawing.Color.LightGray;
            loadingTab.Location = new System.Drawing.Point(4, 24);
            loadingTab.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            loadingTab.Name = "loadingTab";
            loadingTab.Size = new System.Drawing.Size(568, 514);
            loadingTab.TabIndex = 0;
            loadingTab.Text = "Loading...";
            // 
            // closeButton
            // 
            closeButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            closeButton.Location = new System.Drawing.Point(452, 551);
            closeButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            closeButton.Name = "closeButton";
            closeButton.Size = new System.Drawing.Size(128, 29);
            closeButton.TabIndex = 1;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += CloseButton_Click;
            // 
            // copyRawThriftMetadataButton
            // 
            copyRawThriftMetadataButton.Location = new System.Drawing.Point(3, 551);
            copyRawThriftMetadataButton.Name = "copyRawThriftMetadataButton";
            copyRawThriftMetadataButton.Size = new System.Drawing.Size(148, 29);
            copyRawThriftMetadataButton.TabIndex = 2;
            copyRawThriftMetadataButton.Text = "Copy Raw Metadata";
            copyRawThriftMetadataButton.UseVisualStyleBackColor = true;
            copyRawThriftMetadataButton.Click += copyRawThriftMetadataButton_Click;
            // 
            // mainBackgroundWorker
            // 
            mainBackgroundWorker.DoWork += MainBackgroundWorker_DoWork;
            mainBackgroundWorker.RunWorkerCompleted += MainBackgroundWorker_RunWorkerCompleted;
            // 
            // MetadataViewer
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(584, 583);
            Controls.Add(tableLayoutPanel);
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "MetadataViewer";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Parquet Metadata Viewer";
            Load += MetadataViewer_Load;
            KeyUp += MetadataViewer_KeyUp;
            tableLayoutPanel.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage loadingTab;
        private System.ComponentModel.BackgroundWorker mainBackgroundWorker;
        private System.Windows.Forms.Button copyRawThriftMetadataButton;
        private System.Windows.Forms.ToolTip copyRawMetadataToolTip;
    }
}