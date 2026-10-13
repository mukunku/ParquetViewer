//These tests were written by AI (subject to change)

using Parquet;
using Parquet.Schema;
using ParquetViewer.Engine.ParquetNET;

namespace ParquetViewer.Tests;

/// <summary>
/// Every column read goes through <c>ReflectiveColumnReader</c>, because Parquet.Net v6's only read entry
/// point is generic over the value type and the type is only known at runtime.
/// </summary>
/// <remarks>
/// <para>
/// These tests hold that path to its contract: for any type, reading a column reflectively must produce
/// exactly what Parquet.Net's own public <c>ReadRawAsync&lt;T&gt;</c> produces for that type - the same
/// values, and the same definition and repetition levels written back into the caller's buffers. That direct
/// call is the oracle, so the assertions stay meaningful now that there is no separate "typed path" left to
/// compare against.
/// </para>
/// <para>
/// This covers the reflection machinery itself - method lookup, the per-type cache, <c>Memory&lt;T&gt;</c>
/// construction, the <c>ValueTask</c> await, <c>TargetInvocationException</c> unwrapping and the failure wrap
/// - none of which is reachable by asserting on engine output alone.
/// </para>
/// The cancellation pass-through is deliberately not tested here. Parquet.Net 6.1 doesn't forward the
/// token to the stream on the column-read path (verified: a read issued with an already-cancelled token
/// completes normally, and makes no cancellable <c>Stream.ReadAsync</c> call), so no read through
/// <see cref="ReflectiveColumnReader"/> can be made to raise <see cref="OperationCanceledException"/>
/// deterministically. The <c>ex is not OperationCanceledException</c> filter stays as cheap insurance.
/// </remarks>

/// </remarks>
[TestClass]
public class ReflectiveColumnReaderTests
{
    /// <summary>
    /// Same shape as the engine's own open, so the reader gets the same options the app uses.
    /// </summary>
    private static async Task<ParquetReader> OpenAsync(string path)
    {
        var stream = File.OpenRead(path);
        try
        {
            return await ParquetReader.CreateAsync(stream, new ParquetOptions(), leaveStreamOpen: false);
        }
        catch
        {
            await stream.DisposeAsync();
            throw;
        }
    }

    private static int NumValues(ParquetRowGroupReader rowGroup, DataField field) =>
        checked((int)(rowGroup.GetMetadata(field)?.MetaData?.NumValues
            ?? throw new ParquetException($"'{field.Path}' does not exist in this file")));

    private static DataField FieldOfType(ParquetReader reader, Type clrType) =>
        reader.Schema.DataFields.FirstOrDefault(f => f.ClrType == clrType)
        ?? throw new InvalidOperationException($"No column of type `{clrType}` in this fixture");

    /// <summary>
    /// <see cref="ReadOnlyMemory{T}"/> compares by backing-store identity, and the two paths read into
    /// separate buffers, so text has to be compared by its characters. Everything else compares directly.
    /// </summary>
    private static object Comparable<T>(T value) =>
        value is ReadOnlyMemory<char> text ? text.Span.ToString() : value!;

    /// <summary>
    /// Reads a column reflectively and asserts it is indistinguishable from Parquet.Net's own
    /// <c>ReadRawAsync&lt;T&gt;</c> - same values, and same definition/repetition levels written back
    /// into the caller's buffers.
    /// </summary>
    private static async Task AssertReflectiveMatchesTyped<T>(ParquetRowGroupReader rowGroup, DataField field)
        where T : struct
    {
        var numValues = NumValues(rowGroup, field);

        //The oracle: Parquet.Net's own public API, called the way a hand-written typed reader would
        var typedValues = new T[numValues];
        var typedDefinitionLevels = new int[numValues];
        var typedRepetitionLevels = new int[numValues];
        await rowGroup.ReadRawAsync<T>(field, new Memory<T>(typedValues),
            new Memory<int>(typedDefinitionLevels), new Memory<int>(typedRepetitionLevels),
            CancellationToken.None);

        //The reflective path
        var definitionLevels = new int[numValues];
        var repetitionLevels = new int[numValues];
        var reflected = await ReflectiveColumnReader.ReadValuesAsync(rowGroup, field, numValues, definitionLevels,
            repetitionLevels, CancellationToken.None);

        Assert.AreEqual(typeof(T[]), reflected.GetType(), "the reader should build a T[]");
        Assert.HasCount(numValues, reflected);
        Assert.AreSequenceEqual(typedDefinitionLevels, definitionLevels,
            $"definition levels differ for `{field.Path}`");
        Assert.AreSequenceEqual(typedRepetitionLevels, repetitionLevels,
            $"repetition levels differ for `{field.Path}`");
        for (var index = 0; index < numValues; index++)
        {
            Assert.AreEqual(Comparable(typedValues[index]), Comparable((T)reflected.GetValue(index)!),
                $"value {index} differs for `{field.Path}`");
        }

    }

