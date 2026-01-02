namespace ParquetViewer.Engine.ParquetNET.Types
{
    public class StructValue : StructValueBase
    {
        internal bool IsList { get; set; }

        internal StructValue(string name, DataRowLite data) : base(name, data)
        {

        }
    }
}
