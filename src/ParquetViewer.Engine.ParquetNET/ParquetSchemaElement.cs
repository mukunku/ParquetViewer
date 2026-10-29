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

    private SystemFieldTypeId? _systemFieldType = null;
    public ParquetSchemaElement GetListField()
    {
        var field = GetSingleOrByName("list");
        field._systemFieldType = SystemFieldTypeId.ListNode;
        return field;
    }
    public ParquetSchemaElement GetListItemField()
    {
        if (_systemFieldType != SystemFieldTypeId.ListNode)
            throw GetSystemFieldAccessException(SystemFieldTypeId.ListItemNode);

        try
        {
            var field = GetSingleOrByName("item");
            field._systemFieldType = SystemFieldTypeId.ListItemNode;
            return field;
        }
        catch (Exception ex)
        {
            throw new UnsupportedFieldException($"Cannot load field `{Path}`. Invalid List type.", ex);
        }
    }
    public ParquetSchemaElement GetMapKeyValueField()
    {
        var field = GetSingleOrByName("key_value");
        field._systemFieldType = SystemFieldTypeId.MapKeyValueNode;
        return field;
    }
    public ParquetSchemaElement GetMapKeyField()
    {
        if (_systemFieldType != SystemFieldTypeId.MapKeyValueNode)
            throw GetSystemFieldAccessException(SystemFieldTypeId.MapKeyNode);

        var field = GetChildCI("key");
        field._systemFieldType = SystemFieldTypeId.MapKeyNode;
        return field;
    }
    public ParquetSchemaElement GetMapValueField()
    {
        if (_systemFieldType != SystemFieldTypeId.MapKeyValueNode)
            throw GetSystemFieldAccessException(SystemFieldTypeId.MapValueNode);

        var field = GetChildCI("value");
        field._systemFieldType = SystemFieldTypeId.MapValueNode;
        return field;
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
        //Parquet.Net v6 handles byte[] and string values as ReadOnlyMemory types
        _ => DataField.ClrType switch
        {
            _ when DataField.ClrType == typeof(ReadOnlyMemory<char>) => typeof(string),
            _ when DataField.ClrType == typeof(ReadOnlyMemory<byte>) => typeof(byte[]),
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

    private InvalidOperationException GetSystemFieldAccessException(SystemFieldTypeId fieldType)
        => new($"Can't get {fieldType} node from '{Parent?._systemFieldType}' " +
                $"for `{Parent?.Path + '/' + Path}` with types '{Parent?.FieldType.ToString() + '/' + FieldType.ToString()}'");

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