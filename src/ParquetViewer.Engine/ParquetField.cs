namespace ParquetViewer.Engine
{
    internal class ParquetField
    {
        public string Name { get; }
        public List<(Parquet.Thrift.SchemaElement Schema, Parquet.Schema.DataField DataField)> Fields { get; } = new();

        public (Parquet.Thrift.SchemaElement Schema, Parquet.Schema.DataField DataField) this[int i] => Fields[i];

        internal ParquetField(string name)
        {
            this.Name = name;
        }
    }
}
