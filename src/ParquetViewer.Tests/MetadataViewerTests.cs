//These tests were written by AI (subject to change)

using ParquetViewer.Engine.ParquetNET.Types;
using ParquetViewer.Engine.Types;

namespace ParquetViewer.Tests;

/// <summary>
/// Expected thrift metadata for one leaf column of a row group, as surfaced through
/// <see cref="IRowGroupColumnMetadata"/>.
/// </summary>
/// <param name="PathInSchema">
/// Kept pre-split on purpose. Parquet.NET joins the thrift <c>path_in_schema</c> list with "/",
/// DuckDB's <c>parquet_metadata()</c> returns it pre-joined with ", ", so the two engines can
/// never share a single expected string here.
/// </param>
/// <param name="Min">The deprecated parquet <c>statistics.min</c> field.</param>
/// <param name="Max">The deprecated parquet <c>statistics.max</c> field.</param>
/// <param name="MinValue">The modern parquet <c>statistics.min_value</c> field.</param>
/// <param name="MaxValue">The modern parquet <c>statistics.max_value</c> field.</param>
public sealed record ExpectedColumn(
    int ColumnId,
    string[] PathInSchema,
    string Type,
    int NumValues,
    long TotalUncompressedSize,
    long TotalCompressedSize,
    long DataPageOffset,
    long? DictionaryPageOffset,
    long? NullCount,
    object? Min,
    object? Max,
    object? MinValue,
    object? MaxValue,
    bool? IsMinValueExact = null,
    bool? IsMaxValueExact = null);

public sealed record ExpectedRowGroup(
    int Ordinal,
    int RowCount,
    int ColumnCount,
    long FileOffset,
    long TotalByteSize,
    long TotalCompressedSize,
    IReadOnlyList<ExpectedColumn> Columns);

/// <summary>
/// One node of <see cref="IParquetMetadata.SchemaTree"/>, flattened into document order.
/// <see cref="Depth"/> is 0 for the root so that a node can be identified even when sibling
/// subtrees reuse the same names (every 2-tier list has a "list" child, every map a "key_value").
/// </summary>
public sealed record ExpectedSchemaNode(
    int Depth,
    string Path,
    FieldTypeId FieldType,
    RepetitionTypeId? RepetitionType,
    bool IsPrimitive,
    string? Type,
    string? ConvertedType,
    Type ClrType);

public sealed record ExpectedFile(
    int ParquetVersion,
    int RowCount,
    int RowGroupCount,
    string CreatedBy,
    IReadOnlyList<string> Fields,
    IReadOnlyList<string> CustomMetadataKeys,
    IReadOnlyList<ExpectedRowGroup> RowGroups,
    IReadOnlyList<ExpectedSchemaNode> SchemaNodes,
    bool SchemaNodesAreComplete = true);

/// <summary>
/// Locks down the thrift metadata that the MetadataViewer renders ("Thrift Metadata" tab) for a
/// spread of fixture files: list, struct, map, nested map, list-of-struct-of-list-of-struct,
/// TimeOnly/DateOnly logical types, raw BYTE_ARRAY, FIXED_LEN_BYTE_ARRAY and INT96.
/// <para>
/// The two engines do <b>not</b> agree on every field, so each concrete test class supplies a
/// complete expectation table of its own rather than a shared set of invariants. The known
/// divergences are all called out in comments on the tables:
/// <list type="bullet">
/// <item><description><c>PathInSchema</c> separator: "/" vs ", ".</description></item>
/// <item><description>
/// <see cref="IRowGroupMetadata.FileOffset"/>: Parquet.NET reports the thrift value (0 when the
/// writer omitted it), DuckDB reports -1 when NULL and otherwise a value that can differ from the
/// thrift one.
/// </description></item>
/// <item><description>
/// <see cref="IRowGroupMetadata.TotalCompressedSize"/>: Parquet.NET reports the thrift row-group
/// value (0 when omitted), DuckDB sums the per-column compressed sizes instead.
/// </description></item>
/// <item><description>
/// <see cref="IRowGroupColumnStatistics.Min"/>/<see cref="IRowGroupColumnStatistics.Max"/>:
/// Parquet.NET blanks the deprecated fields when they duplicate min_value/max_value, DuckDB
/// always reports them (as strings).
/// </description></item>
/// <item><description>
/// min_value/max_value: Parquet.NET deserializes to CLR types (<see cref="long"/>,
/// <see cref="DateOnly"/>, <see cref="double"/>, ...), DuckDB always returns
/// <see cref="string"/>. Parquet.NET has no branch for <see cref="TimeOnly"/>, so a TIME column
/// falls through to the raw thrift bytes.
/// </description></item>
/// <item><description>
/// <see cref="IParquetSchemaElement.ClrType"/> of a raw BYTE_ARRAY leaf:
/// <see cref="byte"/>[] vs <see cref="ByteArrayValue"/>.
/// </description></item>
/// <item><description>
/// The repeated wrapper of a 2-tier list is a <see cref="FieldTypeId.Struct"/> for Parquet.NET but
/// a <see cref="FieldTypeId.List"/> for DuckDB.
/// </description></item>
/// </list>
/// </para>
/// </summary>
public abstract class MetadataViewerTests
{
    private readonly bool _useDuckDBEngine;

    protected MetadataViewerTests(bool useDuckDBEngine)
    {
        _useDuckDBEngine = useDuckDBEngine;
    }

    /// <summary>How this engine flattens the thrift path_in_schema list into a single string.</summary>
    protected abstract string SchemaPathSeparator { get; }

    /// <summary>This engine's expectations, keyed by fixture path.</summary>
    protected abstract IReadOnlyDictionary<string, ExpectedFile> ExpectedMetadata { get; }

    #region Complex types

    [TestMethod]
    public Task LIST_TYPE_TEST1_METADATA() => AssertMetadataAsync("Data/LIST_TYPE_TEST1.parquet");

    [TestMethod]
    public Task STRUCT_TYPE_TEST_METADATA() => AssertMetadataAsync("Data/STRUCT_TYPE_TEST.parquet");

    [TestMethod]
    public Task MAP_TYPE_TEST1_METADATA() => AssertMetadataAsync("Data/MAP_TYPE_TEST1.parquet");

    [TestMethod]
    public Task NESTED_MAPS_TEST_METADATA() => AssertMetadataAsync("Data/NESTED_MAPS_TEST.parquet");

    [TestMethod]
    public Task LIST_OF_STRUCT_OF_LIST_OF_STRUCT_METADATA() => AssertMetadataAsync("Data/LIST_OF_STRUCT_OF_LIST_OF_STRUCT.parquet");

    [TestMethod]
    public Task EMPTY_LIST_OF_STRUCTS_METADATA() => AssertMetadataAsync("Data/EMPTY_LIST_OF_STRUCTS.parquet");

    #endregion

    #region Scalar and logical types

    [TestMethod]
    public Task TIME_ONLY_TYPE_PYARROW_V22_METADATA() => AssertMetadataAsync("Data/TIME_ONLY_TYPE_PYARROW_V22.parquet");

    [TestMethod]
    public Task BYTEARRAY_VALUE_TEST_METADATA() => AssertMetadataAsync("Data/BYTEARRAY_VALUE_TEST.parquet");

    [TestMethod]
    public Task NULLABLE_GUID_TEST_METADATA() => AssertMetadataAsync("Data/NULLABLE_GUID_TEST.parquet");

    [TestMethod]
    public Task PARQUET_MR_1_15_0_METADATA() => AssertMetadataAsync("Data/PARQUET-MR_1.15.0.parquet");

    [TestMethod]
    public Task RANDOM_TEST_FILE_METADATA() => AssertMetadataAsync("Data/RANDOM_TEST_FILE.parquet");

    #endregion

    private async Task AssertMetadataAsync(string path)
    {
        Assert.IsTrue(ExpectedMetadata.TryGetValue(path, out var expected),
            $"{GetType().Name} has no metadata expectations registered for '{path}'");

        IParquetEngine engine = _useDuckDBEngine
            ? await Engine.DuckDB.ParquetEngine.OpenFileOrFolderAsync(path)
            : await Engine.ParquetNET.ParquetEngine.OpenFileOrFolderAsync(path);

        await using (engine)
        {
            AssertFileLevel(engine, expected!);
            AssertRowGroups(engine.Metadata, expected!);
            AssertSchemaTree(engine.Metadata, expected!);
        }
    }

