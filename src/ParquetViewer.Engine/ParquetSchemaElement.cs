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

        public void AddChild(ParquetSchemaElement child)
        {
            child.SetParent(this);
            _children.Add(child.Path, child);
        }

        public void SetParent(ParquetSchemaElement parent)
        {
            if (Parent is null)
            {
                Parent = parent;
            }
            else
            {
                throw new Exception("Parent already set");
            }    
        }

        public ParquetSchemaElement(SchemaElement schemaElement)
        {
            ArgumentNullException.ThrowIfNull(schemaElement);

            this.SchemaElement = schemaElement;
        }

        public ParquetSchemaElement GetChild(string name) => GetChildImpl(name);

        /// <summary>
        /// Case insensitive version of <see cref="GetChild(string)"/>
        /// Only exists to deal with non-standard Parquet implementations
        /// </summary>
        public ParquetSchemaElement GetChildCI(string name) => 
            GetChildImpl(_children.Keys.FirstOrDefault((key) => key?.Equals(name, StringComparison.InvariantCultureIgnoreCase) == true) ?? name);

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


        //No clue if this makes sense but it works for now
        public bool BelongToListField()
        {
            var parent = this.Parent;
            while (parent is not null)
            {
                if (parent.FieldType() == FieldTypeId.List)
                {
                    return true;
                }
                parent = parent.Parent;
            }
            return false;
        }
    }
}
