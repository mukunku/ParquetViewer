using Parquet;
using Parquet.Schema;
using ParquetViewer.Engine.Exceptions;
using System.Collections.Concurrent;
using System.Reflection;

namespace ParquetViewer.Engine.ParquetNET;

/// <summary>
/// Reads a single column out of a <see cref="ParquetRowGroupReader"/> into a <see cref="ParquetColumnData"/>.
/// </summary>
/// <remarks>
/// <para>
/// Parquet.Net v6 replaced <c>DataColumn</c> with <c>RawColumnData&lt;T&gt;</c>, whose values are pooled
/// spans not arrays, so we allocate the buffers ourselves and copy the values into a managed array.
/// </para>
/// <para>
/// v6 also removed the untyped read entry point: <see cref="ParquetRowGroupReader.ReadRawAsync{T}"/> is
/// generic and constrained to <c>where T : struct</c>, so the element type has to be supplied at runtime.
/// This class resolves the type reflectively rather than if/else'ing over every <see cref="DataField.ClrType"/> 
/// Parquet.Net can report. 
/// 
/// Advantages of reflection approach:
/// - Reflection has no type list to maintain. New primitive types can automatically work in theory.
/// - Don't have to maintain a giant list of if/else's like we do in <see cref="ParquetColumnData.Data"/>.
/// - Performance seemed the same in my local testing.
/// </para>
/// </remarks>
internal static class ReflectiveColumnReader
{
    /// <summary>
    /// The reflection state built once per CLR type and reused for every column of that type.
    /// </summary>
    private sealed record ReflectiveRead(MethodInfo ReadMethod, ConstructorInfo MemoryConstructor);

    //MakeGenericMethod, the MethodInfo lookup and the Memory<T> constructor lookup are all non-trivial, and
    //the same handful of types recur for every column of every row group, so they are cached per CLR type.
    private static readonly ConcurrentDictionary<Type, ReflectiveRead> _reads = new();

    /// <summary>
    /// Reads one column, including its definition and repetition levels, into a
    /// <see cref="ParquetColumnData"/>.
    /// </summary>
    public static async Task<ParquetColumnData> ReadAsync(ParquetRowGroupReader groupReader, ParquetSchemaElement fieldSchemaElement,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(groupReader);
        ArgumentNullException.ThrowIfNull(fieldSchemaElement);

        var field = fieldSchemaElement.DataField ?? throw new MalformedFieldException($"Field `{fieldSchemaElement.PathWithParent}` has no data field");
        var columnChunk = groupReader.GetMetadata(field)
            ?? throw new ParquetException($"`{field.Path}` does not exist in this file");

        var numValues = checked((int)(columnChunk.MetaData?.NumValues
            ?? throw new ParquetException($"Column chunk metadata is missing for `{field.Path}`, meaning the file is most probably corrupt")));

        int[]? definitionLevels = null;
        int[]? repetitionLevels = null;
        if (field.MaxDefinitionLevel > 0 || field.MaxRepetitionLevel > 0)
        {
            definitionLevels = new int[numValues];
            repetitionLevels = new int[numValues];
        }

        var values = await ReadValuesAsync(groupReader, field, numValues, definitionLevels, repetitionLevels, cancellationToken);

        return new ParquetColumnData(fieldSchemaElement, values, definitionLevels, repetitionLevels);
    }

    /// <summary>
    /// Reads the raw values of a column into a new array, writing the definition and repetition levels back
    /// into the caller's buffers.
    /// </summary>
    internal static async Task<Array> ReadValuesAsync(ParquetRowGroupReader groupReader, DataField field, int numValues,
        int[]? definitionLevels, int[]? repetitionLevels, CancellationToken cancellationToken)
    {
        var clrType = field.ClrType;

        //ReadRawAsync<T> is constrained to `where T : struct`. Array.CreateInstance would happily make a
        //string[] for us and MakeGenericMethod would then fail with a much less obvious message, so stop here.
        if (!clrType.IsValueType)
        {
            throw new ParquetEngineException(
                $"Column `{field.Path}` with type '{clrType}' is not a value type.");
        }

        //Resolved outside the try so that a failure to build the reflection state isn't re-wrapped below
        var read = _reads.GetOrAdd(clrType, CreateRead);
        var values = Array.CreateInstance(clrType, numValues);

        try
        {
            await ReadCoreAsync(groupReader, field, read, values, definitionLevels, repetitionLevels,
                cancellationToken);
        }
        catch (OverflowException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new ParquetEngineException(
                $"Parquet.Net describes column `{field.Path}` as '{clrType}', which could not be read " +
                $"({ex.Message}).", ex);
        }

        return values;
    }

    private static async Task ReadCoreAsync(ParquetRowGroupReader groupReader, DataField field, ReflectiveRead read,
        Array values, int[]? definitionLevels, int[]? repetitionLevels, CancellationToken cancellationToken)
    {
        object?[] arguments =
        [
            field,
            read.MemoryConstructor.Invoke([values]),
            definitionLevels is not null ? new Memory<int>(definitionLevels) : (Memory<int>?)null,
            repetitionLevels is not null ? new Memory<int>(repetitionLevels) : (Memory<int>?)null,
            cancellationToken
        ];

        try
        {
            await (ValueTask)read.ReadMethod.Invoke(groupReader, arguments)!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw ex.InnerException;
        }
    }

    /// <exception cref="ParquetEngineException">
    /// Parquet.Net no longer exposes a <c>ReadRawAsync&lt;T&gt;</c> this can bind to,
    /// which would means a new package version broke things.
    /// </exception>
    private static ReflectiveRead CreateRead(Type clrType)
    {
        var readMethod = typeof(ParquetRowGroupReader)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method is { IsGenericMethodDefinition: true, Name: nameof(ParquetRowGroupReader.ReadRawAsync) }
                             && method.GetParameters().Length == 5)
            .SingleOrDefault()
            ?? throw new ParquetEngineException(
                "Could not find a `ParquetRowGroupReader.ReadRawAsync<T>(DataField, Memory<T>, " +
                "Memory<int>?, Memory<int>?, CancellationToken)` to call, so no column can be read. " +
                "This means Parquet.Net changed the method's shape in a new major version.");

        var memoryType = typeof(Memory<>).MakeGenericType(clrType);
        var memoryConstructor = memoryType.GetConstructor([clrType.MakeArrayType()])
            ?? throw new ParquetEngineException(
                $"Could not find the `Memory<{clrType}>({clrType}[])` constructor Parquet.Net needs.");

        return new ReflectiveRead(readMethod.MakeGenericMethod(clrType), memoryConstructor);
    }
}