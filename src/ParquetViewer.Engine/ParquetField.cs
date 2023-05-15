namespace ParquetViewer.Engine
{
    internal class ParquetField
    {
        public string Name { get; }
        public List<(Parquet.Meta.SchemaElement Schema, Parquet.Schema.DataField DataField)> Fields { get; } = new();

        public (Parquet.Meta.SchemaElement Schema, Parquet.Schema.DataField DataField) this[int i] => Fields[i];

        internal ParquetField(string name)
        {
            this.Name = name;
        }
    }
}
