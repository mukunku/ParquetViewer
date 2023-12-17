using System.Collections;
using System.Text;

namespace ParquetViewer.Engine.Types
{
    public class ListValue
    {
        public IList? Data { get; }
        public Type? Type { get; private set; }
        public static string? DateDisplayFormat { get; set; }

        public ListValue(Array data)
        {
            Data = data;
            Type = Data?.GetType().GetElementType();
        }

        public ListValue(ArrayList data, Type type)
        {
            Data = data;
            Type = type; //the parameter is needed for the case where the entire list is null

            foreach (var d in data)
            {
                if (d is not null && d != DBNull.Value)
                {
                    if (Type != d.GetType())
                    {
                        throw new ArgumentException($"Data type {d.GetType()} doesn't match the passed type {type}");
                    }
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder("[");

            if (Data is not null)
            {
                bool isFirst = true;
                foreach (var data in Data)
                {
                    if (!isFirst)
                        sb.Append(',');

                    if (data is DateTime dt && DateDisplayFormat is not null)
                        sb.Append(dt.ToString(DateDisplayFormat));
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
