using System.Data;
using System.Text.Json;

namespace ParquetViewer.Engine
{
    public class StructValue
    {
        public string Name { get; }
        public DataRow Data { get; }

        public StructValue(string name, DataRow data)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (data is null)
                throw new ArgumentNullException(nameof(data));

            Name = name;
            Data = data;
        }

        public override string ToString() => DataTableToJSONWithJavaScriptSerializer(this.Data);

        private string DataTableToJSONWithJavaScriptSerializer(DataRow dataRow)
        {
            try
            {
                var record = new Dictionary<string, object>();
                foreach (DataColumn col in dataRow.Table.Columns)
                {
                    string columnName = col.ColumnName.Replace($"{this.Name}/", string.Empty); //Remove the parent field name from columns when rendering the data as json in the gridview cell.
                    record.Add(columnName, dataRow[col]);
                }

                return JsonSerializer.Serialize(record,
                     new JsonSerializerOptions()
                     {
                         Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping, //don't escape anything to make it human readable
                         WriteIndented = false //keep it minimized. I wonder if users would prefer them to be indented? Bit I feel that wouldn't be desirable for csv exports and such.
                     });
            }
            catch (Exception ex)
            {
                return $"Error while deserializing field: {Environment.NewLine}{Environment.NewLine}{ex}";
            }
        }

    }
}
