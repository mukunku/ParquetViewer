using System.Data;

namespace ParquetViewer.Engine.Types;

public interface IStructValue : IComparable<IStructValue>, IComparable
{
    /// <summary>
    /// The struct's field names, in order.
    /// </summary>
    IReadOnlyList<string> FieldNames { get; }

    /// <summary>
    /// The struct's field values, in the same order as <see cref="FieldNames"/>. Nulls are <see cref="DBNull"/>.
    /// </summary>
    IReadOnlyList<object> Values { get; }

    /// <summary>
    /// The value of the field named <paramref name="fieldName"/>.
    /// </summary>
    object GetValue(string fieldName);

    string ToStringTruncated(int desiredLength);

    DataTable ToDataTable();

    string ToJSON(out bool success, int? desiredLength = null);
}