using Parquet;
using Parquet.Meta;
using ParquetViewer.Engine.Exceptions;
using ParquetViewer.Engine.Types;
using System.Collections;
using System.Data;
using System.Reflection.Metadata.Ecma335;

namespace ParquetViewer.Engine
{
    public partial class ParquetEngine
    {
        public static readonly string TotalRecordCountExtendedPropertyKey = "TOTAL_RECORD_COUNT";

        public async Task<Func<bool, DataTable>> ReadRowsAsync(List<string> selectedFields, int offset, int recordCount, CancellationToken cancellationToken, IProgress<int>? progress = null)
        {
            long recordsLeftToRead = recordCount;
            DataTableLite result = BuildDataTable(null, selectedFields, Math.Min(recordCount, (int)this.RecordCount));

            foreach (var reader in this.GetReaders(offset))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (recordsLeftToRead <= 0)
                    break;

                recordsLeftToRead = await PopulateDataTable(result, reader.ParquetReader, reader.RemainingOffset, recordsLeftToRead, cancellationToken, progress);
            }

            result.DataSetSize = this.RecordCount;

            return (logProgress) =>
            {
                var datatable = result.ToDataTable(cancellationToken, logProgress ? progress : null);
                datatable.ExtendedProperties[TotalRecordCountExtendedPropertyKey] = result.DataSetSize;
                return datatable;
            };
        }

		private async Task<long> PopulateDataTable(DataTableLite dataTable, ParquetReader parquetReader,
            long offset, long recordCount, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            //Read column by column to generate each row in the datatable
            int totalRecordCountSoFar = 0;
            long rowsLeftToRead = recordCount;
            for (int i = 0; i < parquetReader.RowGroupCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (ParquetRowGroupReader groupReader = parquetReader.OpenRowGroupReader(i))
                {
                    if (groupReader.RowCount > int.MaxValue)
                        throw new ArgumentOutOfRangeException(string.Format("Cannot handle row group sizes greater than {0}. Found {1} instead.", int.MaxValue, groupReader.RowCount));

                    int rowsPassedUntilThisRowGroup = totalRecordCountSoFar;
                    totalRecordCountSoFar += (int)groupReader.RowCount;

                    if (offset >= totalRecordCountSoFar)
                        continue;

                    if (rowsLeftToRead <= 0)
                        break;

                    long numberOfRecordsToReadFromThisRowGroup = Math.Min(Math.Min(totalRecordCountSoFar - offset, rowsLeftToRead), groupReader.RowCount);
                    rowsLeftToRead -= numberOfRecordsToReadFromThisRowGroup;

                    long recordsToSkipInThisRowGroup = Math.Max(offset - rowsPassedUntilThisRowGroup, 0);

                    await ProcessRowGroup(dataTable, groupReader, recordsToSkipInThisRowGroup, numberOfRecordsToReadFromThisRowGroup, cancellationToken, progress);
                }
            }

            return rowsLeftToRead;
        }