    private void AssertFileLevel(IParquetEngine engine, ExpectedFile expected)
    {
        var metadata = engine.Metadata;

        Assert.AreEqual(expected.ParquetVersion, metadata.ParquetVersion, "thrift version");
        Assert.AreEqual(expected.RowCount, metadata.RowCount, "metadata row count");
        Assert.AreEqual(expected.RowGroupCount, metadata.RowGroupCount, "row group count");
        Assert.AreEqual(expected.CreatedBy, metadata.CreatedBy, "created_by");

        //The engines derive the record count from their own reader, so this checks the metadata
        //and the reader agree with each other as well as with the file.
        Assert.AreEqual(expected.RowCount, engine.RecordCount, "engine record count");

        Assert.AreSequenceEqual(expected.Fields, engine.Fields, "unexpected top level fields");
        Assert.HasCount(expected.CustomMetadataKeys.Count, engine.CustomMetadata.Keys);
        foreach (var key in expected.CustomMetadataKeys)
        {
            Assert.Contains(key, engine.CustomMetadata.Keys);
        }
    }

    private void AssertRowGroups(IParquetMetadata metadata, ExpectedFile expected)
    {
        Assert.HasCount(expected.RowGroups.Count, metadata.RowGroups);

        var rowGroupIndex = -1;
        foreach (var actual in metadata.RowGroups)
        {
            rowGroupIndex++;
            var rowGroup = expected.RowGroups[rowGroupIndex];

            Assert.AreEqual(rowGroup.Ordinal, actual.Ordinal, $"row group {rowGroupIndex} ordinal");
            Assert.AreEqual(rowGroup.RowCount, actual.RowCount, $"row group {rowGroupIndex} row count");
            Assert.AreEqual(rowGroup.ColumnCount, actual.ColumnCount, $"row group {rowGroupIndex} column count");
            Assert.AreEqual(rowGroup.FileOffset, actual.FileOffset, $"row group {rowGroupIndex} file offset");
            Assert.AreEqual(rowGroup.TotalByteSize, actual.TotalByteSize, $"row group {rowGroupIndex} total byte size");
            Assert.AreEqual(rowGroup.TotalCompressedSize, actual.TotalCompressedSize, $"row group {rowGroupIndex} total compressed size");

            //None of the fixtures are sorted, and DuckDB cannot report sorting columns at all.
            Assert.IsNull(actual.SortingColumns, $"row group {rowGroupIndex} sorting columns");

            Assert.IsNotNull(actual.Columns, $"row group {rowGroupIndex} columns");
            Assert.HasCount(rowGroup.ColumnCount, actual.Columns!);

            foreach (var column in rowGroup.Columns)
            {
                AssertColumn(rowGroupIndex, actual.Columns!, column);
            }
        }
    }

    private void AssertColumn(int rowGroupIndex, ICollection<IRowGroupColumnMetadata> actualColumns, ExpectedColumn expected)
    {
        var actual = actualColumns.SingleOrDefault(c => c.ColumnId == expected.ColumnId);
        Assert.IsNotNull(actual, $"row group {rowGroupIndex} has no column with ColumnId {expected.ColumnId}");

        var label = $"column {expected.ColumnId} ({string.Join(SchemaPathSeparator, expected.PathInSchema)})";
        Assert.AreEqual(string.Join(SchemaPathSeparator, expected.PathInSchema), actual!.PathInSchema, $"{label} path_in_schema");
        Assert.AreEqual(expected.Type, actual.Type, $"{label} physical type");
        Assert.AreEqual(expected.NumValues, actual.NumValues, $"{label} num_values");
        Assert.AreEqual(expected.TotalUncompressedSize, actual.TotalUncompressedSize, $"{label} total_uncompressed_size");
        Assert.AreEqual(expected.TotalCompressedSize, actual.TotalCompressedSize, $"{label} total_compressed_size");
        Assert.AreEqual(expected.DataPageOffset, actual.DataPageOffset, $"{label} data_page_offset");
        Assert.AreEqual(expected.DictionaryPageOffset, actual.DictionaryPageOffset, $"{label} dictionary_page_offset");

        //No fixture in this suite was written with an index page or a bloom filter.
        Assert.IsNull(actual.IndexPageOffset, $"{label} index_page_offset");
        Assert.IsNull(actual.BloomFilterOffset, $"{label} bloom_filter_offset");
        Assert.IsNull(actual.BloomFilterLength, $"{label} bloom_filter_length");

        Assert.IsNotNull(actual.Statistics, $"{label} statistics");
        var statistics = actual.Statistics!;
        Assert.AreEqual(expected.NullCount, statistics.NullCount, $"{label} statistics.null_count");

        //No fixture in this suite carries distinct counts, so the field should stay unset.
        Assert.IsNull(statistics.DistinctCount, $"{label} statistics.distinct_count");

        AssertStatValue(expected.Min, statistics.Min, $"{label} statistics.min");
        AssertStatValue(expected.Max, statistics.Max, $"{label} statistics.max");
        AssertStatValue(expected.MinValue, statistics.MinValue, $"{label} statistics.min_value");
        AssertStatValue(expected.MaxValue, statistics.MaxValue, $"{label} statistics.max_value");
        Assert.AreEqual(expected.IsMinValueExact, statistics.IsMinValueExact, $"{label} statistics.is_min_value_exact");
        Assert.AreEqual(expected.IsMaxValueExact, statistics.IsMaxValueExact, $"{label} statistics.is_max_value_exact");
    }

    /// <summary>
    /// Statistics values are <see cref="object"/> because the CLR type they arrive as depends
    /// entirely on the engine (and, for Parquet.NET, on the schema node it could resolve).
    /// </summary>
    private static void AssertStatValue(object? expected, object? actual, string label)
    {
        if (expected is byte[] expectedBytes)
        {
            Assert.IsInstanceOfType<byte[]>(actual, $"{label}: expected raw bytes, got {Describe(actual)}");
            CollectionAssert.AreEqual(expectedBytes, (byte[])actual!, $"{label} bytes differ");
            return;
        }

        Assert.AreEqual(expected, actual, $"{label}: expected {Describe(expected)}, got {Describe(actual)}");
    }

    private static string Describe(object? value) => value switch
    {
        null => "null",
        byte[] b => $"byte[{b.Length}]:{Convert.ToHexString(b)}",
        _ => $"{value.GetType().Name}:{value}"
    };

    private static void AssertSchemaTree(IParquetMetadata metadata, ExpectedFile expected)
    {
        var actual = Flatten(metadata.SchemaTree).ToList();

        if (expected.SchemaNodesAreComplete)
        {
            Assert.AreEqual(expected.SchemaNodes.Count, actual.Count, "schema tree node count");
            for (var i = 0; i < expected.SchemaNodes.Count; i++)
            {
                AssertSchemaNode(expected.SchemaNodes[i], actual[i], $"schema node {i}");
            }
            return;
        }

        //Fixtures too wide to spell out in full: only assert the nodes that were listed.
        foreach (var node in expected.SchemaNodes)
        {
            var match = actual.FirstOrDefault(a => a.Depth == node.Depth && a.Path == node.Path);
            Assert.IsNotNull(match, $"schema tree has no '{node.Path}' node at depth {node.Depth}");
            AssertSchemaNode(node, match!, $"schema node '{node.Path}' at depth {node.Depth}");
        }
    }

    private static void AssertSchemaNode(ExpectedSchemaNode expected, FlatSchemaNode actual, string label)
    {
        Assert.AreEqual(expected.Path, actual.Path, $"{label} path");
        Assert.AreEqual(expected.FieldType, actual.FieldType, $"{label} field type");
        Assert.AreEqual(expected.RepetitionType, actual.RepetitionType, $"{label} repetition type");
        Assert.AreEqual(expected.IsPrimitive, actual.IsPrimitive, $"{label} is primitive");
        Assert.AreEqual(expected.Type, actual.Type, $"{label} physical type");
        Assert.AreEqual(expected.ConvertedType, actual.ConvertedType, $"{label} converted type");
        Assert.AreEqual(expected.ClrType, actual.ClrType, $"{label} clr type");
    }

