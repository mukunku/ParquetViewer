using Parquet.Meta;
using Parquet.Schema;
using ParquetViewer.Engine.Exceptions;
using ParquetViewer.Engine.Types;

namespace ParquetViewer.Engine.ParquetNET;

public class ParquetSchemaElement : IParquetSchemaElement
{
    public string Path => SchemaElement.Name;
    public string PathWithParent => string.Concat(Parent?.Parent is not null /*exclude root node*/ ? (Parent.Path + "/") : string.Empty, Path);
    public SchemaElement SchemaElement { get; set; }
    public DataField? DataField { get; set; }
    public ParquetSchemaElement? Parent { get; private set; }

    private readonly Dictionary<string, ParquetSchemaElement> _children = [];
    public IReadOnlyList<ParquetSchemaElement> Children => _children.Values.ToList();

    public FieldTypeId FieldType
    {
        get
        {
            if (DataField is not null)
                return FieldTypeId.Primitive;
            else if (SchemaElement.LogicalType?.LIST is not null || SchemaElement.ConvertedType == Parquet.Meta.ConvertedType.LIST)
                return FieldTypeId.List;
            else if (SchemaElement.LogicalType?.MAP is not null || SchemaElement.ConvertedType == Parquet.Meta.ConvertedType.MAP)
                return FieldTypeId.Map;
            else if (SchemaElement.NumChildren > 0) //Struct
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

    public ParquetSchemaElement(SchemaElement schemaElement)
    {
        ArgumentNullException.ThrowIfNull(schemaElement);

        SchemaElement = schemaElement;
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
            ? result : throw new MalformedFieldException($"Field not found: `{Path}/{name}`");

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
            return _children.TryGetValue(name, out var value)
                ? value : throw new MalformedFieldException($"Field `{Path}` has no child named '{name}'");
        }
    }

    public override string ToString() => Path;

    /// <summary>
    /// Gets the repeated node of this LIST field.
    /// </summary>
    public ParquetSchemaElement GetListField() => GetSingleOrByName("list");

    /// <summary>
    /// Gets the element of a three-level list. Must be called on a LIST field's repeated node (see <see cref="GetListField"/>).
    /// </summary>
    public ParquetSchemaElement GetListItemField()
    {
        //Checked using the schema structure so it doesn't matter how this node was reached
        if (Parent?.FieldType != FieldTypeId.List)
            throw new InvalidOperationException($"Can't get the list item of `{PathWithParent}` as it isn't the repeated node of a LIST field");

        try
        {
            return GetSingleOrByName("item");
        }
        catch (Exception ex)
        {
            throw new UnsupportedFieldException($"Cannot load field `{Path}`. Invalid List type.", ex);
        }
    }

    /// <summary>
    /// Gets the repeated key_value node of this MAP field.
    /// </summary>
    public ParquetSchemaElement GetMapKeyValueField() => GetSingleOrByName("key_value");

    /// <summary>
    /// Gets the map's key. Must be called on a MAP field's key_value node (see <see cref="GetMapKeyValueField"/>).
    /// </summary>
    public ParquetSchemaElement GetMapKeyField()
    {
        //Checked using the schema structure so it doesn't matter how this node was reached
        if (Parent?.FieldType != FieldTypeId.Map)
            throw new InvalidOperationException($"Can't get the map key of `{PathWithParent}` as it isn't the key_value node of a MAP field");

        return GetChildCI("key");
    }

    /// <summary>
    /// Gets the map's value. Must be called on a MAP field's key_value node (see <see cref="GetMapKeyValueField"/>).
    /// </summary>
    public ParquetSchemaElement GetMapValueField()
    {
        //Checked using the schema structure so it doesn't matter how this node was reached
        if (Parent?.FieldType != FieldTypeId.Map)
            throw new InvalidOperationException($"Can't get the map value of `{PathWithParent}` as it isn't the key_value node of a MAP field");

        return GetChildCI("value");
    }

    private FieldLevels? _levels;

    /// <summary>
    /// The repetition/definition level arithmetic for this element, computed once and cached.
    /// See <see cref="FieldLevels"/>.
    /// </summary>
    public FieldLevels Levels => _levels ??= FieldLevels.For(this);

    public bool IsPrimitive => FieldType == FieldTypeId.Primitive;

    ICollection<IParquetSchemaElement> IParquetSchemaElement.Children => Children.ToList<IParquetSchemaElement>();

    public System.Type ClrType => DataField switch
    {
        //Parquet.Net v6 dropped `UseTimeOnlyTypeForTime*`, TIME columns now come back as raw
        //int/long counts of millis/micros/nanos. ParquetColumnData converts them back to TimeOnly so
        //the rest of the app (grid, JSON export, filters) keeps seeing TimeOnly.
        TimeDataField => typeof(TimeOnly),
        //Null DataField means this is a complex type
        null => FieldType switch
        {
            FieldTypeId.List => typeof(ListValue),
            FieldTypeId.Map => typeof(MapValue),
            FieldTypeId.Struct => typeof(StructValue),
            FieldTypeId.Primitive => throw new InvalidOperationException($"Cannot determine CLR type for primitive field `{Path}` without ClrType information."),
            _ => throw new InvalidOperationException($"Unknown complex type {FieldType} detected for field `{Path}`")
        },
        //Parquet.Net v6 handles byte[] and string values as ReadOnlyMemory types.
        _ => DataField.ClrType switch
        {
            _ when DataField.ClrType == typeof(ReadOnlyMemory<char>) => typeof(string),
            _ when DataField.ClrType == typeof(ReadOnlyMemory<byte>) => typeof(ByteArrayValue),
            _ => DataField.ClrType
        }
    };

    public object? LogicalType => LogicalTypeMetadataResolver.LogicalTypeToJSONObject(SchemaElement.LogicalType);

    public RepetitionTypeId? RepetitionType => SchemaElement.RepetitionType switch
    {
        FieldRepetitionType.REQUIRED => RepetitionTypeId.Required,
        FieldRepetitionType.OPTIONAL => RepetitionTypeId.Optional,
        FieldRepetitionType.REPEATED => RepetitionTypeId.Repeated,
        _ => null
    };

    public int? TypeLength => SchemaElement.TypeLength;
    public int? NumChildren => SchemaElement.NumChildren;
    public string? ConvertedType => SchemaElement.ConvertedType?.ToString();
    public int? Scale => SchemaElement.Scale;
    public int? Precision => SchemaElement.Precision;
    public string? Type => SchemaElement.Type?.ToString();

    object? IParquetSchemaElement.LogicalType => LogicalType;

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
}