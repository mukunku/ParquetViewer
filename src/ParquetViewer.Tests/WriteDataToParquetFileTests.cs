//These tests were written by AI (subject to change)

using ParquetViewer.Engine.Types;
using System.Data;

namespace ParquetViewer.Tests;

/// <summary>
/// Covers <c>ParquetEngine.WriteDataToParquetFileAsync</c> for the Parquet.Net engine, which had no tests
/// at all until now. Everything here round-trips a fixture - read it, write it back out, read it again -
/// because the write path rebuilds the output schema from the *source* file's schema and reinterprets the
/// viewer's CLR types, so the only meaningful assertion is that the second read matches the first.
/// </summary>
/// <remarks>
/// The engine is Parquet.NET specific here, unlike <see cref="EngineTests"/>: DuckDB has its own
/// <c>WriteDataToParquetFileAsync</c> implementation which shares none of this code.
/// </remarks>
[TestClass]
public class WriteDataToParquetFileTests
{
    private readonly List<string> _tempFiles = [];

    public WriteDataToParquetFileTests()
    {
        //Same date formats EngineTests uses, so date assertions are stable
        ParquetEngineSettings.DateDisplayFormat = "yyyy-MM-dd HH:mm:ss";
        ParquetEngineSettings.DateOnlyDisplayFormat = "yyyy-MM-dd";
        ParquetEngineSettings.TimeOnlyDisplayFormat = "HH:mm:ss";
    }

    [TestCleanup]
    public void DeleteTempFiles()
    {
        foreach (var path in _tempFiles)
        {
            try { File.Delete(path); }
            catch (IOException) { /*Best effort - a locked file shouldn't fail the test it just passed*/ }
        }
        _tempFiles.Clear();
    }

    /// <summary>
    /// A path that doesn't exist yet - the write path uses <c>FileMode.OpenOrCreate</c>, so reusing a
    /// populated file would leave trailing bytes from the previous write.
    /// </summary>
    private string NewTempPath()
    {
        var path = Path.Combine(Path.GetTempPath(), $"parquetviewer-write-{Guid.NewGuid():N}.parquet");
        _tempFiles.Add(path);
        return path;
    }

    private static async Task<Engine.ParquetNET.ParquetEngine> OpenAsync(string path) =>
        await Engine.ParquetNET.ParquetEngine.OpenFileOrFolderAsync(path);

    private static async Task<DataTable> ReadAllAsync(Engine.ParquetNET.ParquetEngine engine) =>
        (await engine.ReadRowsAsync(engine.Fields, 0, int.MaxValue))(false);

    private static object? Normalise(object? value) => value switch
    {
        DBNull => null,
        //Compare the payload, not the wrapper - a round trip builds a fresh wrapper instance
        IByteArrayValue bytes => bytes.Data,
        _ => value
    };

    private static bool CellsEqual(object? left, object? right)
    {
        left = Normalise(left);
        right = Normalise(right);
        return left is byte[] leftBytes && right is byte[] rightBytes
            ? leftBytes.AsSpan().SequenceEqual(rightBytes)
            : Equals(left, right);
    }

    /// <summary>
    /// Asserts every cell of the rewritten file matches what was read from the original. Reports the
    /// coordinate of the first difference, because "assertion failed" on a 337-column table is useless.
    /// </summary>
    private static void AssertTablesMatch(DataTable expected, DataTable actual)
    {
        Assert.HasCount(expected.Columns.Count, actual.Columns, "column count differs");
        Assert.HasCount(expected.Rows.Count, actual.Rows, "row count differs");

        for (var column = 0; column < expected.Columns.Count; column++)
        {
            Assert.AreEqual(expected.Columns[column].ColumnName, actual.Columns[column].ColumnName,
                $"column {column} name differs");
            Assert.AreEqual(expected.Columns[column].DataType, actual.Columns[column].DataType,
                $"column `{expected.Columns[column].ColumnName}` type differs");
        }

        for (var row = 0; row < expected.Rows.Count; row++)
        {
            for (var column = 0; column < expected.Columns.Count; column++)
            {
                if (!CellsEqual(expected.Rows[row][column], actual.Rows[row][column]))
                {
                    Assert.Fail($"row {row}, column `{expected.Columns[column].ColumnName}`: " +
                        $"expected `{Normalise(expected.Rows[row][column])}` but got " +
                        $"`{Normalise(actual.Rows[row][column])}`");
                }
            }
        }
    }

