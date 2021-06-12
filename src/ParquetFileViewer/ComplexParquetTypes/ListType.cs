using System;
using System.Text;

namespace ParquetFileViewer.ComplexParquetTypes
{
    public class ListType
    {
        public Array Data { get; }
        public Type Type
        {
            get
            {
                return this.Data?.GetType().GetElementType();
            }
        }

        public ListType(Array data)
        {
            this.Data = data;
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