        private async Task ProcessRowGroup(DataTableLite dataTable, ParquetRowGroupReader groupReader,
            long skipRecords, long readRecords, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            int rowBeginIndex = dataTable.Rows.Count;
            bool isFirstColumn = true;

            foreach (DataTableLite.ColumnLite column in dataTable.Columns.Values)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var field = column.ParentSchema.GetChild(column.Name);
                switch (field.FieldType())
                {
                    case ParquetSchemaElement.FieldTypeId.Primitive:
                        await ReadPrimitiveField(dataTable, groupReader, rowBeginIndex, field, skipRecords,
                            readRecords, isFirstColumn, cancellationToken, progress);
                        break;
                    case ParquetSchemaElement.FieldTypeId.List:
                        var listField = field.GetSingle("list");
                        ParquetSchemaElement itemField;
                        try
                        {
                            itemField = listField.GetSingle("item");
                        }
                        catch (Exception ex)
                        {
                            throw new UnsupportedFieldException($"Cannot load field `{field.Path}`. Invalid List type.", ex);
                        }
                        var fieldIndex = dataTable.Columns[field.Path]!.Ordinal;
                        await ReadListField(dataTable, groupReader, rowBeginIndex, itemField, fieldIndex,
                            skipRecords, readRecords, isFirstColumn, cancellationToken, progress);
                        break;
                    case ParquetSchemaElement.FieldTypeId.Map:
                        await ReadMapField(dataTable, groupReader, field, skipRecords,
                            readRecords, isFirstColumn, cancellationToken, progress);
                        break;
                    case ParquetSchemaElement.FieldTypeId.Struct:
                        await ReadStructField(dataTable, groupReader, rowBeginIndex, field, skipRecords,
                            readRecords, isFirstColumn, cancellationToken, progress);
                        break;
                }

                isFirstColumn = false;
            }
        }

		private async Task ReadPrimitiveField(DataTableLite dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex, ParquetSchemaElement field,
        long skipRecords, long readRecords, bool isFirstColumn, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            int rowIndex = rowBeginIndex;

            int skippedRecords = 0;
            var dataColumn = await groupReader.ReadColumnAsync(field.DataField ?? throw new Exception($"Pritimive field `{field.Path}` is missing its data field"), cancellationToken);

            bool doesFieldBelongToAList = dataColumn.RepetitionLevels?.Any(l => l > 0) ?? false;
            int fieldIndex = dataTable.Columns[field.Path]?.Ordinal ?? throw new Exception($"Column `{field.Path}` is missing");
            if (doesFieldBelongToAList)
            {
                dataColumn = null;
                await ReadListField(dataTable, groupReader, rowBeginIndex, field, fieldIndex, skipRecords, readRecords, isFirstColumn, cancellationToken, progress);
            }
            else
            {
                var fieldType = dataTable.Columns[field.Path].Type; var byteArrayValueType = typeof(ByteArrayValue);
                foreach (var value in dataColumn.Data)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (skipRecords > skippedRecords)
                    {
                        skippedRecords++;
                        continue;
                    }

                    if (rowIndex - rowBeginIndex >= readRecords)
                        break;

                    if (isFirstColumn)
                    {
                        dataTable.NewRow();
                    }

                    if (value == DBNull.Value || value is null)
                    {
                        dataTable.Rows[rowIndex]![fieldIndex] = DBNull.Value;
                    }
                    else if (fieldType == byteArrayValueType)
                    {
                        dataTable.Rows[rowIndex]![fieldIndex] = new ByteArrayValue(field.Path, (byte[])value);
                    }
                    else
                    {
                        dataTable.Rows[rowIndex]![fieldIndex] = FixDateTime(value, field);
                    }

                    rowIndex++;
                    progress?.Report(1);
                }
            }
        }

        private async Task ReadListField(DataTableLite dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex, ParquetSchemaElement itemField, int fieldIndex,
            long skipRecords, long readRecords, bool isFirstColumn, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            if (itemField.FieldType() == ParquetSchemaElement.FieldTypeId.Primitive)
            {
                int rowIndex = rowBeginIndex;

                int skippedRecords = 0;
                var dataColumn = await groupReader.ReadColumnAsync(itemField.DataField!, cancellationToken);

                ArrayList? rowValue = null;
                for (int i = 0; i < dataColumn.Data.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    rowValue ??= new ArrayList();

                    bool IsEndOfRow() => (i + 1) == dataColumn.RepetitionLevels!.Length
                        || dataColumn.RepetitionLevels[i + 1] == 0; //0 means new list

                    //Skip rows
                    while (skipRecords > skippedRecords)
                    {
                        if (IsEndOfRow())
                            skippedRecords++;

                        i++;
                    }

                    //If we skipped to the end then just exit
                    if (i == dataColumn.Data.Length)
                        break;

                    if (IsEndOfRow())
                    {
                        if (isFirstColumn)
                        {
                            dataTable.NewRow();
                        }

                        var lastItem = dataColumn.Data.GetValue(i) ?? DBNull.Value;
                        rowValue.Add(lastItem);

                        dataTable.Rows[rowIndex]![fieldIndex] = new ListValue(rowValue, itemField.DataField!.ClrType);
                        rowValue = null;

                        rowIndex++;
                        progress?.Report(1);

                        if (rowIndex - rowBeginIndex >= readRecords)
                            break;
                    }
                    else
                    {
                        var value = dataColumn.Data.GetValue(i) ?? DBNull.Value;
                        rowValue.Add(value);
                    }
                }
            }
            else if (itemField.FieldType() == ParquetSchemaElement.FieldTypeId.Struct)
            {
                //Read struct data as a new datatable
                DataTableLite structFieldTable = BuildDataTable(itemField, itemField.Children.Select(f => f.Path).ToList(), (int)readRecords);

                //Need to calculate progress differently for structs
                var structFieldReadProgress = StructReadProgress(progress, structFieldTable.Columns.Count);

                //Read the struct data and populate the datatable
                await ProcessRowGroup(structFieldTable, groupReader, skipRecords, readRecords, cancellationToken, structFieldReadProgress);

                //We need to pivot the data into a new data table (because we read it in columnar fashion above)
                int rowIndex = rowBeginIndex;
                foreach (var values in structFieldTable.Rows)
                {
                    var newStructFieldTable = BuildDataTable(itemField, itemField.Children.Select(f => f.Path).ToList(), (int)readRecords);
                    for (var columnOrdinal = 0; columnOrdinal < values.Length; columnOrdinal++)
                    {
                        if (values[columnOrdinal] == DBNull.Value) 
                        {
                            //Empty array
                            continue;
                        }

                        var columnValues = (ListValue)values[columnOrdinal];
                        for (var rowValueIndex = 0; rowValueIndex < columnValues.Data.Count; rowValueIndex++)
                        {
                            var columnValue = columnValues.Data[rowValueIndex] ?? throw new SystemException("This should never happen");
                            bool isFirstValueColumn = columnOrdinal == 0;
                            if (isFirstValueColumn)
                            {
                                newStructFieldTable.NewRow();
                            }
                            newStructFieldTable.Rows[rowValueIndex][columnOrdinal] = columnValue;
                        }
                    }

                    if (isFirstColumn)
                        dataTable.NewRow();

                    var listValuesDataTable = newStructFieldTable.ToDataTable(cancellationToken);
                    var listValues = new ArrayList(listValuesDataTable.Rows.Count);
                    foreach (DataRow row in listValuesDataTable.Rows)
                    {
                        var newStructField = new StructValue(itemField.Path, row);
                        listValues.Add(newStructField);
                    }

                    dataTable.Rows[rowIndex][fieldIndex] = new ListValue(listValues, typeof(StructValue));
                    rowIndex++;
                }
            }
            else
            {
                throw new NotSupportedException($"Lists of {itemField.FieldType()}s are not currently supported");
            }
        }

        private async Task ReadMapField(DataTableLite dataTable, ParquetRowGroupReader groupReader, ParquetSchemaElement field,
            long skipRecords, long readRecords, bool isFirstColumn, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            var keyValueField = field.GetSingle("key_value");
            var keyField = keyValueField.GetChildCI("key");
            var valueField = keyValueField.GetChildCI("value");

            if (keyField.Children.Any() || valueField.Children.Any())
                throw new UnsupportedFieldException($"Cannot load field `{field.Path}`. Nested map types are not supported");

            int currentRecord = 0;
            int skippedRecords = 0;
            var keyDataColumn = await groupReader.ReadColumnAsync(keyField.DataField!, cancellationToken);
            var valueDataColumn = await groupReader.ReadColumnAsync(valueField.DataField!, cancellationToken);
            int totalRecords = keyDataColumn.RepetitionLevels.Count(p => p == 0);
            
            var entries = new MapValueCollection();
            int rowCount = Math.Max(keyDataColumn.Data.Length, valueDataColumn.Data.Length);
            var fieldIndex = dataTable.Columns[field.Path]!.Ordinal;
            for (int i = 0; i < rowCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (skipRecords > 0 && skippedRecords <= skipRecords)
                {
					        if (keyDataColumn.RepetitionLevels[i] == 0)
					        {
						        skippedRecords++;
					        }
                  if (skippedRecords <= skipRecords)
                  {
            			  continue;
                  }
                }

                if (isFirstColumn)
                {
                    dataTable.NewRow();
                }

                if (keyDataColumn.RepetitionLevels[i] == 0)
                {
                  if (currentRecord > 0) 
                  {
                    dataTable.Rows[currentRecord-1]![fieldIndex] = entries;
                    entries = new MapValueCollection();
                  }
                  if (currentRecord > readRecords || currentRecord == totalRecords)
                  {
                    break;
                  }
                  currentRecord++;
                }
                var key = keyDataColumn.Data.Length > i ? keyDataColumn.Data.GetValue(i) ?? DBNull.Value : DBNull.Value;
                var value = valueDataColumn.Data.Length > i ? valueDataColumn.Data.GetValue(i) ?? DBNull.Value : DBNull.Value;
				        entries.values.Add(new MapValue(key, keyField.DataField!.ClrType, value, valueField.DataField!.ClrType));

                progress?.Report(1);
            }
            // RepretitionLevels has no trailing 0 this is why we need to do one more step afterwards. 
			      dataTable.Rows[currentRecord - 1]![fieldIndex] = entries;
		}

		private async Task ReadStructField(DataTableLite dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex, ParquetSchemaElement field,
           long skipRecords, long readRecords, bool isFirstColumn, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            //Read struct data as a new datatable
            DataTableLite structFieldTable = BuildDataTable(field, field.Children.Select(f => f.Path).ToList(), 1);

            //Need to calculate progress differently for structs
            var structFieldReadProgress = StructReadProgress(progress, structFieldTable.Columns.Count);

            //Read the struct data and populate the datatable
            await ProcessRowGroup(structFieldTable, groupReader, skipRecords, readRecords, cancellationToken, structFieldReadProgress);

            var rowIndex = rowBeginIndex;
            var fieldIndex = dataTable.Columns[field.Path]?.Ordinal ?? throw new Exception($"Column `{field.Path}` is missing");
            var finalResultDataTable = structFieldTable.ToDataTable(cancellationToken);
            for (var i = 0; i < finalResultDataTable.Rows.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (isFirstColumn)
                {
                    dataTable.NewRow();
                }

                dataTable.Rows[rowIndex]![fieldIndex] = new StructValue(field.Path, finalResultDataTable.Rows[i]);
                rowIndex++;
            }
        }

        private SimpleProgress StructReadProgress(IProgress<int>? _progress, int fieldCount)
        {
            var progress = new SimpleProgress();
            progress.ProgressChanged += (int progressSoFar) =>
            {
                if (fieldCount > 0)
                {
                    //To report progress accurately we'll need to divide the progress total  
                    //by the field count to convert it to row count in the main data table.
                    var increment = progressSoFar % fieldCount;
                    if (increment == 0)
                        _progress?.Report(1);
                }
                else
                {
                    //If the struct field has no columns, then each read is one row.
                    _progress?.Report(1);
                }
            };
            return progress;
        }

        private DataTableLite BuildDataTable(ParquetSchemaElement? parent, List<string> fields, int expectedRecordCount)
        {
            parent ??= this.ParquetSchemaTree;
            DataTableLite dataTable = new(expectedRecordCount);
            foreach (var field in fields)
            {
                var schema = parent.GetChild(field);
                if (schema.SchemaElement.ConvertedType == ConvertedType.LIST)
                {
                    dataTable.AddColumn(field, typeof(ListValue), parent);
                }
                else if (schema.SchemaElement.ConvertedType == ConvertedType.MAP)
                {
                    dataTable.AddColumn(field, typeof(MapValueCollection), parent);
                }
                else if (this.FixMalformedDateTime
                    && schema.SchemaElement.LogicalType?.TIMESTAMP is not null
                    && schema.SchemaElement?.ConvertedType is null)
                {
                    //Fix for malformed datetime fields (#88)
                    dataTable.AddColumn(field, typeof(DateTime), parent);
                }
                else if (schema.SchemaElement.NumChildren > 0) //Struct
                {
                    dataTable.AddColumn(field, typeof(StructValue), parent);
                }
                else if (schema.SchemaElement.Type == Parquet.Meta.Type.BYTE_ARRAY
                    && schema.SchemaElement.LogicalType is null
                    && schema.SchemaElement.ConvertedType is null)
                {
                    dataTable.AddColumn(field, typeof(ByteArrayValue), parent);
                }
                else
                {
                    var clrType = schema.DataField?.ClrType ?? throw new Exception($"{(parent is not null ? parent + "/" : string.Empty)}/{field} has no data field");
                    dataTable.AddColumn(field, clrType, parent);
                }
            }
            return dataTable;
        }

        private object FixDateTime(object value, ParquetSchemaElement field)
        {
            if (!this.FixMalformedDateTime)
                return value;

            var timestampSchema = field.SchemaElement?.LogicalType?.TIMESTAMP;
            if (timestampSchema is not null && field.SchemaElement?.ConvertedType is null)
            {
                long castValue;
                if (field.DataField?.ClrType == typeof(long?))
                {
                    castValue = ((long?)value).Value; //We know this isn't null from the null check above
                }
                else if (field.DataField?.ClrType == typeof(long))
                {
                    castValue = (long)value;
                }
                else
                {
                    throw new UnsupportedFieldException($"Field {field.Path} is not a valid timestamp field");
                }

                int divideBy = 0;
                if (timestampSchema.Unit.NANOS != null)
                    divideBy = 1000 * 1000;
                else if (timestampSchema.Unit.MICROS != null)
                    divideBy = 1000;
                else if (timestampSchema.Unit.MILLIS != null)
                    divideBy = 1;

                if (divideBy > 0)
                    value = DateTimeOffset.FromUnixTimeMilliseconds(castValue / divideBy).DateTime;
                else //Not sure if this 'else' is correct but adding just in case
                    value = DateTimeOffset.FromUnixTimeSeconds(castValue).DateTime;
            }

            return value;
        }

        private class SimpleProgress : IProgress<int>
        {
            private int _progress = 0;
            public Action<int>? ProgressChanged;

            public void Report(int value)
            {
                _progress += value;
                ProgressChanged?.Invoke(_progress);
            }
        }
    }
}