    private sealed record FlatSchemaNode(
        int Depth,
        string Path,
        FieldTypeId FieldType,
        RepetitionTypeId? RepetitionType,
        bool IsPrimitive,
        string? Type,
        string? ConvertedType,
        Type ClrType);

    private static IEnumerable<FlatSchemaNode> Flatten(IParquetSchemaElement element, int depth = 0)
    {
        yield return new FlatSchemaNode(
            depth,
            element.Path,
            element.FieldType,
            element.RepetitionType,
            element.IsPrimitive,
            element.Type,
            element.ConvertedType,
            element.ClrType);

        foreach (var child in element.Children)
        {
            foreach (var node in Flatten(child, depth + 1))
            {
                yield return node;
            }
        }
    }
}

/// <summary>
/// Parquet.NET reads the thrift footer directly, so it sees exactly what the writer stored:
/// absent optional row-group fields become 0, statistics min/max are blanked when they duplicate
/// min_value/max_value, and statistics values are deserialized to real CLR types.
/// </summary>
[TestClass]
public class ParquetNETMetadataViewerTests : MetadataViewerTests
{
    public ParquetNETMetadataViewerTests() : base(useDuckDBEngine: false)
    {

    }

    protected override string SchemaPathSeparator => "/";

    protected override IReadOnlyDictionary<string, ExpectedFile> ExpectedMetadata { get; } =
        new Dictionary<string, ExpectedFile>
        {
            ["Data/LIST_TYPE_TEST1.parquet"] = new(
                ParquetVersion: 1,
                RowCount: 3,
                RowGroupCount: 1,
                CreatedBy: "parquet-cpp version 1.5.1-SNAPSHOT",
                Fields: ["int64_list", "utf8_list"],
                CustomMetadataKeys: ["pandas", "ARROW:schema"],
                RowGroups:
                [
                    //The writer (parquet-cpp 1.5.1) omitted file_offset and total_compressed_size,
                    //so both surface as 0 rather than DuckDB's -1 / column-size sum.
                    new ExpectedRowGroup(0, 3, 2, 0, 215, 0,
                    [
                        new(0, ["int64_list", "list", "item"], "INT64", 6, 124, 122, 46, 4, 1, null, null, 1L, 4L),
                        new(1, ["utf8_list", "list", "item"], "BYTE_ARRAY", 8, 90, 93, 263, 220, 1, null, null, "abc", "xyz"),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "schema", FieldTypeId.Struct, RepetitionTypeId.Required, false, null, null, typeof(StructValueExt)),
                    new(1, "int64_list", FieldTypeId.List, RepetitionTypeId.Optional, false, null, "LIST", typeof(ListValue)),
                    new(2, "list", FieldTypeId.Struct, RepetitionTypeId.Repeated, false, null, null, typeof(StructValueExt)),
                    new(3, "item", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "INT64", null, typeof(long)),
                    new(1, "utf8_list", FieldTypeId.List, RepetitionTypeId.Optional, false, null, "LIST", typeof(ListValue)),
                    new(2, "list", FieldTypeId.Struct, RepetitionTypeId.Repeated, false, null, null, typeof(StructValueExt)),
                    new(3, "item", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                ]),

            //45 leaf columns, so only a representative sample is spelled out. The column count is
            //still asserted, and the samples cover a plain string, an all-null map key, a
            //required int64, a required boolean and the final all-null boolean.
            ["Data/STRUCT_TYPE_TEST.parquet"] = new(
                ParquetVersion: 1,
                RowCount: 10,
                RowGroupCount: 1,
                CreatedBy: "parquet-mr version 1.10.1 (build a89df8f9932b6ef6633d06069e50c9b7970bebd1)",
                Fields: ["txn", "add", "remove", "metaData", "protocol", "commitInfo"],
                CustomMetadataKeys: ["org.apache.spark.sql.parquet.row.metadata"],
                RowGroups:
                [
                    new ExpectedRowGroup(0, 10, 45, 0, 3479, 0,
                    [
                        new(0, ["txn", "appId"], "BYTE_ARRAY", 10, 222, 224, 4, null, 9, null, null, "e4a20b59-dd0e-4c50-b074-e8ae4786df30", "e4a20b59-dd0e-4c50-b074-e8ae4786df30"),
                        new(4, ["add", "partitionValues", "key_value", "key"], "BYTE_ARRAY", 10, 36, 38, 737, null, 10, null, null, null, null),
                        new(6, ["add", "size"], "INT64", 10, 92, 92, 815, null, 7, null, null, 396L, 404L),
                        new(8, ["add", "dataChange"], "BOOLEAN", 10, 41, 43, 1011, null, 7, null, null, false, false),
                        new(44, ["commitInfo", "isBlindAppend"], "BOOLEAN", 10, 27, 29, 3449, null, 10, null, null, null, null),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "spark_schema", FieldTypeId.Struct, null, false, null, null, typeof(StructValueExt)),
                    new(1, "txn", FieldTypeId.Struct, RepetitionTypeId.Optional, false, null, null, typeof(StructValueExt)),
                    new(2, "version", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "INT64", null, typeof(long)),
                    new(1, "add", FieldTypeId.Struct, RepetitionTypeId.Optional, false, null, null, typeof(StructValueExt)),
                    new(2, "partitionValues", FieldTypeId.Map, RepetitionTypeId.Optional, false, null, "MAP", typeof(MapValue)),
                    new(1, "metaData", FieldTypeId.Struct, RepetitionTypeId.Optional, false, null, null, typeof(StructValueExt)),
                    new(2, "format", FieldTypeId.Struct, RepetitionTypeId.Optional, false, null, null, typeof(StructValueExt)),
                    //A map nested two structs deep, to prove the tree is walked all the way down.
                    new(3, "options", FieldTypeId.Map, RepetitionTypeId.Optional, false, null, "MAP", typeof(MapValue)),
                    new(1, "protocol", FieldTypeId.Struct, RepetitionTypeId.Optional, false, null, null, typeof(StructValueExt)),
                    new(2, "minReaderVersion", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "INT32", null, typeof(int)),
                    new(1, "commitInfo", FieldTypeId.Struct, RepetitionTypeId.Optional, false, null, null, typeof(StructValueExt)),
                    new(2, "operationParameters", FieldTypeId.Map, RepetitionTypeId.Optional, false, null, "MAP", typeof(MapValue)),
                ],
                SchemaNodesAreComplete: false),

            ["Data/MAP_TYPE_TEST1.parquet"] = new(
                ParquetVersion: 1,
                RowCount: 2,
                RowGroupCount: 1,
                CreatedBy: "parquet-cpp version 1.5.1-SNAPSHOT",
                Fields: ["col1", "col2"],
                CustomMetadataKeys: ["pandas", "ARROW:schema"],
                RowGroups:
                [
                    new ExpectedRowGroup(0, 2, 3, 96, 273, 273,
                    [
                        new(0, ["col1", "key_value", "key"], "BYTE_ARRAY", 4, 88, 92, 45, 4, 0, null, null, "id", "value2"),
                        new(1, ["col1", "key_value", "value"], "BYTE_ARRAY", 4, 113, 109, 228, 176, 0, null, null, "else", "something2"),
                        new(2, ["col2"], "BYTE_ARRAY", 2, 68, 72, 405, 375, 0, null, null, "bar", "foo"),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "schema", FieldTypeId.Struct, RepetitionTypeId.Required, false, null, null, typeof(StructValueExt)),
                    new(1, "col1", FieldTypeId.Map, RepetitionTypeId.Optional, false, null, "MAP", typeof(MapValue)),
                    new(2, "key_value", FieldTypeId.Struct, RepetitionTypeId.Repeated, false, null, null, typeof(StructValueExt)),
                    //Map keys are always required, map values are not.
                    new(3, "key", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                    new(3, "value", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                    new(1, "col2", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                ]),

            //The only fixture that writes the deprecated statistics min/max without also writing
            //min_value/max_value, so the deprecated fields are the only ones populated here.
            ["Data/NESTED_MAPS_TEST.parquet"] = new(
                ParquetVersion: 1,
                RowCount: 6,
                RowGroupCount: 1,
                CreatedBy: "parquet-mr version 1.8.2 (build c6522788629e590a53eb79874b95f6c3ff11f16c)",
                Fields: ["a", "b", "c"],
                CustomMetadataKeys: ["org.apache.spark.sql.parquet.row.metadata"],
                RowGroups:
                [
                    new ExpectedRowGroup(0, 6, 5, 0, 325, 0,
                    [
                        new(0, ["a", "key_value", "key"], "BYTE_ARRAY", 6, 70, 69, 4, null, 0, "a", "f", null, null),
                        new(1, ["a", "key_value", "value", "key_value", "key"], "INT32", 9, 91, 95, 73, null, 2, 1, 5, null, null),
                        new(2, ["a", "key_value", "value", "key_value", "value"], "BOOLEAN", 9, 48, 50, 168, null, 2, false, true, null, null),
                        new(3, ["b"], "INT32", 6, 52, 56, 218, null, 0, 1, 1, null, null),
                        new(4, ["c"], "DOUBLE", 6, 64, 68, 274, null, 0, 1d, 1d, null, null),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "spark_schema", FieldTypeId.Struct, null, false, null, null, typeof(StructValueExt)),
                    new(1, "a", FieldTypeId.Map, RepetitionTypeId.Optional, false, null, "MAP", typeof(MapValue)),
                    new(2, "key_value", FieldTypeId.Struct, RepetitionTypeId.Repeated, false, null, null, typeof(StructValueExt)),
                    new(3, "key", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                    new(3, "value", FieldTypeId.Map, RepetitionTypeId.Optional, false, null, "MAP", typeof(MapValue)),
                    new(4, "key_value", FieldTypeId.Struct, RepetitionTypeId.Repeated, false, null, null, typeof(StructValueExt)),
                    new(5, "key", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "INT32", null, typeof(int)),
                    new(5, "value", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "BOOLEAN", null, typeof(bool)),
                    new(1, "b", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "INT32", null, typeof(int)),
                    new(1, "c", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "DOUBLE", null, typeof(double)),
                ]),

            ["Data/LIST_OF_STRUCT_OF_LIST_OF_STRUCT.parquet"] = new(
                ParquetVersion: 1,
                RowCount: 1,
                RowGroupCount: 1,
                CreatedBy: "Polars",
                Fields: ["aws_scores"],
                CustomMetadataKeys: ["ARROW:schema"],
                RowGroups:
                [
                    new ExpectedRowGroup(0, 1, 3, 4, 1546, 557,
                    [
                        new(0, ["aws_scores", "list", "element", "Labels", "list", "element", "Name"], "BYTE_ARRAY", 56, 909, 176, 4, null, 0, null, null, "GRAPHIC", "VIOLENCE_OR_THREAT"),
                        new(1, ["aws_scores", "list", "element", "Labels", "list", "element", "Score"], "DOUBLE", 56, 516, 267, 283, null, 0, null, null, 0.01860000006854534d, 0.18400000035762787d),
                        new(2, ["aws_scores", "list", "element", "Toxicity"], "DOUBLE", 8, 121, 114, 646, null, 0, null, null, 0.07419999688863754d, 0.10809999704360962d),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "root", FieldTypeId.Struct, null, false, null, null, typeof(StructValueExt)),
                    new(1, "aws_scores", FieldTypeId.List, RepetitionTypeId.Optional, false, null, "LIST", typeof(ListValue)),
                    new(2, "list", FieldTypeId.Struct, RepetitionTypeId.Repeated, false, null, null, typeof(StructValueExt)),
                    new(3, "element", FieldTypeId.Struct, RepetitionTypeId.Optional, false, null, null, typeof(StructValueExt)),
                    new(4, "Labels", FieldTypeId.List, RepetitionTypeId.Optional, false, null, "LIST", typeof(ListValue)),
                    new(5, "list", FieldTypeId.Struct, RepetitionTypeId.Repeated, false, null, null, typeof(StructValueExt)),
                    new(6, "element", FieldTypeId.Struct, RepetitionTypeId.Optional, false, null, null, typeof(StructValueExt)),
                    new(7, "Name", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                    new(7, "Score", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "DOUBLE", null, typeof(double)),
                    new(4, "Toxicity", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "DOUBLE", null, typeof(double)),
                ]),

            //Every column in this file is entirely null, so there are no statistics values to
            //deserialise and NullCount equals the row count.
            ["Data/EMPTY_LIST_OF_STRUCTS.parquet"] = new(
                ParquetVersion: 1,
                RowCount: 2,
                RowGroupCount: 1,
                CreatedBy: "parquet-mr version 1.13.1 (build db4183109d5b734ec5930d870cdae161e408ddba)",
                Fields: ["Product", "Orders"],
                CustomMetadataKeys: ["org.apache.spark.version", "org.apache.spark.sql.parquet.row.metadata"],
                RowGroups:
                [
                    new ExpectedRowGroup(0, 2, 3, 4, 125, 131,
                    [
                        new(0, ["Product"], "BYTE_ARRAY", 2, 53, 55, 4, null, 0, null, null, "Product1", "Product2"),
                        new(1, ["Orders", "list", "element", "DateTime"], "INT96", 2, 36, 38, 59, null, 2, null, null, null, null),
                        new(2, ["Orders", "list", "element", "Quantity"], "DOUBLE", 2, 36, 38, 97, null, 2, null, null, null, null),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "spark_schema", FieldTypeId.Struct, null, false, null, null, typeof(StructValueExt)),
                    new(1, "Product", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                    new(1, "Orders", FieldTypeId.List, RepetitionTypeId.Optional, false, null, "LIST", typeof(ListValue)),
                    new(2, "list", FieldTypeId.Struct, RepetitionTypeId.Repeated, false, null, null, typeof(StructValueExt)),
                    new(3, "element", FieldTypeId.Struct, RepetitionTypeId.Required, false, null, null, typeof(StructValueExt)),
                    new(4, "DateTime", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "INT96", null, typeof(DateTime)),
                    new(4, "Quantity", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "DOUBLE", null, typeof(double)),
                ]),

            //The only fixture in the suite written with thrift version 2, and the only one that
            //sets is_min_value_exact/is_max_value_exact.
            ["Data/TIME_ONLY_TYPE_PYARROW_V22.parquet"] = new(
                ParquetVersion: 2,
                RowCount: 4626,
                RowGroupCount: 1,
                CreatedBy: "parquet-cpp-arrow version 22.0.0",
                Fields: ["DATE", "TIME"],
                CustomMetadataKeys: ["pandas", "ARROW:schema"],
                RowGroups:
                [
                    new ExpectedRowGroup(0, 4626, 2, 4, 44699, 18740,
                    [
                        new(0, ["DATE"], "INT32", 4626, 77, 85, 26, 4, 0, null, null, new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 1), true, true),
                        //TryDeserializeValue has no TimeOnly branch, so the TIME statistics fall
                        //through to the raw little endian int64 microseconds. DuckDB renders these
                        //as "05:00:00" and "07:16:38" instead.
                        new(1, ["TIME"], "INT64", 4626, 44622, 18655, 11571, 89, 0, null, null,
                            new byte[] { 0x00, 0x34, 0xE2, 0x30, 0x04, 0x00, 0x00, 0x00 },
                            new byte[] { 0x80, 0xC1, 0x85, 0x19, 0x06, 0x00, 0x00, 0x00 }, true, true),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "schema", FieldTypeId.Struct, RepetitionTypeId.Required, false, null, null, typeof(StructValueExt)),
                    new(1, "DATE", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "INT32", "DATE", typeof(DateOnly)),
                    //No converted type, the TIME annotation lives purely in the logical type.
                    new(1, "TIME", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "INT64", null, typeof(TimeOnly)),
                ]),

            //A BYTE_ARRAY with no UTF8 converted type, so the CLR type is a raw byte array.
            ["Data/BYTEARRAY_VALUE_TEST.parquet"] = new(
                ParquetVersion: 1,
                RowCount: 1,
                RowGroupCount: 1,
                CreatedBy: "Parquet.Net version 5.4.0 (build d1913912b8b649c2abeaa8b7813bf283a7f0c290)",
                Fields: ["mesh"],
                CustomMetadataKeys: ["ParquetViewer"],
                RowGroups:
                [
                    new ExpectedRowGroup(0, 1, 1, 0, 116, 52,
                    [
                        new(0, ["mesh"], "BYTE_ARRAY", 1, 116, 52, 4, null, 0, null, null, null, null),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "root", FieldTypeId.Struct, null, false, null, null, typeof(StructValueExt)),
                    new(1, "mesh", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", null, typeof(byte[])),
                ]),

            //33 flat columns, only a sample is spelled out. create_date is an INT96 with no
            //converted type: Parquet.NET's DateTime branch finds no logical time unit and returns
            //the raw tick count, while DuckDB renders a date.
            ["Data/NULLABLE_GUID_TEST.parquet"] = new(
                ParquetVersion: 1,
                RowCount: 1,
                RowGroupCount: 1,
                CreatedBy: "Parquet.Net version 4.11.2 (build 57fcfdb89064e71d7bafe89a6e04a1eec2668ca2)",
                Fields:
                [
                    "site_section_id", "event_id", "user_cookie", "device_cookie", "create_date",
                    "event_type_id", "time", "ip_address", "referent_event_id", "browser_type_id",
                    "panel_user_id", "panel_household_id", "external_device_id", "external_device_source_id",
                    "url", "referrer_url", "browser", "video_url", "video_width", "video_height",
                    "video_duration", "video_time", "is_live_stream", "video_title", "browser_id",
                    "operating_system_id", "event_chain_id", "consent", "date_user", "location",
                    "city", "sf2_consent", "door_url",
                ],
                CustomMetadataKeys: [],
                RowGroups:
                [
                    new ExpectedRowGroup(0, 1, 33, 0, 2753, 0,
                    [
                        new(0, ["site_section_id"], "INT32", 1, 49, 51, 4, null, 0, null, null, 842, 842),
                        //FIXED_LEN_BYTE_ARRAY(16) without a UUID logical type on this column, so the
                        //GUID statistics are never written and MinValue stays null.
                        new(1, ["event_id"], "FIXED_LEN_BYTE_ARRAY", 1, 37, 39, 55, null, 0, null, null, null, null),
                        new(4, ["create_date"], "INT96", 1, 89, 91, 172, null, 0, null, null, 0L, 0L),
                        new(8, ["referent_event_id"], "FIXED_LEN_BYTE_ARRAY", 1, 43, 45, 467, null, 0, null, null, null, null),
                        new(22, ["is_live_stream"], "BOOLEAN", 1, 28, 30, 1775, null, 0, null, null, null, null),
                        new(32, ["door_url"], "BYTE_ARRAY", 1, 119, 121, 2636, null, 0, null, null, "www.somedoor.url", "www.somedoor.url"),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "root", FieldTypeId.Struct, null, false, null, null, typeof(StructValueExt)),
                    new(1, "site_section_id", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "INT32", "INT_32", typeof(int)),
                    new(1, "event_id", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "FIXED_LEN_BYTE_ARRAY", null, typeof(Guid)),
                    new(1, "create_date", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "INT96", null, typeof(DateTime)),
                    new(1, "ip_address", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                    new(1, "is_live_stream", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BOOLEAN", null, typeof(bool)),
                ],
                SchemaNodesAreComplete: false),

            //A second parquet-mr version, so the DATE and TIMESTAMP_MICROS logical type handling
            //can be compared against the 1.10.1 file above.
            ["Data/PARQUET-MR_1.15.0.parquet"] = new(
                ParquetVersion: 1,
                RowCount: 5,
                RowGroupCount: 1,
                CreatedBy: "parquet-mr version 1.15.0 (build 4665401d36e468c988322f78621da7c2d1c22ede)",
                Fields: ["id", "name", "age", "height", "isStudent", "enrollmentDate", "lastLogin"],
                CustomMetadataKeys: ["org.apache.spark.version", "org.apache.spark.sql.parquet.row.metadata"],
                RowGroups:
                [
                    new ExpectedRowGroup(0, 5, 7, 4, 395, 407,
                    [
                        new(0, ["id"], "INT32", 5, 43, 45, 4, null, 0, null, null, 1, 5),
                        new(1, ["name"], "BYTE_ARRAY", 5, 101, 102, 49, null, 0, null, null, "David Lee", "Mike Johnson"),
                        new(2, ["age"], "INT32", 5, 43, 45, 151, null, 0, null, null, 28, 35),
                        new(3, ["height"], "DOUBLE", 5, 63, 60, 196, null, 0, null, null, 165.7d, 185.1d),
                        new(4, ["isStudent"], "BOOLEAN", 5, 24, 26, 256, null, 0, null, null, false, true),
                        new(5, ["enrollmentDate"], "INT32", 5, 63, 67, 314, 282, 0, null, null, new DateOnly(2023, 1, 1), new DateOnly(2024, 12, 23)),
                        //TIMESTAMP_MICROS, so the statistics become a UTC DateTime rather than a
                        //raw tick count.
                        new(6, ["lastLogin"], "INT64", 5, 58, 62, 378, 349, 0, null, null,
                            new DateTime(2024, 12, 23, 22, 8, 39, 138, DateTimeKind.Utc),
                            new DateTime(2024, 12, 23, 22, 8, 39, 138, DateTimeKind.Utc)),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "spark_schema", FieldTypeId.Struct, null, false, null, null, typeof(StructValueExt)),
                    new(1, "id", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "INT32", null, typeof(int)),
                    new(1, "name", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                    new(1, "age", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "INT32", null, typeof(int)),
                    new(1, "height", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "DOUBLE", null, typeof(double)),
                    new(1, "isStudent", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "BOOLEAN", null, typeof(bool)),
                    new(1, "enrollmentDate", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "INT32", "DATE", typeof(DateOnly)),
                    new(1, "lastLogin", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "INT64", "TIMESTAMP_MICROS", typeof(DateTime)),
                ]),

            //A wide, flat, thrift version 2 file. Columns 8/14/etc are declared DOUBLE but are all
            //null, which is why they carry no statistics values at all.
            ["Data/RANDOM_TEST_FILE.parquet"] = new(
                ParquetVersion: 2,
                RowCount: 5,
                RowGroupCount: 1,
                CreatedBy: "parquet-cpp-arrow version 9.0.0",
                Fields:
                [
                    "Transmission_Number", "Line_Number", "DC", "GOLD_Article", "GOLD_Storage_Loc",
                    "GOLD_Logistic_Variant", "SAP_Article", "Delivery_Quantity_Base_Unit", "Base_UOM",
                    "Delivery_Quantity_Preparation_Unit", "Preparation_UOM", "Client", "Shipping_Date",
                    "GOLD_Shipping_Id", "Line_Id", "SAP_OBD_Number", "Article_Managed_by_Unit",
                    "Shipped_Net_Weight", "Missing_Quantity", "Missing_Weight", "Pallet_Number",
                    "Mother_Pallet", "Daughter_Pallet", "Code_Picker", "Code_Loader", "Route",
                    "Tour_Rank", "Promo_Number", "Full_Pallet", "Depot_Origine_Transfer",
                    "Depot_Preparation", "Order_Date", "Empty_Included", "Empty_Article_Number_Pallet",
                    "Empty_Article_Number_CV", "Crate_Number", "Warehouse_number", "Child_OBD_Number",
                    "Missing_Motivation", "Promo_Week", "Delivery_Type", "Source_Name",
                ],
                CustomMetadataKeys: ["pandas", "ARROW:schema"],
                RowGroups:
                [
                    new ExpectedRowGroup(0, 5, 42, 4, 3237, 3354,
                    [
                        new(0, ["Transmission_Number"], "INT64", 5, 100, 104, 36, 4, 0, null, null, 798921L, 799315L),
                        //All five rows are null, so the writer emitted no min/max and NullCount is
                        //the full row count.
                        new(8, ["Base_UOM"], "DOUBLE", 5, 42, 45, 1517, 1502, 5, null, null, null, null),
                        new(12, ["Shipping_Date"], "BYTE_ARRAY", 5, 82, 86, 2121, 2091, 1, null, null, "2022-08-12", "2022-08-12"),
                        new(20, ["Pallet_Number"], "DOUBLE", 5, 109, 113, 3471, 3431, 1, null, null, 54001131035764410d, 54001133003406770d),
                        new(41, ["Source_Name"], "BYTE_ARRAY", 5, 226, 203, 6670, 6587, 0, null, null, "DLIx12_SHIPCONF_BW15_20220811015519979.DWL", "DLIx12_SHIPCONF_BW15_20220812020138531.DWL"),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "schema", FieldTypeId.Struct, RepetitionTypeId.Required, false, null, null, typeof(StructValueExt)),
                    new(1, "Transmission_Number", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "INT64", null, typeof(long)),
                    new(1, "DC", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                    new(1, "Base_UOM", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "DOUBLE", null, typeof(double)),
                    new(1, "Source_Name", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                ],
                SchemaNodesAreComplete: false),
        };
}

/// <summary>
/// DuckDB goes through <c>parquet_metadata()</c> / <c>parquet_file_metadata()</c> instead of the
/// thrift footer, so it stringifies every statistics value, reports -1 for a NULL file offset and
/// rebuilds the row group compressed size by summing its columns. Those differences are pinned
/// per column below rather than smoothed over.
/// </summary>
[TestClass]
public class DuckDBMetadataViewerTests : MetadataViewerTests
{
    public DuckDBMetadataViewerTests() : base(useDuckDBEngine: true)
    {

    }

    protected override string SchemaPathSeparator => ", ";

    protected override IReadOnlyDictionary<string, ExpectedFile> ExpectedMetadata { get; } =
        new Dictionary<string, ExpectedFile>
        {
            ["Data/LIST_TYPE_TEST1.parquet"] = new(
                ParquetVersion: 1,
                RowCount: 3,
                RowGroupCount: 1,
                CreatedBy: "parquet-cpp version 1.5.1-SNAPSHOT",
                Fields: ["int64_list", "utf8_list"],
                CustomMetadataKeys: ["pandas", "ARROW:schema"],
                RowGroups:
                [
                    //file_offset is NULL in parquet_metadata() so it becomes -1, and
                    //TotalCompressedSize is 122 + 93 rather than the absent thrift value of 0.
                    new ExpectedRowGroup(0, 3, 2, -1, 215, 215,
                    [
                        //DuckDB keeps the deprecated min/max even though they duplicate
                        //min_value/max_value, and stringifies the numbers.
                        new(0, ["int64_list", "list", "item"], "INT64", 6, 124, 122, 46, 4, 1, "1", "4", "1", "4"),
                        new(1, ["utf8_list", "list", "item"], "BYTE_ARRAY", 8, 90, 93, 263, 220, 1, null, null, "abc", "xyz"),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "schema", FieldTypeId.Struct, RepetitionTypeId.Required, false, null, null, typeof(StructValue)),
                    new(1, "int64_list", FieldTypeId.List, RepetitionTypeId.Optional, false, null, "LIST", typeof(ListValue)),
                    //DuckDB reports the repeated wrapper of a 2-tier list as a List, not a Struct.
                    new(2, "list", FieldTypeId.List, RepetitionTypeId.Repeated, false, null, null, typeof(ListValue)),
                    new(3, "item", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "INT64", null, typeof(long)),
                    new(1, "utf8_list", FieldTypeId.List, RepetitionTypeId.Optional, false, null, "LIST", typeof(ListValue)),
                    new(2, "list", FieldTypeId.List, RepetitionTypeId.Repeated, false, null, null, typeof(ListValue)),
                    new(3, "item", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                ]),

            ["Data/STRUCT_TYPE_TEST.parquet"] = new(
                ParquetVersion: 1,
                RowCount: 10,
                RowGroupCount: 1,
                CreatedBy: "parquet-mr version 1.10.1 (build a89df8f9932b6ef6633d06069e50c9b7970bebd1)",
                Fields: ["txn", "add", "remove", "metaData", "protocol", "commitInfo"],
                CustomMetadataKeys: ["org.apache.spark.sql.parquet.row.metadata"],
                RowGroups:
                [
                    new ExpectedRowGroup(0, 10, 45, -1, 3479, 3474,
                    [
                        new(0, ["txn", "appId"], "BYTE_ARRAY", 10, 222, 224, 4, null, 9, "e4a20b59-dd0e-4c50-b074-e8ae4786df30", "e4a20b59-dd0e-4c50-b074-e8ae4786df30", "e4a20b59-dd0e-4c50-b074-e8ae4786df30", "e4a20b59-dd0e-4c50-b074-e8ae4786df30"),
                        new(4, ["add", "partitionValues", "key_value", "key"], "BYTE_ARRAY", 10, 36, 38, 737, null, 10, null, null, null, null),
                        new(6, ["add", "size"], "INT64", 10, 92, 92, 815, null, 7, "396", "404", "396", "404"),
                        new(8, ["add", "dataChange"], "BOOLEAN", 10, 41, 43, 1011, null, 7, "false", "false", "false", "false"),
                        new(44, ["commitInfo", "isBlindAppend"], "BOOLEAN", 10, 27, 29, 3449, null, 10, null, null, null, null),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "spark_schema", FieldTypeId.Struct, null, false, null, null, typeof(StructValue)),
                    new(1, "txn", FieldTypeId.Struct, RepetitionTypeId.Optional, false, null, null, typeof(StructValue)),
                    new(2, "version", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "INT64", null, typeof(long)),
                    new(1, "add", FieldTypeId.Struct, RepetitionTypeId.Optional, false, null, null, typeof(StructValue)),
                    new(2, "partitionValues", FieldTypeId.Map, RepetitionTypeId.Optional, false, null, "MAP", typeof(MapValue)),
                    new(1, "metaData", FieldTypeId.Struct, RepetitionTypeId.Optional, false, null, null, typeof(StructValue)),
                    new(2, "format", FieldTypeId.Struct, RepetitionTypeId.Optional, false, null, null, typeof(StructValue)),
                    new(3, "options", FieldTypeId.Map, RepetitionTypeId.Optional, false, null, "MAP", typeof(MapValue)),
                    new(1, "protocol", FieldTypeId.Struct, RepetitionTypeId.Optional, false, null, null, typeof(StructValue)),
                    new(2, "minReaderVersion", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "INT32", null, typeof(int)),
                    new(1, "commitInfo", FieldTypeId.Struct, RepetitionTypeId.Optional, false, null, null, typeof(StructValue)),
                    new(2, "operationParameters", FieldTypeId.Map, RepetitionTypeId.Optional, false, null, "MAP", typeof(MapValue)),
                ],
                SchemaNodesAreComplete: false),

            ["Data/MAP_TYPE_TEST1.parquet"] = new(
                ParquetVersion: 1,
                RowCount: 2,
                RowGroupCount: 1,
                CreatedBy: "parquet-cpp version 1.5.1-SNAPSHOT",
                Fields: ["col1", "col2"],
                CustomMetadataKeys: ["pandas", "ARROW:schema"],
                RowGroups:
                [
                    new ExpectedRowGroup(0, 2, 3, 96, 273, 273,
                    [
                        new(0, ["col1", "key_value", "key"], "BYTE_ARRAY", 4, 88, 92, 45, 4, 0, null, null, "id", "value2"),
                        new(1, ["col1", "key_value", "value"], "BYTE_ARRAY", 4, 113, 109, 228, 176, 0, null, null, "else", "something2"),
                        new(2, ["col2"], "BYTE_ARRAY", 2, 68, 72, 405, 375, 0, null, null, "bar", "foo"),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "schema", FieldTypeId.Struct, RepetitionTypeId.Required, false, null, null, typeof(StructValue)),
                    new(1, "col1", FieldTypeId.Map, RepetitionTypeId.Optional, false, null, "MAP", typeof(MapValue)),
                    //Maps are unaffected: key_value is a Struct in both engines.
                    new(2, "key_value", FieldTypeId.Struct, RepetitionTypeId.Repeated, false, null, null, typeof(StructValue)),
                    new(3, "key", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                    new(3, "value", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                    new(1, "col2", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                ]),

            ["Data/NESTED_MAPS_TEST.parquet"] = new(
                ParquetVersion: 1,
                RowCount: 6,
                RowGroupCount: 1,
                CreatedBy: "parquet-mr version 1.8.2 (build c6522788629e590a53eb79874b95f6c3ff11f16c)",
                Fields: ["a", "b", "c"],
                CustomMetadataKeys: ["org.apache.spark.sql.parquet.row.metadata"],
                RowGroups:
                [
                    new ExpectedRowGroup(0, 6, 5, -1, 325, 338,
                    [
                        new(0, ["a", "key_value", "key"], "BYTE_ARRAY", 6, 70, 69, 4, null, 0, "a", "f", null, null),
                        new(1, ["a", "key_value", "value", "key_value", "key"], "INT32", 9, 91, 95, 73, null, 2, "1", "5", null, null),
                        new(2, ["a", "key_value", "value", "key_value", "value"], "BOOLEAN", 9, 48, 50, 168, null, 2, "false", "true", null, null),
                        new(3, ["b"], "INT32", 6, 52, 56, 218, null, 0, "1", "1", null, null),
                        new(4, ["c"], "DOUBLE", 6, 64, 68, 274, null, 0, "1.0", "1.0", null, null),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "spark_schema", FieldTypeId.Struct, null, false, null, null, typeof(StructValue)),
                    new(1, "a", FieldTypeId.Map, RepetitionTypeId.Optional, false, null, "MAP", typeof(MapValue)),
                    new(2, "key_value", FieldTypeId.Struct, RepetitionTypeId.Repeated, false, null, null, typeof(StructValue)),
                    new(3, "key", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                    new(3, "value", FieldTypeId.Map, RepetitionTypeId.Optional, false, null, "MAP", typeof(MapValue)),
                    new(4, "key_value", FieldTypeId.Struct, RepetitionTypeId.Repeated, false, null, null, typeof(StructValue)),
                    new(5, "key", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "INT32", null, typeof(int)),
                    new(5, "value", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "BOOLEAN", null, typeof(bool)),
                    new(1, "b", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "INT32", null, typeof(int)),
                    new(1, "c", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "DOUBLE", null, typeof(double)),
                ]),

            ["Data/LIST_OF_STRUCT_OF_LIST_OF_STRUCT.parquet"] = new(
                ParquetVersion: 1,
                RowCount: 1,
                RowGroupCount: 1,
                CreatedBy: "Polars",
                Fields: ["aws_scores"],
                CustomMetadataKeys: ["ARROW:schema"],
                RowGroups:
                [
                    //DuckDB reports a real file offset here (180) even though the thrift
                    //RowGroup.file_offset is 4.
                    new ExpectedRowGroup(0, 1, 3, 180, 1546, 557,
                    [
                        new(0, ["aws_scores", "list", "element", "Labels", "list", "element", "Name"], "BYTE_ARRAY", 56, 909, 176, 4, null, 0, null, null, "GRAPHIC", "VIOLENCE_OR_THREAT"),
                        new(1, ["aws_scores", "list", "element", "Labels", "list", "element", "Score"], "DOUBLE", 56, 516, 267, 283, null, 0, null, null, "0.01860000006854534", "0.18400000035762787"),
                        new(2, ["aws_scores", "list", "element", "Toxicity"], "DOUBLE", 8, 121, 114, 646, null, 0, null, null, "0.07419999688863754", "0.10809999704360962"),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "root", FieldTypeId.Struct, null, false, null, null, typeof(StructValue)),
                    new(1, "aws_scores", FieldTypeId.List, RepetitionTypeId.Optional, false, null, "LIST", typeof(ListValue)),
                    new(2, "list", FieldTypeId.List, RepetitionTypeId.Repeated, false, null, null, typeof(ListValue)),
                    new(3, "element", FieldTypeId.Struct, RepetitionTypeId.Optional, false, null, null, typeof(StructValue)),
                    new(4, "Labels", FieldTypeId.List, RepetitionTypeId.Optional, false, null, "LIST", typeof(ListValue)),
                    new(5, "list", FieldTypeId.List, RepetitionTypeId.Repeated, false, null, null, typeof(ListValue)),
                    new(6, "element", FieldTypeId.Struct, RepetitionTypeId.Optional, false, null, null, typeof(StructValue)),
                    new(7, "Name", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                    new(7, "Score", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "DOUBLE", null, typeof(double)),
                    new(4, "Toxicity", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "DOUBLE", null, typeof(double)),
                ]),

            ["Data/EMPTY_LIST_OF_STRUCTS.parquet"] = new(
                ParquetVersion: 1,
                RowCount: 2,
                RowGroupCount: 1,
                CreatedBy: "parquet-mr version 1.13.1 (build db4183109d5b734ec5930d870cdae161e408ddba)",
                Fields: ["Product", "Orders"],
                CustomMetadataKeys: ["org.apache.spark.version", "org.apache.spark.sql.parquet.row.metadata"],
                RowGroups:
                [
                    new ExpectedRowGroup(0, 2, 3, 4, 125, 131,
                    [
                        new(0, ["Product"], "BYTE_ARRAY", 2, 53, 55, 4, null, 0, null, null, "Product1", "Product2"),
                        new(1, ["Orders", "list", "element", "DateTime"], "INT96", 2, 36, 38, 59, null, 2, null, null, null, null),
                        new(2, ["Orders", "list", "element", "Quantity"], "DOUBLE", 2, 36, 38, 97, null, 2, null, null, null, null),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "spark_schema", FieldTypeId.Struct, null, false, null, null, typeof(StructValue)),
                    new(1, "Product", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                    new(1, "Orders", FieldTypeId.List, RepetitionTypeId.Optional, false, null, "LIST", typeof(ListValue)),
                    new(2, "list", FieldTypeId.List, RepetitionTypeId.Repeated, false, null, null, typeof(ListValue)),
                    new(3, "element", FieldTypeId.Struct, RepetitionTypeId.Required, false, null, null, typeof(StructValue)),
                    new(4, "DateTime", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "INT96", null, typeof(DateTime)),
                    new(4, "Quantity", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "DOUBLE", null, typeof(double)),
                ]),

            ["Data/TIME_ONLY_TYPE_PYARROW_V22.parquet"] = new(
                ParquetVersion: 2,
                RowCount: 4626,
                RowGroupCount: 1,
                CreatedBy: "parquet-cpp-arrow version 22.0.0",
                Fields: ["DATE", "TIME"],
                CustomMetadataKeys: ["pandas", "ARROW:schema"],
                RowGroups:
                [
                    new ExpectedRowGroup(0, 4626, 2, 0, 44699, 18740,
                    [
                        new(0, ["DATE"], "INT32", 4626, 77, 85, 26, 4, 0, "2024-01-01", "2024-01-01", "2024-01-01", "2024-01-01", true, true),
                        //DuckDB understands the TIME(MICROS) logical type and renders a clock time,
                        //where Parquet.NET returns the raw int64 bytes.
                        new(1, ["TIME"], "INT64", 4626, 44622, 18655, 11571, 89, 0, "05:00:00", "07:16:38", "05:00:00", "07:16:38", true, true),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "schema", FieldTypeId.Struct, RepetitionTypeId.Required, false, null, null, typeof(StructValue)),
                    new(1, "DATE", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "INT32", "DATE", typeof(DateOnly)),
                    new(1, "TIME", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "INT64", null, typeof(TimeOnly)),
                ]),

            ["Data/BYTEARRAY_VALUE_TEST.parquet"] = new(
                ParquetVersion: 1,
                RowCount: 1,
                RowGroupCount: 1,
                CreatedBy: "Parquet.Net version 5.4.0 (build d1913912b8b649c2abeaa8b7813bf283a7f0c290)",
                Fields: ["mesh"],
                CustomMetadataKeys: ["ParquetViewer"],
                RowGroups:
                [
                    new ExpectedRowGroup(0, 1, 1, -1, 116, 52,
                    [
                        new(0, ["mesh"], "BYTE_ARRAY", 1, 116, 52, 4, null, 0, null, null, null, null),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "root", FieldTypeId.Struct, null, false, null, null, typeof(StructValue)),
                    //DuckDB wraps an unconverted BYTE_ARRAY in its own display type.
                    new(1, "mesh", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", null, typeof(ByteArrayValue)),
                ]),

            ["Data/NULLABLE_GUID_TEST.parquet"] = new(
                ParquetVersion: 1,
                RowCount: 1,
                RowGroupCount: 1,
                CreatedBy: "Parquet.Net version 4.11.2 (build 57fcfdb89064e71d7bafe89a6e04a1eec2668ca2)",
                Fields:
                [
                    "site_section_id", "event_id", "user_cookie", "device_cookie", "create_date",
                    "event_type_id", "time", "ip_address", "referent_event_id", "browser_type_id",
                    "panel_user_id", "panel_household_id", "external_device_id", "external_device_source_id",
                    "url", "referrer_url", "browser", "video_url", "video_width", "video_height",
                    "video_duration", "video_time", "is_live_stream", "video_title", "browser_id",
                    "operating_system_id", "event_chain_id", "consent", "date_user", "location",
                    "city", "sf2_consent", "door_url",
                ],
                CustomMetadataKeys: [],
                RowGroups:
                [
                    new ExpectedRowGroup(0, 1, 33, -1, 2753, 2753,
                    [
                        new(0, ["site_section_id"], "INT32", 1, 49, 51, 4, null, 0, "842", "842", "842", "842"),
                        new(1, ["event_id"], "FIXED_LEN_BYTE_ARRAY", 1, 37, 39, 55, null, 0, null, null, null, null),
                        //DuckDB decodes the INT96 into a timestamp; Parquet.NET has no time unit
                        //to work with and yields 0.
                        new(4, ["create_date"], "INT96", 1, 89, 91, 172, null, 0, "2019-01-01 00:00:00", "2019-01-01 00:00:00", "2019-01-01 00:00:00", "2019-01-01 00:00:00"),
                        new(8, ["referent_event_id"], "FIXED_LEN_BYTE_ARRAY", 1, 43, 45, 467, null, 0, null, null, null, null),
                        new(22, ["is_live_stream"], "BOOLEAN", 1, 28, 30, 1775, null, 0, null, null, null, null),
                        new(32, ["door_url"], "BYTE_ARRAY", 1, 119, 121, 2636, null, 0, "www.somedoor.url", "www.somedoor.url", "www.somedoor.url", "www.somedoor.url"),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "root", FieldTypeId.Struct, null, false, null, null, typeof(StructValue)),
                    new(1, "site_section_id", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "INT32", "INT_32", typeof(int)),
                    new(1, "event_id", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "FIXED_LEN_BYTE_ARRAY", null, typeof(Guid)),
                    new(1, "create_date", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "INT96", null, typeof(DateTime)),
                    new(1, "ip_address", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                    new(1, "is_live_stream", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BOOLEAN", null, typeof(bool)),
                ],
                SchemaNodesAreComplete: false),

            ["Data/PARQUET-MR_1.15.0.parquet"] = new(
                ParquetVersion: 1,
                RowCount: 5,
                RowGroupCount: 1,
                CreatedBy: "parquet-mr version 1.15.0 (build 4665401d36e468c988322f78621da7c2d1c22ede)",
                Fields: ["id", "name", "age", "height", "isStudent", "enrollmentDate", "lastLogin"],
                CustomMetadataKeys: ["org.apache.spark.version", "org.apache.spark.sql.parquet.row.metadata"],
                RowGroups:
                [
                    new ExpectedRowGroup(0, 5, 7, 0, 395, 407,
                    [
                        new(0, ["id"], "INT32", 5, 43, 45, 4, null, 0, "1", "5", "1", "5"),
                        //parquet-mr wrote min/max but not min_value/max_value for the string column.
                        new(1, ["name"], "BYTE_ARRAY", 5, 101, 102, 49, null, 0, null, null, "David Lee", "Mike Johnson"),
                        new(2, ["age"], "INT32", 5, 43, 45, 151, null, 0, "28", "35", "28", "35"),
                        new(3, ["height"], "DOUBLE", 5, 63, 60, 196, null, 0, "165.7", "185.1", "165.7", "185.1"),
                        new(4, ["isStudent"], "BOOLEAN", 5, 24, 26, 256, null, 0, "false", "true", "false", "true"),
                        new(5, ["enrollmentDate"], "INT32", 5, 63, 67, 314, 282, 0, "2023-01-01", "2024-12-23", "2023-01-01", "2024-12-23"),
                        //DuckDB keeps the sub second precision of the MICROS timestamp.
                        new(6, ["lastLogin"], "INT64", 5, 58, 62, 378, 349, 0, "2024-12-23 22:08:39.138", "2024-12-23 22:08:39.138", "2024-12-23 22:08:39.138", "2024-12-23 22:08:39.138"),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "spark_schema", FieldTypeId.Struct, null, false, null, null, typeof(StructValue)),
                    new(1, "id", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "INT32", null, typeof(int)),
                    new(1, "name", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                    new(1, "age", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "INT32", null, typeof(int)),
                    new(1, "height", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "DOUBLE", null, typeof(double)),
                    new(1, "isStudent", FieldTypeId.Primitive, RepetitionTypeId.Required, true, "BOOLEAN", null, typeof(bool)),
                    new(1, "enrollmentDate", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "INT32", "DATE", typeof(DateOnly)),
                    new(1, "lastLogin", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "INT64", "TIMESTAMP_MICROS", typeof(DateTime)),
                ]),

            ["Data/RANDOM_TEST_FILE.parquet"] = new(
                ParquetVersion: 2,
                RowCount: 5,
                RowGroupCount: 1,
                CreatedBy: "parquet-cpp-arrow version 9.0.0",
                Fields:
                [
                    "Transmission_Number", "Line_Number", "DC", "GOLD_Article", "GOLD_Storage_Loc",
                    "GOLD_Logistic_Variant", "SAP_Article", "Delivery_Quantity_Base_Unit", "Base_UOM",
                    "Delivery_Quantity_Preparation_Unit", "Preparation_UOM", "Client", "Shipping_Date",
                    "GOLD_Shipping_Id", "Line_Id", "SAP_OBD_Number", "Article_Managed_by_Unit",
                    "Shipped_Net_Weight", "Missing_Quantity", "Missing_Weight", "Pallet_Number",
                    "Mother_Pallet", "Daughter_Pallet", "Code_Picker", "Code_Loader", "Route",
                    "Tour_Rank", "Promo_Number", "Full_Pallet", "Depot_Origine_Transfer",
                    "Depot_Preparation", "Order_Date", "Empty_Included", "Empty_Article_Number_Pallet",
                    "Empty_Article_Number_CV", "Crate_Number", "Warehouse_number", "Child_OBD_Number",
                    "Missing_Motivation", "Promo_Week", "Delivery_Type", "Source_Name",
                ],
                CustomMetadataKeys: ["pandas", "ARROW:schema"],
                RowGroups:
                [
                    new ExpectedRowGroup(0, 5, 42, 108, 3237, 3354,
                    [
                        new(0, ["Transmission_Number"], "INT64", 5, 100, 104, 36, 4, 0, "798921", "799315", "798921", "799315"),
                        new(8, ["Base_UOM"], "DOUBLE", 5, 42, 45, 1517, 1502, 5, null, null, null, null),
                        new(12, ["Shipping_Date"], "BYTE_ARRAY", 5, 82, 86, 2121, 2091, 1, null, null, "2022-08-12", "2022-08-12"),
                        //DuckDB formats the double in scientific notation rather than the full
                        //integer form Parquet.NET holds.
                        new(20, ["Pallet_Number"], "DOUBLE", 5, 109, 113, 3471, 3431, 1, "5.400113103576441e+16", "5.400113300340677e+16", "5.400113103576441e+16", "5.400113300340677e+16"),
                        new(41, ["Source_Name"], "BYTE_ARRAY", 5, 226, 203, 6670, 6587, 0, null, null, "DLIx12_SHIPCONF_BW15_20220811015519979.DWL", "DLIx12_SHIPCONF_BW15_20220812020138531.DWL"),
                    ]),
                ],
                SchemaNodes:
                [
                    new(0, "schema", FieldTypeId.Struct, RepetitionTypeId.Required, false, null, null, typeof(StructValue)),
                    new(1, "Transmission_Number", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "INT64", null, typeof(long)),
                    new(1, "DC", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                    new(1, "Base_UOM", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "DOUBLE", null, typeof(double)),
                    new(1, "Source_Name", FieldTypeId.Primitive, RepetitionTypeId.Optional, true, "BYTE_ARRAY", "UTF8", typeof(string)),
                ],
                SchemaNodesAreComplete: false),
        };
}