using System.Text.Json.Serialization;

namespace ParquetViewer.Engine
{
    public interface IParquetSchemaElement<T> : IParquetSchemaElement where T : IParquetSchemaElement
    {
        new string Path { get; }
        
        new ICollection<T> Children { get; }

        new Type ClrType { get; }

        new FieldTypeId FieldType { get; }

        new RepetitionTypeId? RepetitionType { get; }

        new bool IsPrimitive { get; }

        /// <summary>
        /// Case insensitive version of <see cref="GetChild(string)"/>
        /// Only exists to deal with non-standard Parquet implementations
        /// </summary>
        new T GetChildCI(string name);
        new T GetChild(string name);
        new T GetListField();
        new T GetListItemField();
        new T GetSingleOrByName(string name);
        new T GetMapKeyValueField();
        new T GetMapKeyField();
        new T GetMapValueField();
    }

    public interface IParquetSchemaElement
    {
        string Path { get; }

        ICollection<IParquetSchemaElement> Children { get; }

        [JsonIgnore]
        Type ClrType { get; }

        FieldTypeId FieldType { get; }

        RepetitionTypeId? RepetitionType { get; }

        bool IsPrimitive { get; }

        public string? Type { get; }
        public int? TypeLength { get; }
        public int? NumChildren { get; }
        public string? ConvertedType { get; }
        public int? Scale { get; }
        public int? Precision { get; }
        public object? LogicalType { get; }

        /// <summary>
        /// Case insensitive version of <see cref="GetChild(string)"/>
        /// Only exists to deal with non-standard Parquet implementations
        /// </summary>
        IParquetSchemaElement GetChildCI(string name);
        IParquetSchemaElement GetChild(string name);
        IParquetSchemaElement GetListField();
        IParquetSchemaElement GetListItemField();
        IParquetSchemaElement GetSingleOrByName(string name);
        IParquetSchemaElement GetMapKeyValueField();
        IParquetSchemaElement GetMapKeyField();
        IParquetSchemaElement GetMapValueField();
    }
}