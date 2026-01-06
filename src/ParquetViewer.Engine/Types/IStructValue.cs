using System.Data;

namespace ParquetViewer.Engine.Types
{
    public interface IStructValue : IComparable<IStructValue>, IComparable
    {
        public string Name { get; }

        public DataRowLite Data { get; }

        IReadOnlyCollection<string> FieldNames { get; }

        string ToStringTruncated(int desiredLength);

        DataTable ToDataTable();

        string ToJSON(out bool success, int? desiredLength = null);
    }
}