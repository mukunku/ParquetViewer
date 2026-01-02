using System.Collections;

namespace ParquetViewer.Engine.Types
{
    public interface IMapValue : IComparable<IMapValue>, IComparable, IEnumerable<(object Key, object Value)>
    {
        public ArrayList Keys { get; }
        public Type KeyType { get; }
        public ArrayList Values { get; }
        public Type ValueType { get; }
        (object Key, object Value) GetMapValue(int index);
        int Length { get; }
    }
}
