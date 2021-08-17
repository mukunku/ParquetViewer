using Parquet.Thrift;
using ParquetFileViewer.Helpers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParquetFileViewer
{
    public partial class MetadataViewer : Form
    {
        private static readonly string THRIFT_METADATA = "Thrift Metadata";
        private static readonly string APACHE_ARROW_SCHEMA = "ARROW:schema";
        private static readonly string PANDAS_SCHEMA = "pandas";
        private Parquet.ParquetReader parquetReader;

        public MetadataViewer(Parquet.ParquetReader parquetReader) : this()
        {
            this.parquetReader = parquetReader;
        }

        public MetadataViewer()
        {
            InitializeComponent();
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
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true
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
            if (parquetReader.ThriftMetadata != null)
            {
                var thriftMetadata = parquetReader.ThriftMetadata;
                var jsonObject = new Newtonsoft.Json.Linq.JObject();
                jsonObject[nameof(thriftMetadata.Version)] = thriftMetadata.Version;
                jsonObject[nameof(thriftMetadata.Num_rows)] = thriftMetadata.Num_rows;
                jsonObject[nameof(thriftMetadata.Created_by)] = thriftMetadata.Created_by;

                var schemas = new Newtonsoft.Json.Linq.JArray();
                foreach (var schema in thriftMetadata.Schema)
                {
                    if ("schema".Equals(schema.Name) && schemas.Count == 0)
                        continue;

                    var schemaObject = new Newtonsoft.Json.Linq.JObject();
                    schemaObject[nameof(schema.Field_id)] = schema.Field_id;
                    schemaObject[nameof(schema.Name)] = schema.Name;
                    schemaObject[nameof(schema.Type)] = schema.Type.ToString();
                    schemaObject[nameof(schema.Type_length)] = schema.Type_length;
                    schemaObject[nameof(schema.LogicalType)] = schema.LogicalType?.ToString();
                    schemaObject[nameof(schema.Scale)] = schema.Scale;
                    schemaObject[nameof(schema.Precision)] = schema.Precision;
                    schemaObject[nameof(schema.Repetition_type)] = schema.Repetition_type.ToString();
                    schemaObject[nameof(schema.Converted_type)] = schema.Converted_type.ToString();

                    schemas.Add(schemaObject);
                }
                jsonObject[nameof(thriftMetadata.Schema)] = schemas;

                metadataResult.Add((THRIFT_METADATA, jsonObject.ToString().FormatJSON()));
            }
            else
                metadataResult.Add((THRIFT_METADATA, "No thrift metadata available"));

            if (this.parquetReader.CustomMetadata != null)
            {
                foreach (var _customMetadata in this.parquetReader.CustomMetadata)
                {
                    string value = _customMetadata.Value;
                    if (PANDAS_SCHEMA.Equals(_customMetadata.Key))
                    {
                        value = value.FormatJSON();
                    }
                    else if (APACHE_ARROW_SCHEMA.Equals(_customMetadata.Key))
                    {
                        //TODO: Base64 decode on its own doesn't accomplish anything.
                        //Need some way to read the schema but there isn't anything in the apache arrow repo for this...
                        //https://github.com/apache/arrow/blob/master/csharp/src/Apache.Arrow/Ipc/MessageSerializer.cs
                        //value = value.Base64Decode();
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
                MessageBox.Show($"Something went wrong while reading the file's metadata: " +
                    $"{Environment.NewLine}{e.Error.ToString()}", "Metadata Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                this.tabControl.TabPages.Clear();
                if (e.Result is List<(string TabName, string Text)> tabs)
                {
                    foreach (var tab in tabs)
                    {
                        this.AddTab(tab.TabName, tab.Text);
                    }
                }
            }
        }
    }
}
