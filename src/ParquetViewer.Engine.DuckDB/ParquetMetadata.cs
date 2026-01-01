using DuckDB.NET.Data;
using DuckDB.NET.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static ParquetViewer.Engine.IParquetSchemaElement;

namespace ParquetViewer.Engine.DuckDB
{
    public class ParquetMetadata : IParquetMetadata
    {
        public int ParquetVersion { get; }

        public int RowGroupCount { get; }

        public int RowCount { get; }

        public string CreatedBy { get; }

        public ICollection<IRowGroupMetadata> RowGroups { get; }

        public IParquetSchemaElement SchemaTree { get; }

        private ParquetMetadata(IParquetSchemaElement schemaTree)
        {
            this.SchemaTree = schemaTree;
        }

        public static async Task<ParquetMetadata> FromDuckDBAsync(DuckDBConnection db, string parquetFilePath)
        {
            var metadata =  new ParquetMetadata(await DuckDBHelper.GetParquetSchemaTreeAsync(db, parquetFilePath));
            return metadata;
        }
    }

    public class RowGroupMetadata : IRowGroupMetadata
    {
        public int Ordinal { get; }
        public int RowCount { get; }
        public ICollection<ISortingColumnMetadata>? SortingColumns { get; }
        public long FileOffset { get; }
        public long TotalByteSize { get; }
        public long TotalCompressedSize { get; }
        public RowGroupMetadata()
        {
        }
    }

    public class ParquetSchemaElement : IParquetSchemaElement
    {
        public string Path { get; }

        public ICollection<IParquetSchemaElement> Children { get; }

        public bool IsPrimitive => throw new NotImplementedException();

        public int? NumChildren => (int?)this._numChildren;

        private string? _underlyingType;
        private string? _typeLength;
        private RepetitionTypeId? _repetitionType;
        private long? _numChildren;
        private string? _convertedType;
        private long? _scale;
        private long? _precision;
        private string? _fieldId;
        private string? _logicalType;
        private DuckDBType? _duckDbType;
        private Type? _clrType;

        public ParquetSchemaElement(
            string path, 
            string? underlyingType, 
            string? typeLength,
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
            this.Children = new List<IParquetSchemaElement>();
            this.Path = path;
            this._underlyingType = underlyingType;
            this._typeLength = typeLength;
            this._repetitionType = repetitionType;
            this._numChildren = numChildren;
            this._convertedType = convertedType;
            this._scale = scale;
            this._precision = precision;
            this._fieldId = fieldId;
            this._logicalType = logicalType;
            this._duckDbType = duckDbType;
            this._clrType = ClrType;
        }

        public static ParquetSchemaElement FromRow(DuckDBDataReader row)
        {
            string columnName = row.GetString(1);
            string? columnTypeName = row.IsDBNull(2) ? null : row.GetString(2);
            string? typeLength = row.IsDBNull(3) ? null : row.GetString(3);
            string? repetitionTypeName = row.IsDBNull(4) ? null : row.GetString(4);
            long? numChildren = row.IsDBNull(5) ? null : row.GetInt64(5);
            string? convertedType = row.IsDBNull(6) ? null : row.GetString(6);
            long? scale = row.IsDBNull(7) ? null : row.GetInt64(7);
            long? precision = row.IsDBNull(8) ? null : row.GetInt64(8);
            string? fieldId = row.IsDBNull(9) ? null : row.GetString(9);
            string? logicalType = row.IsDBNull(10) ? null : row.GetString(10);
            string? duckDbTypeName = row.IsDBNull(11) ? null : row.GetString(11);

            DuckDBType? duckDBType = null;
            Type? clrType = null;
            if (duckDbTypeName is not null)
            {
                (duckDBType, clrType) = DuckDBHelper.ParseDuckDBType(duckDbTypeName);
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
    }
}
