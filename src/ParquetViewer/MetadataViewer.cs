using ParquetViewer.Controls;
using ParquetViewer.Engine;
using ParquetViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace ParquetViewer
{
    public partial class MetadataViewer : FormBase
    {
        private static readonly string THRIFT_METADATA = "Thrift Metadata";
        private static readonly string APACHE_ARROW_SCHEMA = "ARROW:schema";
        private static readonly string PANDAS_SCHEMA = "pandas";
        private IParquetEngine? _parquetEngine;

        public MetadataViewer(IParquetEngine parquetEngine) : this()
        {
            this._parquetEngine = parquetEngine;
        }

        public MetadataViewer()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        private void MetadataViewer_Load(object sender, EventArgs e)
        {
            this.mainBackgroundWorker.RunWorkerAsync();
        }

        private void AddTab(string tabName, string text)
        {
            TabPage tab = new TabPage(tabName);
            tab.Controls.Add(new TextBox()
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                BackColor = Color.LightGray,
                Text = text,
                ScrollBars = ScrollBars.Both,
                ReadOnly = true,
                WordWrap = false //gives significant performance boost
            });

            this.tabControl.TabPages.Add(tab);
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MainBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var metadataResult = new List<(string TabName, string Text)>();
            if (_parquetEngine!.Metadata != null)
            {
                string json = ParquetMetadataAnalyzers.ThriftMetadataToJSON(_parquetEngine, _parquetEngine.RecordCount, _parquetEngine.Fields.Count);
                metadataResult.Add((THRIFT_METADATA, json));
            }
            else
                metadataResult.Add((THRIFT_METADATA, Resources.Errors.NoThriftMetadataAvailableErrorMessage));

            if (_parquetEngine.CustomMetadata != null)
            {
                foreach (var _customMetadata in _parquetEngine.CustomMetadata)
                {
                    string value = _customMetadata.Value;
                    if (PANDAS_SCHEMA.Equals(_customMetadata.Key))
                    {
                        //Pandas is already json; so just make it pretty.
                        value = ParquetMetadataAnalyzers.TryFormatJSON(value);
                    }
                    else if (APACHE_ARROW_SCHEMA.Equals(_customMetadata.Key))
                    {
                        value = ParquetMetadataAnalyzers.ApacheArrowToJSON(value);
                    }
                    else
                    {
                        value = ParquetMetadataAnalyzers.TryFormatJSON(value);
                    }

                    metadataResult.Add((_customMetadata.Key, value));
                }
            }

            e.Result = metadataResult;
        }

        private void MainBackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(this,
                    Resources.Errors.MetadataReadErrorMessage + Environment.NewLine + e.Error,
                    Resources.Errors.MetadataReadErrorTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                this.tabControl.SuspendLayout();
                this.tabControl.TabPages.Clear();
                if (e.Result is List<(string TabName, string Text)> tabs)
                {
                    foreach (var tab in tabs)
                    {
                        this.AddTab(tab.TabName, tab.Text);
                    }
                }
                this.tabControl.ResumeLayout();
            }
        }

        private void MetadataViewer_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        public override void SetTheme(Theme theme)
        {
            if (DesignMode)
            {
                return;
            }

            base.SetTheme(theme);
            this.closeButton.ForeColor = Color.Black;
        }
    }
}