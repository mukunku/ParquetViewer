using ParquetViewer.Engine.Types;

namespace ParquetViewer.Engine.ParquetNET.Types
{
    public class StructValue : StructValueBase
    {
        internal bool IsList { get; set; }

        internal StructValue(DataRowLite data) : base(data)
        {

        }
    }
}