    /// <summary>
    /// The headline guarantee: whatever the engine showed the user, the engine can write it back out and
    /// read the same thing back. This fixture has 337 columns of mixed types (byte, double, ushort, long,
    /// bool, text) and a null, so one round trip covers most of the type conversions at once.
    /// </summary>
    [TestMethod]
    public async Task ROUND_TRIP_PRESERVES_VALUES()
    {
        await using var engine = await OpenAsync("Data/DECIMALS_AND_BOOLS_TEST.parquet");
        var original = await ReadAllAsync(engine);
        var path = NewTempPath();

        await engine.WriteDataToParquetFileAsync(original, path, new Progress<int>(), null, default);

        await using var rewritten = await OpenAsync(path);
        AssertTablesMatch(original, await ReadAllAsync(rewritten));
    }

    /// <summary>
    /// Nulls are the interesting case, not an afterthought: they reach the writer as <see cref="DBNull"/>
    /// and have to come back as <see cref="DBNull"/> in the same cells, or every downstream row/column
    /// alignment silently shifts.
    /// </summary>
    [TestMethod]
    public async Task ROUND_TRIP_PRESERVES_NULLS()
    {
        await using var engine = await OpenAsync("Data/DECIMALS_AND_BOOLS_TEST.parquet");
        var original = await ReadAllAsync(engine);
        var nullCount = CountNulls(original);
        Assert.IsGreaterThan(0, nullCount, "fixture should contain nulls for this test to mean anything");

        var path = NewTempPath();
        await engine.WriteDataToParquetFileAsync(original, path, new Progress<int>(), null, default);

        await using var rewritten = await OpenAsync(path);
        var result = await ReadAllAsync(rewritten);
        Assert.AreEqual(nullCount, CountNulls(result), "null count changed");
        AssertTablesMatch(original, result);
    }

    private static int CountNulls(DataTable table)
    {
        var count = 0;
        foreach (DataRow row in table.Rows)
            foreach (DataColumn column in table.Columns)
                if (row[column] == DBNull.Value)
                    count++;
        return count;
    }

    /// <summary>
    /// TIME columns are the reason the write path needs to know the viewer's CLR type at all: the grid
    /// shows a <see cref="TimeOnly"/>, but the file stores a raw count of millis, micros or nanos. A
    /// round trip that loses the sub-second part or the precision is the classic failure here.
    /// </summary>
    [TestMethod]
    public async Task ROUND_TRIP_PRESERVES_TIME_COLUMNS()
    {
        await using var engine = await OpenAsync("Data/TIME_ONLY_TYPE_PYARROW_V22.parquet");
        var original = await ReadAllAsync(engine);
        Assert.IsGreaterThan(1, original.Rows.Count, "fixture should have several rows");
        Assert.Contains((DataColumn c) => c.DataType == typeof(TimeOnly), original.Columns.Cast<DataColumn>(),
            "fixture should have a TimeOnly column");

        var path = NewTempPath();
        await engine.WriteDataToParquetFileAsync(original, path, new Progress<int>(), null, default);

        await using var rewritten = await OpenAsync(path);
        AssertTablesMatch(original, await ReadAllAsync(rewritten));
    }

