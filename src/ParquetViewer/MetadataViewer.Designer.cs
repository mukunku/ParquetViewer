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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MetadataViewer));
            tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            tabControl = new System.Windows.Forms.TabControl();
            loadingTab = new System.Windows.Forms.TabPage();
            closeButton = new System.Windows.Forms.Button();
            mainBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            copyRawMetadataButtonToolTip = new System.Windows.Forms.ToolTip(components);
            tableLayoutPanel.SuspendLayout();
            tabControl.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            resources.ApplyResources(tableLayoutPanel, "tableLayoutPanel");
            tableLayoutPanel.Controls.Add(tabControl, 0, 0);
            tableLayoutPanel.Controls.Add(closeButton, 1, 1);
            tableLayoutPanel.Name = "tableLayoutPanel";
            // 
            // tabControl
            // 
            resources.ApplyResources(tabControl, "tabControl");
            tableLayoutPanel.SetColumnSpan(tabControl, 2);
            tabControl.Controls.Add(loadingTab);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            // 
            // loadingTab
            // 
            resources.ApplyResources(loadingTab, "loadingTab");
            loadingTab.BackColor = System.Drawing.Color.LightGray;
            resources.ApplyResources(loadingTab, "loadingTab");
            loadingTab.Name = "loadingTab";
            // 
            // closeButton
            // 
            resources.ApplyResources(closeButton, "closeButton");
            closeButton.Name = "closeButton";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += CloseButton_Click;
            // 
            // mainBackgroundWorker
            // 
            mainBackgroundWorker.DoWork += MainBackgroundWorker_DoWork;
            mainBackgroundWorker.RunWorkerCompleted += MainBackgroundWorker_RunWorkerCompleted;
            // 
            // MetadataViewer
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel);
            KeyPreview = true;
            Name = "MetadataViewer";
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
        private System.Windows.Forms.ToolTip copyRawMetadataButtonToolTip;
    }
}