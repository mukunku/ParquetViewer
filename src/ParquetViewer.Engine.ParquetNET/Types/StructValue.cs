using ParquetViewer.Engine.Types;

namespace ParquetViewer.Engine.ParquetNET.Types
{
    public class StructValueExt : StructValue
    {
        internal bool IsList { get; set; }

        internal StructValueExt(DataRowLite data) : base(data)
        {

        }
    }
}