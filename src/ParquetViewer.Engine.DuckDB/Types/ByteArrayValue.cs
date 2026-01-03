using ParquetViewer.Engine.Types;

namespace ParquetViewer.Engine.DuckDB.Types
{
    public class ByteArrayValue : ByteArrayValueBase
    {
        public ByteArrayValue(string name, byte[] data) : base(name, data)
        {

        }
    }
}
