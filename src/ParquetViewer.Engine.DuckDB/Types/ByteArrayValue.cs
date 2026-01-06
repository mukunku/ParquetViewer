using ParquetViewer.Engine.Types;

namespace ParquetViewer.Engine.DuckDB.Types
{
    public class ByteArrayValue : ByteArrayValueBase
    {
        public ByteArrayValue(byte[] data) : base(data)
        {

        }
    }
}
