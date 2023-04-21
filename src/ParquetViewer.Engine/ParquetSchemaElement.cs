using Parquet.Schema;
using Parquet.Thrift;

namespace ParquetViewer.Engine
{
    internal class ParquetSchemaElement
    {
        public string Path => SchemaElement.Name;
        public SchemaElement SchemaElement { get; set; }
        public DataField DataField { get; set; }
        public List<ParquetSchemaElement> Children { get; set; } = new();

        public ParquetSchemaElement(SchemaElement schemaElement)
        {
            if (schemaElement is null)
                throw new ArgumentNullException(nameof(schemaElement));

            this.SchemaElement = schemaElement;
        }

        public ParquetSchemaElement GetChildByName(string name) => Children.Where(c => c.Path.Equals(name)).FirstOrDefault() 
            ?? throw new Exception($"Field schema path not found: {Path}/{name}");
    }
}
