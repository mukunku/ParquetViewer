using ParquetViewer.Common;
using System.Collections;
using System.Text;

namespace ParquetViewer.Engine
{
    public class ListValue : IComplexValue
    {
        public IList? Data { get; }
        public Type? Type { get; private set; }

        public ListValue(Array data)
        {
            this.Data = data;
            this.Type = this.Data?.GetType().GetElementType();
        }

        public ListValue(ArrayList data)
        {
            this.Data = data;
            this.Type = null;
            foreach (var d in data)
            {
                if (d is not null && d != DBNull.Value)
                {
                    this.Type = d.GetType();
                    break;
                }
            }
        }

        //TODO: Adjust auto column sizing to ignore list cells
        public override string ToString()
        {
            var sb = new StringBuilder("[");

            if (this.Data is not null)
            {
                bool isFirst = true;
                foreach (var data in this.Data)
                {
                    if (!isFirst)
                        sb.Append(',');

                    if (data is DateTime dt)
                        sb.Append(dt.ToString(AppSettings.DateTimeDisplayFormat.GetDateFormat()));
                    else
                        sb.Append(data?.ToString() ?? string.Empty);

                    isFirst = false;
                }

                if (isFirst)
                    sb.Append(' ');
            }

            sb.Append(']');
            return sb.ToString();
        }
    }
}
