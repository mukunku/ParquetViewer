using Parquet.Meta;
using Parquet.Schema;

namespace ParquetViewer.Engine
{
    public class ParquetSchemaElement
    {
        public string Path => SchemaElement.Name;
        public SchemaElement SchemaElement { get; set; }
        public DataField? DataField { get; set; }

        private readonly Dictionary<string, ParquetSchemaElement> _children = new();
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

        public ParquetSchemaElement GetChild(string name) => GetChildImpl(name);
    
        public ParquetSchemaElement GetChild(string? parent, string name)
        {
            if (parent is null)
            {
                return GetChildImpl(name);
            }

            var child = GetChildImpl(parent);
            return child.GetChild(name);
        }

        private ParquetSchemaElement GetChildImpl(string? name) => name is not null && _children.TryGetValue(name, out var result)
                ? result : throw new Exception($"Field schema path not found: `{Path}/{name}`");

        public ParquetSchemaElement GetImmediateChildOrSingle(string name)
        {
            if (_children.TryGetValue(name, out var result))
            {
                return result;
            }

            if (_children.Count == 1)
            {
                return _children.First().Value;
            }

            throw new Exception($"Field schema path not found: `{Path}/{name}`");
        }
    }
}