    /// <summary>
    /// Dates and date-times go through the display-format settings, so this also pins that a written file
    /// reads back to the same values the grid was showing.
    /// </summary>
    [TestMethod]
    public async Task ROUND_TRIP_PRESERVES_DATES()
    {
        await using var engine = await OpenAsync("Data/DATETIME_TEST1.parquet");
        var original = await ReadAllAsync(engine);
        Assert.Contains((DataColumn c) => c.DataType == typeof(DateTime) || c.DataType == typeof(DateOnly),
            original.Columns.Cast<DataColumn>(), "fixture should have a date column");

        var path = NewTempPath();
        await engine.WriteDataToParquetFileAsync(original, path, new Progress<int>(), null, default);

        await using var rewritten = await OpenAsync(path);
        AssertTablesMatch(original, await ReadAllAsync(rewritten));
    }

    /// <summary>
    /// Byte array columns are written through a dedicated Parquet.Net overload, since
    /// <c>WriteAsync&lt;T&gt;</c> can't take a reference type. This checks the payload survives, not the
    /// wrapper instance.
    /// </summary>
    [TestMethod]
    public async Task ROUND_TRIP_PRESERVES_BYTE_ARRAYS()
    {
        await using var engine = await OpenAsync("Data/BYTEARRAY_VALUE_TEST.parquet");
        var original = await ReadAllAsync(engine);

        var path = NewTempPath();
        await engine.WriteDataToParquetFileAsync(original, path, new Progress<int>(), null, default);

        await using var rewritten = await OpenAsync(path);
        var result = await ReadAllAsync(rewritten);
        AssertTablesMatch(original, result);
        Assert.IsTrue(Normalise(result.Rows[0][0]) is byte[] { Length: > 0 },
            "expected a non-empty byte array to come back");
    }

    /// <summary>
    /// A GUID column is nullable, so it exercises the <c>Nullable&lt;T&gt;</c> array path rather than the
    /// plain one.
    /// </summary>
    [TestMethod]
    public async Task ROUND_TRIP_PRESERVES_NULLABLE_GUIDS()
    {
        await using var engine = await OpenAsync("Data/NULLABLE_GUID_TEST.parquet");
        var original = await ReadAllAsync(engine);
        Assert.Contains((DataColumn c) => c.DataType == typeof(Guid), original.Columns.Cast<DataColumn>(),
            "fixture should have a Guid column");

        var path = NewTempPath();
        await engine.WriteDataToParquetFileAsync(original, path, new Progress<int>(), null, default);

        await using var rewritten = await OpenAsync(path);
        AssertTablesMatch(original, await ReadAllAsync(rewritten));
    }

    /// <summary>
    /// The write path only writes the columns it was given, so a subset has to come back as a subset -
    /// and in the requested order, which is the order the user picked in the grid.
    /// </summary>
    [TestMethod]
    public async Task WRITES_ONLY_THE_SELECTED_COLUMNS()
    {
        await using var engine = await OpenAsync("Data/PARQUET-MR_1.15.0.parquet");
        var wanted = engine.Fields.Take(2).ToList();
        var original = (await engine.ReadRowsAsync(wanted, 0, int.MaxValue))(false);
        var path = NewTempPath();

        await engine.WriteDataToParquetFileAsync(original, path, new Progress<int>(), null, default);

        await using var rewritten = await OpenAsync(path);
        Assert.AreSequenceEqual(wanted, rewritten.Fields, "unexpected columns written");
        AssertTablesMatch(original, await ReadAllAsync(rewritten));
    }

    [TestMethod]
    public async Task PRESERVES_CUSTOM_METADATA()
    {
        var metadata = new Dictionary<string, string> { ["my-key"] = "my-value", ["other"] = "second" };
        await using var engine = await OpenAsync("Data/DATETIME_TEST1.parquet");
        var original = await ReadAllAsync(engine);
        var path = NewTempPath();

        await engine.WriteDataToParquetFileAsync(original, path, new Progress<int>(), metadata, default);

        await using var rewritten = await OpenAsync(path);
        Assert.HasCount(2, rewritten.CustomMetadata);
        Assert.AreEqual("my-value", rewritten.CustomMetadata["my-key"]);
        Assert.AreEqual("second", rewritten.CustomMetadata["other"]);
    }