    [TestMethod]
    public async Task REFLECTION_MATCHES_TYPED_PATH_FOR_EACH_SCALAR_TYPE()
    {
        await using var reader = await OpenAsync("Data/DECIMALS_AND_BOOLS_TEST.parquet");
        var rowGroup = reader.RowGroups[0];

        await AssertReflectiveMatchesTyped<ushort>(rowGroup, FieldOfType(reader, typeof(ushort)));
        await AssertReflectiveMatchesTyped<byte>(rowGroup, FieldOfType(reader, typeof(byte)));
        await AssertReflectiveMatchesTyped<long>(rowGroup, FieldOfType(reader, typeof(long)));
        await AssertReflectiveMatchesTyped<double>(rowGroup, FieldOfType(reader, typeof(double)));
        await AssertReflectiveMatchesTyped<bool>(rowGroup, FieldOfType(reader, typeof(bool)));
    }

    /// <summary>
    /// Two columns of the same type, so the second call goes through the cached <c>MethodInfo</c> rather
    /// than rebuilding it.
    /// </summary>
    [TestMethod]
    public async Task REFLECTION_MATCHES_TYPED_PATH_AND_SERVES_THE_METHOD_CACHE()
    {
        await using var reader = await OpenAsync("Data/DECIMALS_AND_BOOLS_TEST.parquet");
        var rowGroup = reader.RowGroups[0];
        var ushorts = reader.Schema.DataFields.Where(f => f.ClrType == typeof(ushort)).ToList();
        Assert.IsGreaterThanOrEqualTo(2, ushorts.Count, "fixture should have at least two ushort columns");

        await AssertReflectiveMatchesTyped<ushort>(rowGroup, ushorts[0]);
        await AssertReflectiveMatchesTyped<ushort>(rowGroup, ushorts[1]);
    }

    /// <summary>
    /// <c>ReadOnlyMemory&lt;char&gt;</c> is the most interesting case: it's a reference-shaped struct
    /// over a pooled buffer, so it's the read most likely to differ between the two paths.
    /// </summary>
    [TestMethod]
    public async Task REFLECTION_MATCHES_TYPED_PATH_FOR_TEXT()
    {
        await using var reader = await OpenAsync("Data/DECIMALS_AND_BOOLS_TEST.parquet");
        var rowGroup = reader.RowGroups[0];

        await AssertReflectiveMatchesTyped<ReadOnlyMemory<char>>(
            rowGroup, FieldOfType(reader, typeof(ReadOnlyMemory<char>)));
    }

    /// <summary>
    /// The interesting one: a repeated, nullable list element, where the definition and repetition
    /// levels are non-trivial and the values are compacted. A fallback that mishandled the level
    /// buffers would desynchronise every value after the first gap.
    /// </summary>
    [TestMethod]
    public async Task REFLECTION_MATCHES_TYPED_PATH_FOR_REPEATED_NULLABLE_COLUMN()
    {
        await using var reader = await OpenAsync("Data/LIST_TYPE_TEST1.parquet");
        var rowGroup = reader.RowGroups[0];

        var listItem = reader.Schema.Flatten().OfType<DataField>()
            .First(f => f.MaxRepetitionLevel > 0 && f.ClrType == typeof(long));
        Assert.IsGreaterThan(1, listItem.MaxDefinitionLevel, "fixture should have a non-trivial definition level");

        await AssertReflectiveMatchesTyped<long>(rowGroup, listItem);
    }

    [TestMethod]
    public async Task DECIMAL_OVERFLOW_ERRORS_SURFACE_AS_EXPECTED()
    {
        await using var reader = await OpenAsync("Data/DECIMALS_OUTOFRANGE_TEST.parquet");
        var rowGroup = reader.RowGroups[0];
        var field = FieldOfType(reader, typeof(decimal));
        var numValues = NumValues(rowGroup, field);

        _ = await Assert.ThrowsAsync<OverflowException>(() =>
            ReflectiveColumnReader.ReadValuesAsync(rowGroup, field, numValues, new int[numValues],
                new int[numValues], CancellationToken.None));
    }
}