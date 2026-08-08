using System.Collections;

namespace ParquetViewer.Engine.Types
{
    public interface IListValue : IComparable<IListValue>, IComparable, IEnumerable<object>
    {
        public IList Data { get; }
        public Type Type { get; }
    }
}