using ParquetViewer.Engine.Types;
using System.Collections;
using System.Text;

namespace ParquetViewer.Engine.ParquetNET.Types
{
    public class ListValue : IListValue, IComparable<ListValue>, IComparable, IEnumerable<object>
    {
        public IList Data { get; }
        public Type Type { get; private set; }

        public ListValue(Array data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Type = Data.GetType().GetElementType() ?? throw new ArgumentException("Invalid array type");
        }

        public ListValue(ArrayList data, Type type)
        {
            Data = data;
            Type = type; //the parameter is needed for the case where the entire list is null

            foreach (var d in data)
            {
                if (d != DBNull.Value && d is not null
                    && Type != d.GetType())
                {
                    throw new ArrayTypeMismatchException($"Data type {d.GetType()} doesn't match the passed type {type}");
                }
            }
        }

        public int Length => Data.Count;

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

        public int CompareTo(ListValue? other)
        {
            if (other is null)
                return 1;
            else if (this is null)
                return -1;

            for (var i = 0; i < Data.Count; i++)
            {
                if (other.Data.Count == i)
                {
                    //This list has more values, so lets say it's 'less than' in sort order
                    return -1;
                }

                var value = Data[i];
                var otherValue = other.Data[i];
                int comparison = Helpers.CompareTo(value, otherValue);
                if (comparison != 0)
                    return comparison;
            }

            if (Data.Count < other.Data.Count)
                return 1; //this list has less values so say it's 'more than' in sort order

            return 0; //the lists appear equal
        }

        public int CompareTo(object? obj)
        {
            if (obj is ListValue list)
                return CompareTo(list);
            else
                return 1;
        }

        public IEnumerator<object> GetEnumerator()
        {
            foreach (var item in Data)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int CompareTo(IListValue? other) => this.CompareTo((object?)other);
    }
}
