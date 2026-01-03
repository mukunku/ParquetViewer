using Parquet.Meta;
using Parquet.Schema;
using ParquetViewer.Engine.Exceptions;
using ParquetViewer.Engine.ParquetNET.Types;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using static ParquetViewer.Engine.IParquetSchemaElement;

namespace ParquetViewer.Engine.ParquetNET
{
    public class ParquetSchemaElement : IParquetSchemaElement
    {
        public string Path => SchemaElement.Name;
        public string PathWithParent => string.Concat(this.Parent?.Parent is not null /*exclude root node*/ ? (this.Parent.Path + "/") : string.Empty, Path);
        public SchemaElement SchemaElement { get; set; }
        public DataField? DataField { get; set; }
        public ParquetSchemaElement? Parent { get; private set; }

        private readonly Dictionary<string, ParquetSchemaElement> _children = new();
        public IReadOnlyList<ParquetSchemaElement> Children => _children.Values.ToList();

        private IEnumerable<ParquetSchemaElement> _parentsExcludingRoot
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

        public FieldTypeId FieldType
        {
            get
            {
                if (this.DataField is not null)
                    return FieldTypeId.Primitive;
                else if (this.SchemaElement.LogicalType?.LIST is not null || this.SchemaElement.ConvertedType == Parquet.Meta.ConvertedType.LIST)
                    return FieldTypeId.List;
                else if (this.SchemaElement.LogicalType?.MAP is not null || this.SchemaElement.ConvertedType == Parquet.Meta.ConvertedType.MAP)
                    return FieldTypeId.Map;
                else if (this.SchemaElement.NumChildren > 0) //Struct
                    return FieldTypeId.Struct;
                else
                    throw new UnsupportedFieldException($"Could not determine field type for `{Path}`");
            }
        }

        public void AddChild(ParquetSchemaElement child)
        {
            _children.Add(child.Path, child);
            child.Parent = this;
        }

        public ParquetSchemaElement(SchemaElement schemaElement, System.Type? clrType)
        {
            if (schemaElement is null)
                throw new ArgumentNullException(nameof(schemaElement));

            this.SchemaElement = schemaElement;
            this._clrType = clrType;
        }

