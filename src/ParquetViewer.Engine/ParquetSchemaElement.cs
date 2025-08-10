using Parquet.Meta;
using Parquet.Schema;
using ParquetViewer.Engine.Exceptions;

namespace ParquetViewer.Engine
{
    public class ParquetSchemaElement
    {
        public string Path => SchemaElement.Name;
        public SchemaElement SchemaElement { get; set; }
        public DataField? DataField { get; set; }
        public ParquetSchemaElement? Parent { get; private set; }

        private readonly Dictionary<string, ParquetSchemaElement> _children = new();
        public IReadOnlyList<ParquetSchemaElement> Children => _children.Values.ToList();

        public IEnumerable<ParquetSchemaElement> Parents
        {
            get
            {
                var current = this.Parent;
                while (current is not null)
                {
                    //Don't return the root node
                    var isRoot = current.Parent is null;
                    if (isRoot)
                        break;

                    yield return current;
                    current = current.Parent;
                }
            }
        }

        public void AddChild(ParquetSchemaElement child)
        {
            _children.Add(child.Path, child);
            child.Parent = this;
        }

        public ParquetSchemaElement(SchemaElement schemaElement)
        {
            if (schemaElement is null)
                throw new ArgumentNullException(nameof(schemaElement));

            this.SchemaElement = schemaElement;
        }

        public ParquetSchemaElement GetChild(string name) => GetChildImpl(name);

        /// <summary>
        /// Case insensitive version of <see cref="GetChild(string)"/>
        /// Only exists to deal with non-standard Parquet implementations
        /// </summary>
        public ParquetSchemaElement GetChildCI(string name) => 
            GetChildImpl(_children.Keys.FirstOrDefault((key) => key?.Equals(name, StringComparison.InvariantCultureIgnoreCase) == true) ?? name);

        private ParquetSchemaElement GetChildImpl(string? name) => name is not null && _children.TryGetValue(name, out var result)
                ? result : throw new MalformedFieldException($"Field schema path not found: `{Path}/{name}`");

        public ParquetSchemaElement GetSingleOrByName(string name)
        {
            if (_children.Count == 0)
            {
                throw new MalformedFieldException($"Field `{Path}` has no children. Expected '{name}'.");
            }

            if (_children.Count == 1)
            {
                return _children.First().Value;
            }
            else
            {
                return _children.ContainsKey(name) 
                    ? _children[name] : throw new MalformedFieldException($"Field `{Path}` has no child named '{name}'");
            }
        }

        public override string ToString() => this.Path;

        public FieldTypeId FieldType()
        {
            if (this.DataField is not null)
                return FieldTypeId.Primitive;
            else if (this.SchemaElement.LogicalType?.LIST is not null || this.SchemaElement.ConvertedType == ConvertedType.LIST)
                return FieldTypeId.List;
            else if (this.SchemaElement.LogicalType?.MAP is not null || this.SchemaElement.ConvertedType == ConvertedType.MAP)
                return FieldTypeId.Map;
            else if (this.SchemaElement.NumChildren > 0) //Struct
                return FieldTypeId.Struct;
            else
                throw new UnsupportedFieldException($"Could not determine field type for `{Path}`");
        }

        public enum FieldTypeId
        {
            Primitive,
            List,
            Struct,
            Map
        }
    }
}
