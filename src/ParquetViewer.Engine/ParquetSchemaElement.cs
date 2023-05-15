using Parquet.Meta;
using Parquet.Schema;

namespace ParquetViewer.Engine
{
    internal class ParquetSchemaElement
    {
        public string Path => SchemaElement.Name;
        public SchemaElement SchemaElement { get; set; }
        public DataField? DataField { get; set; }

        private Dictionary<string, ParquetSchemaElement> _children = new();
        public IReadOnlyList<ParquetSchemaElement> Children => _children.Values.ToList();

        public void AddChild(ParquetSchemaElement child)
        {
            _children.Add(child.Path, child);
        }

        public ParquetSchemaElement(SchemaElement schemaElement)
        {
            if (schemaElement is null)
                throw new ArgumentNullException(nameof(schemaElement));

            this.SchemaElement = schemaElement;
        }

        public ParquetSchemaElement GetChildByName(string name) => _children.TryGetValue(name, out var result)
            ? result : throw new Exception($"Field schema path not found: {Path}/{name}");
    }
}