        public ParquetSchemaElement GetChild(string name)
        {
            try
            {
                return GetChildImpl(name);
            }
            catch (MalformedFieldException mfe)
            {
                try
                {
                    GetChildCI(name);
                    mfe.Data["has_ci_match"] = true;
                }
                catch
                {
                    mfe.Data["has_ci_match"] = false;
                }
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Case insensitive version of <see cref="GetChild(string)"/>
        /// Only exists to deal with non-standard Parquet implementations
        /// </summary>
        private ParquetSchemaElement GetChildCI(string name) =>
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

        private SystemFieldTypeId? _systemFieldType = null;
        public ParquetSchemaElement GetListField()
        {
            var field = this.GetSingleOrByName("list");
            field._systemFieldType = SystemFieldTypeId.ListNode;
            return field;
        }
        public ParquetSchemaElement GetListItemField()
        {
            if (this._systemFieldType != SystemFieldTypeId.ListNode)
                throw GetSystemFieldAccessException(SystemFieldTypeId.ListItemNode);

            try
            {
                var field = this.GetSingleOrByName("item");
                field._systemFieldType = SystemFieldTypeId.ListItemNode;
                return field;
            }
            catch (Exception ex)
            {
                throw new UnsupportedFieldException($"Cannot load field `{this.Path}`. Invalid List type.", ex);
            }
        }
        public ParquetSchemaElement GetMapKeyValueField()
        {
            var field = this.GetSingleOrByName("key_value");
            field._systemFieldType = SystemFieldTypeId.MapKeyValueNode;
            return field;
        }
        public ParquetSchemaElement GetMapKeyField()
        {
            if (this._systemFieldType != SystemFieldTypeId.MapKeyValueNode)
                throw GetSystemFieldAccessException(SystemFieldTypeId.MapKeyNode);

            var field = this.GetChildCI("key");
            field._systemFieldType = SystemFieldTypeId.MapKeyNode;
            return field;
        }
        public ParquetSchemaElement GetMapValueField()
        {
            if (this._systemFieldType != SystemFieldTypeId.MapKeyValueNode)
                throw GetSystemFieldAccessException(SystemFieldTypeId.MapValueNode);

            var field = this.GetChildCI("value");
            field._systemFieldType = SystemFieldTypeId.MapValueNode;
            return field;
        }
        public bool BelongsToListField => this._systemFieldType == SystemFieldTypeId.ListItemNode;
        public bool BelongsToListOfStructsField => 
            this.Parent?._systemFieldType == SystemFieldTypeId.ListItemNode && this.Parent?.FieldType == FieldTypeId.Struct;
        public int NumberOfListParents => _parentsExcludingRoot.Count(field => field.SchemaElement.RepetitionType == FieldRepetitionType.REPEATED);

        public int CurrentDefinitionLevel => _parentsExcludingRoot.Append(this)
            .Count(
                field => field.SchemaElement.RepetitionType == FieldRepetitionType.OPTIONAL
                || (field._systemFieldType == SystemFieldTypeId.ListNode && field.Parent?._systemFieldType == SystemFieldTypeId.ListItemNode) //Fixes list-of-lists tests
            );

        public bool IsPrimitive => FieldType == FieldTypeId.Primitive;

        ICollection<IParquetSchemaElement> IParquetSchemaElement.Children => this.Children.ToList<IParquetSchemaElement>();

        private System.Type? _clrType = null;
        public System.Type ClrType => this._clrType ?? this.FieldType switch
        {
            FieldTypeId.List => typeof(ListValue),
            FieldTypeId.Map => typeof(MapValue),
            FieldTypeId.Struct => typeof(StructValue),
            _ => throw new InvalidOperationException("Cannot determine CLR type for primitive field without ClrType information."),
        };

        public RepetitionTypeId? RepetitionType => this.SchemaElement.RepetitionType switch
        {
            FieldRepetitionType.REQUIRED => RepetitionTypeId.Required,
            FieldRepetitionType.OPTIONAL => RepetitionTypeId.Optional,
            FieldRepetitionType.REPEATED => RepetitionTypeId.Repeated,
            _ => null
        };

        private Exception GetSystemFieldAccessException(SystemFieldTypeId fieldType)
            => new InvalidOperationException($"Can't get {fieldType} node from '{this.Parent?._systemFieldType}' " +
                    $"for `{this.Parent?.Path + '/' + this.Path}` with types '{this.Parent?.FieldType.ToString() + '/' + this.FieldType.ToString()}'");

        IParquetSchemaElement IParquetSchemaElement.GetChildCI(string name)
            => GetChildCI(name);

        IParquetSchemaElement IParquetSchemaElement.GetChild(string name)
            => GetChild(name);

        IParquetSchemaElement IParquetSchemaElement.GetListField()
            => GetListField();

        IParquetSchemaElement IParquetSchemaElement.GetListItemField()
            => GetListItemField();

        IParquetSchemaElement IParquetSchemaElement.GetSingleOrByName(string name)
            => GetSingleOrByName(name);

        IParquetSchemaElement IParquetSchemaElement.GetMapKeyValueField()
            => GetMapKeyValueField();

        IParquetSchemaElement IParquetSchemaElement.GetMapKeyField()
            => GetMapKeyField();

        IParquetSchemaElement IParquetSchemaElement.GetMapValueField()
            => GetMapValueField();

        private enum SystemFieldTypeId
        {
            ListNode,
            ListItemNode,
            StructValueNode,
            MapKeyValueNode,
            MapKeyNode,
            MapValueNode
        }
    }
}
