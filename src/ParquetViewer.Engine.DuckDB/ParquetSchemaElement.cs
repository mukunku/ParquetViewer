using DuckDB.NET.Data;
using DuckDB.NET.Native;
using ParquetViewer.Engine.DuckDB.Types;
using ParquetViewer.Engine.Exceptions;

namespace ParquetViewer.Engine.DuckDB
{
    public class ParquetSchemaElement : IParquetSchemaElement<ParquetSchemaElement>
    {
        public string Path { get; }

        public ICollection<ParquetSchemaElement> Children { get; }

        public bool IsPrimitive => this._clrType is not null;

        public Type ClrType => this._clrType ?? this.FieldType switch
        {
            FieldTypeId.List => typeof(ListValue),
            FieldTypeId.Map => typeof(MapValue),
            FieldTypeId.Struct => typeof(StructValue),
            _ => throw new InvalidOperationException("Cannot determine CLR type for primitive field without ClrType information."),
        };

        public FieldTypeId FieldType => this.ConvertedType switch
        {
            "LIST" => FieldTypeId.List,
            "MAP" => FieldTypeId.Map,
            "STRUCT" => FieldTypeId.Struct,
            _ => GuessFieldType(),
        };

        /// <summary>
        /// DuckDB isn't good with metadata resolution it seems. So we have to guess the field type based on available metadata.
        /// </summary>
        private FieldTypeId GuessFieldType()
        {
            if (this._clrType is not null || (this.NumChildren ?? 0) <= 0)
            {
                if (this._repetitionType == RepetitionTypeId.Repeated)
                    return FieldTypeId.List;
                else
                    return FieldTypeId.Primitive;
            }

            if (this.NumChildren == 2)
            {
                try
                {
                    this.GetMapKeyValueField();
                    return FieldTypeId.Map;
                }
                catch { }
            }

            if (this.NumChildren == 1 && this._repetitionType == RepetitionTypeId.Repeated)
                return FieldTypeId.List;

            return FieldTypeId.Struct;
        }

        public RepetitionTypeId? RepetitionType => this._repetitionType;

        public bool IsByteArrayType => _clrType == typeof(ByteArrayValue);

        ICollection<IParquetSchemaElement> IParquetSchemaElement.Children => this.Children.ToList<IParquetSchemaElement>();

        public string? Type => this._underlyingType;

        private string? _underlyingType;
        public int? TypeLength { get; }
        private RepetitionTypeId? _repetitionType;
        public int? NumChildren { get; }
        public string? ConvertedType { get; }
        public int? Scale { get; }
        public int? Precision { get; }
        private string? _fieldId;
        public object? LogicalType { get; }
        private DuckDBType? _duckDbType;
        private Type? _clrType;

        public ParquetSchemaElement(
            string path,
            string? underlyingType,
            int? typeLength,
            RepetitionTypeId? repetitionType,
            long? numChildren,
            string? convertedType,
            long? scale,
            long? precision,
            string? fieldId,
            string? logicalType,
            DuckDBType? duckDbType,
            Type? ClrType)
        {
            this.Children = new List<ParquetSchemaElement>();
            this.Path = path;
            this._underlyingType = underlyingType;
            this.TypeLength = typeLength;
            this._repetitionType = repetitionType;
            this.NumChildren = (int?)numChildren;
            this.ConvertedType = convertedType;
            this.Scale = (int?)scale;
            this.Precision = (int?)precision;
            this._fieldId = fieldId;
            this.LogicalType = logicalType;
            this._duckDbType = duckDbType;
            this._clrType = ClrType;
        }

        public static ParquetSchemaElement FromRow(DuckDBDataReader row)
        {
            string columnName = row.GetString(1);
            string? columnTypeName = row.IsDBNull(2) ? null : row.GetString(2);
            string? typeLengthString = row.IsDBNull(3) ? null : row.GetString(3);
            string? repetitionTypeName = row.IsDBNull(4) ? null : row.GetString(4);
            long? numChildren = row.IsDBNull(5) ? null : row.GetInt64(5);
            string? convertedType = row.IsDBNull(6) ? null : row.GetString(6);
            long? scale = row.IsDBNull(7) ? null : row.GetInt64(7);
            long? precision = row.IsDBNull(8) ? null : row.GetInt64(8);
            string? fieldId = row.IsDBNull(9) ? null : row.GetString(9);
            string? logicalType = row.IsDBNull(10) ? null : row.GetString(10);
            string? duckDbTypeName = row.IsDBNull(11) ? null : row.GetString(11); //Note: This field isn't returned for complex types like LIST, MAP, STRUCT unfortunately

            int? typeLength = int.TryParse(typeLengthString, out var typeLengthValue) ? typeLengthValue : null;

            DuckDBType? duckDBType = null;
            Type? clrType = null;
            if (duckDbTypeName is not null)
            {
                (duckDBType, clrType) = DuckDBHelper.ParseDuckDBType(duckDbTypeName, columnTypeName);
            }

            RepetitionTypeId? repetitionType = null;
            if (repetitionTypeName is not null)
            {
                repetitionType = repetitionTypeName.ToUpperInvariant() switch
                {
                    "REQUIRED" => RepetitionTypeId.Required,
                    "OPTIONAL" => RepetitionTypeId.Optional,
                    "REPEATED" => RepetitionTypeId.Repeated,
                    _ => throw new ArgumentOutOfRangeException(nameof(repetitionTypeName), $"Unsupported repetition type: {repetitionTypeName}")
                };
            }

            var element = new ParquetSchemaElement(
                    columnName,
                    columnTypeName,
                    typeLength,
                    repetitionType,
                    numChildren,
                    convertedType,
                    scale,
                    precision,
                    fieldId,
                    logicalType,
                    duckDBType,
                    clrType);

            return element;
        }

        public ParquetSchemaElement GetSingleOrByName(string name)
        {
            if (this.Children.Count == 0)
            {
                throw new MalformedFieldException($"Field `{Path}` has no children. Expected '{name}'.");
            }

            if (this.Children.Count == 1)
            {
                return this.Children.First();
            }
            else
            {
                return this.Children.FirstOrDefault(c => c.Path == name)
                    ?? throw new MalformedFieldException($"Field `{Path}` has no child named '{name}'");
            }
        }

        public ParquetSchemaElement GetListField()
        {
            var field = this.GetSingleOrByName("list");
            return field;
        }
        public ParquetSchemaElement GetListItemField()
        {
            try
            {
                if (this.Children.Count == 0)
                {
                    //Assume this is a 2-tier list...
                    return this;
                }

                var field = this.GetSingleOrByName("item");
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
            return field;
        }
        public ParquetSchemaElement GetMapKeyField()
        {
            var field = this.GetChildCI("key");
            return field;
        }
        public ParquetSchemaElement GetMapValueField()
        {
            var field = this.GetChildCI("value");
            return field;
        }

        public ParquetSchemaElement GetChildCI(string name) =>
            Children.First((f) => f.Path.Equals(name, StringComparison.InvariantCultureIgnoreCase));

        public ParquetSchemaElement GetChild(string name)
            => Children.First((f) => f.Path.Equals(name));

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
}