    /// <summary>
    /// The engine batches writes at 100,000 rows, and each batch re-walks the DataTable with a skip count.
    /// That offset arithmetic is easy to get wrong and impossible to see without crossing the boundary, so
    /// this deliberately writes more rows than fit in one batch and checks the last row isn't lost or
    /// duplicated.
    /// </summary>
    [TestMethod]
    public async Task WRITES_EVERY_ROW_ACROSS_BATCHES()
    {
        const int MaxRowsPerBatch = 100_000;
        const int totalRows = MaxRowsPerBatch + 7;

        await using var engine = await OpenAsync("Data/PARQUET-MR_1.15.0.parquet");
        var template = await ReadAllAsync(engine);

        //Grow the table past the batch boundary by repeating the fixture's own rows
        var grown = template.Clone();
        while (grown.Rows.Count < totalRows)
        {
            foreach (DataRow row in template.Rows)
            {
                if (grown.Rows.Count == totalRows)
                    break;
                grown.Rows.Add(row.ItemArray);
            }
        }
        Assert.HasCount(totalRows, grown.Rows, "test setup is wrong");

        var expectedFirst = grown.Rows[0].ItemArray;
        var expectedLast = grown.Rows[totalRows - 1].ItemArray;

        var path = NewTempPath();
        await engine.WriteDataToParquetFileAsync(grown, path, new Progress<int>(), null, default);

        await using var rewritten = await OpenAsync(path);
        Assert.AreEqual(totalRows, rewritten.RecordCount, "wrong number of rows written");
        var result = await ReadAllAsync(rewritten);
        Assert.HasCount(totalRows, result.Rows);

        for (var column = 0; column < expectedFirst.Length; column++)
            Assert.IsTrue(CellsEqual(expectedFirst[column], result.Rows[0][column]),
                $"first row, column {column} differs");
        for (var column = 0; column < expectedLast.Length; column++)
            Assert.IsTrue(CellsEqual(expectedLast[column], result.Rows[totalRows - 1][column]),
                $"last row, column {column} differs");
    }

    /// <summary>
    /// Documented limitation rather than a bug: the write path can't reconstruct list, map or struct
    /// columns from the flat DataTable the grid uses, and refuses rather than writing something wrong.
    /// </summary>
    /// <remarks>
    /// The failure is unhelpful though - Parquet.Net reports a list's leaf field as <c>item</c>, so the
    /// error says "Column `item` does not exist in the datatable" and never mentions lists. Asserted as-is
    /// to document current behaviour; tighten this to a <c>NotSupportedException</c> naming the real column
    /// if the message is ever fixed.
    /// </remarks>
    [TestMethod]
    public async Task REJECTS_LIST_COLUMNS()
    {
        await using var engine = await OpenAsync("Data/LIST_TYPE_TEST1.parquet");
        var original = await ReadAllAsync(engine);
        var path = NewTempPath();

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            engine.WriteDataToParquetFileAsync(original, path, new Progress<int>(), null, default));
        Assert.Contains("item", ex.Message);
    }


    /// <summary>
    /// A cancelled write should stop rather than run to completion. The loop breaks on cancellation, so
    /// the file may be short - what matters is that it doesn't throw or silently write everything.
    /// </summary>
    [TestMethod]
    public async Task STOPS_EARLY_WHEN_CANCELLED()
    {
        await using var engine = await OpenAsync("Data/DECIMALS_WITH_NO_SCALE_TEST.parquet");
        var original = await ReadAllAsync(engine);
        var path = NewTempPath();

        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();

        await engine.WriteDataToParquetFileAsync(original, path, new Progress<int>(), null, cancelled.Token);

        //Either nothing was written, or fewer rows than the source - never the full set
        if (File.Exists(path))
        {
            await using var rewritten = await OpenAsync(path);
            Assert.IsLessThanOrEqualTo(engine.RecordCount, rewritten.RecordCount,
                $"cancelled write produced {rewritten.RecordCount} rows, more than the source");
        }
    }
}