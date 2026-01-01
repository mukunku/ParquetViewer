using ParquetViewer.Engine.Types;
using System.Collections;
using System.Text;

namespace ParquetViewer.Engine.DuckDB.Types
{
    internal class ListValue : IListValue
    {
        public IList Data { get; }

        public Type Type { get; }

        public ListValue(IList data, Type itemType)
        {
            this.Data = data;
            this.Type = itemType;
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

                    if (data is DateTime dt && ParquetEngineSettings.DateDisplayFormat is not null)
                        sb.Append(dt.ToString(ParquetEngineSettings.DateDisplayFormat));
                    else if (data is DateOnly dateOnly && ParquetEngineSettings.DateOnlyDisplayFormat is not null)
                        sb.Append(dateOnly.ToString(ParquetEngineSettings.DateOnlyDisplayFormat));
                    else
                        sb.Append(data?.ToString() ?? string.Empty);

                    isFirst = false;
                }
            }

            sb.Append(']');
            return sb.ToString();
        }

        public int CompareTo(IListValue? other)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(object? obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<object> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
