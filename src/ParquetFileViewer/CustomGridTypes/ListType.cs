using System;
using System.Collections;
using System.Text;

namespace ParquetFileViewer.CustomGridTypes
{
    public class ListType
    {
        public IList Data { get; }
        public Type Type { get; private set; }

        public ListType(Array data)
        {
            this.Data = data;
            this.Type = this.Data?.GetType().GetElementType();
        }

        public ListType(ArrayList data)
        {
            this.Data = data;
            this.Type = typeof(string); //default to string (will this ever happen?)
            foreach(var d in data)
            {
                if (d != null && d != DBNull.Value)
                {
                    this.Type = d.GetType();
                    break;
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("[");

            bool isFirst = true;
            foreach (var data in this.Data)
            {
                if (!isFirst)
                    sb.Append(",");

                if (data is DateTime dt && AppSettings.UseISODateFormat)
                    sb.Append(dt.ToString(Constants.ISO8601_DATETIME_FORMAT));
                else
                    sb.Append(data?.ToString() ?? string.Empty);

                isFirst = false;
            }

            if (isFirst)
                sb.Append(" ");

            sb.Append("]");
            return sb.ToString();
        }
    }
}
