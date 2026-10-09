using Parquet;
using Parquet.Schema;

namespace ParquetViewer.TestFileMutator;

/// <summary>
/// Manual test tool for ParquetViewer's "file was modified" reload behaviour.
///
/// Parquet.NET caches the thrift footer when a file is opened, so once a file is modified in place
/// the running viewer keeps serving stale metadata until it opens a fresh engine. This tool rewrites
/// the target .parquet file out-of-band (temp file + copy over, the same way an external process
/// would) so you can watch ParquetViewer detect the change and reload.
///
/// Usage:
///   create <path> [rows]           Write the baseline file (columns Id, Name, Score, IsActive, Rank).
///   same-columns <path> [rows]     Rewrite with the SAME columns but a different row count and values.
///   different-columns <path>[rows] Rewrite with completely DIFFERENT columns and types.
///   info <path>                    Print the schema and row count that are currently on disk.
///   exit                           Leave interactive mode.
///
/// Run with no arguments to get an interactive prompt instead of a one-shot command.
/// </summary>
internal static class Program
{
    private const int DEFAULT_CREATE_ROWS = 500;
    private const int DEFAULT_SAME_COLUMNS_ROWS = 1000;
    private const int DEFAULT_DIFFERENT_COLUMNS_ROWS = 250;

    private static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        if (args.Length == 0)
        {
            await InteractiveLoopAsync();
            return 0;
        }

        try
        {
            return await RunCommandAsync(args[0], args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static async Task InteractiveLoopAsync()
    {
        Console.WriteLine("ParquetViewer test file mutator - interactive mode. Type 'help' for commands.");
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLowerInvariant();
            if (command is "exit" or "quit")
                return;

            try
            {
                await RunCommandAsync(command, parts);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static async Task<int> RunCommandAsync(string command, string[] args)
    {
        switch (command)
        {
            case "create":
                return await MutateAsync(Scenario.Baseline, ReadPath(args, 1), ReadRows(args, 2, DEFAULT_CREATE_ROWS));

            case "same-columns":
                return await MutateAsync(Scenario.SameColumns, ReadPath(args, 1), ReadRows(args, 2, DEFAULT_SAME_COLUMNS_ROWS));

            case "different-columns":
                return await MutateAsync(Scenario.DifferentColumns, ReadPath(args, 1), ReadRows(args, 2, DEFAULT_DIFFERENT_COLUMNS_ROWS));

            case "info":
                await PrintInfoAsync(ReadPath(args, 1));
                return 0;

            case "help":
                PrintHelp();
                return 0;

            default:
                Console.Error.WriteLine($"Unknown command '{command}'. Type 'help' for commands.");
                return 1;
        }
    }

    private static async Task<int> MutateAsync(Scenario scenario, string path, int rows)
    {
        if (rows <= 0)
            throw new ArgumentException("Row count must be greater than zero.", nameof(rows));

        var directory = System.IO.Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var temp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"parquetviewer-mutator-{Guid.NewGuid():N}.parquet");
        try
        {
            await FileGenerator.WriteAsync(temp, scenario, rows);

            //Copy the freshly written file over the target. This is a true in-place replacement, the
            //same thing an external process modifying the file would do, so the running viewer (which
            //holds the file open with FileShare.ReadWrite) keeps its handle and just sees a change.
            File.Copy(temp, path, overwrite: true);
        }
        finally
        {
            try { File.Delete(temp); } catch (IOException) { /* Best effort */ }
        }

        Console.WriteLine($"[ok] Wrote {rows:N0} rows to `{path}` ({ScenarioDescription(scenario)})");
        return 0;
    }

    private static string ScenarioDescription(Scenario scenario) => scenario switch
    {
        Scenario.Baseline =>
            "baseline: Id(int), Name(string), Score(double), IsActive(bool), Rank(int?)"
            + Environment.NewLine
            + "     Open this file in ParquetViewer to begin. Selected columns: all five.",
        Scenario.SameColumns =>
            "SAME columns as the baseline but a different row count and different values"
            + Environment.NewLine
            + "     In ParquetViewer the title should show '(file was modified)'. Trigger a reload"
            + "     (Ctrl+R or change offset/row count) and you should see the new rows, with your"
            + "     selected columns untouched.",
        Scenario.DifferentColumns =>
            "DIFFERENT columns: Code(long), Label(string), Rank(int), Ratio(double)"
            + Environment.NewLine
            + "     After a reload ParquetViewer should detect your previously selected fields no"
            + "     longer exist and reset the selection to the new columns.",
        _ => throw new ArgumentOutOfRangeException(nameof(scenario))
    };

    private static string ReadPath(string[] args, int index) =>
        args.Length > index ? args[index] : throw new ArgumentException("Missing required argument: <path>");

    private static int ReadRows(string[] args, int index, int defaultValue) =>
        args.Length > index && int.TryParse(args[index], out var rows) ? rows : defaultValue;

    private static async Task PrintInfoAsync(string path)
    {
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"File does not exist: `{path}`");
        }

        //Open with the same share flags the viewer uses so this works while the file is open in ParquetViewer.
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        await using var reader = await ParquetReader.CreateAsync(stream, null, false);

        Console.WriteLine($"File        : {System.IO.Path.GetFullPath(path)}");
        Console.WriteLine($"Row count   : {reader.Metadata?.NumRows:N0}");
        Console.WriteLine($"Row groups  : {reader.RowGroupCount}");
        Console.WriteLine("Schema      :");
        foreach (var field in reader.Schema.Fields)
        {
            var typeName = field is DataField dataField ? dataField.ClrType.Name : field.GetType().Name;
            Console.WriteLine($"  - {field.Name} ({typeName})");
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
            ParquetViewer test file mutator

            Commands:
              create <path> [rows]           Write the baseline (Id, Name, Score, IsActive, Rank). Default 500 rows.
              same-columns <path> [rows]     Rewrite with the SAME columns, different rows/values. Default 1000 rows.
              different-columns <path> [rows] Rewrite with DIFFERENT columns. Default 250 rows.
              info <path>                    Show the schema + row count currently on disk.
              help                           Show this help.
              exit                           Leave interactive mode.

            Typical flow while ParquetViewer has a file open on `<path>`:
              1. create <path>          - make the baseline
              2. same-columns <path>    - modify in place (same columns); check reload keeps columns
              3. different-columns <path> - modify in place (new columns); check reload resets selection
            """);
    }
}

internal enum Scenario
{
    Baseline,
    SameColumns,
    DifferentColumns
}

internal static class FileGenerator
{
    private static readonly ParquetSchema _baselineSchema = new(
        new DataField<int>("Id"),
        new DataField<string>("Name"),
        new DataField<double>("Score"),
        new DataField<bool>("IsActive"),
        new DataField<int?>("Rank"));

    private static readonly ParquetSchema _differentSchema = new(
        new DataField<long>("Code"),
        new DataField<string>("Label"),
        new DataField<int>("Rank"),
        new DataField<double>("Ratio"));

    public static async Task WriteAsync(string path, Scenario scenario, int rows)
    {
        var schema = scenario == Scenario.DifferentColumns ? _differentSchema : _baselineSchema;
        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        await using var writer = await ParquetWriter.CreateAsync(schema, stream, null, false);

        using (var rowGroup = writer.CreateRowGroup())
        {
            foreach (var dataField in schema.GetDataFields())
            {
                await WriteColumnAsync(rowGroup, dataField, scenario, rows);
            }
        }
    }

    private static async Task WriteColumnAsync(ParquetRowGroupWriter rowGroup, DataField field, Scenario scenario, int rows)
    {
        switch (field.Name)
        {
            case "Id":
                await rowGroup.WriteAsync<int>(field, Enumerable.Range(0, rows).ToArray().AsMemory(), null, null, default);
                return;
            case "Name":
                var namePrefix = scenario == Scenario.SameColumns ? "Updated" : "Row";
                await rowGroup.WriteAsync(field, Enumerable.Range(0, rows).Select(i => $"{namePrefix} #{i}").ToArray(), null);
                return;
            case "Score":
                //Same-columns step negates the values so you can tell fresh data from stale at a glance.
                var scoreMultiplier = scenario == Scenario.SameColumns ? -1.5d : 1.5d;
                await rowGroup.WriteAsync<double>(field, Enumerable.Range(0, rows).Select(i => i * scoreMultiplier).ToArray().AsMemory(), null, null, default);
                return;
            case "IsActive":
                await rowGroup.WriteAsync<bool>(field, Enumerable.Range(0, rows).Select(i => i % 2 == 0).ToArray().AsMemory(), null, null, default);
                return;
            case "Rank":
                //Rank is nullable (int?) in the baseline/same-columns schema and required (int) in the different-columns schema.
                if (scenario != Scenario.DifferentColumns)
                    await rowGroup.WriteAsync<int>(field, Enumerable.Range(0, rows).Select(i => (int?)(i % 7)).ToArray().AsMemory(), null, null, default);
                else
                    await rowGroup.WriteAsync<int>(field, Enumerable.Range(0, rows).Select(i => rows - i).ToArray().AsMemory(), null, null, default);
                return;
            case "Code":
                await rowGroup.WriteAsync<long>(field, Enumerable.Range(0, rows).Select(i => 10_000L + i).ToArray().AsMemory(), null, null, default);
                return;
            case "Label":
                await rowGroup.WriteAsync(field, Enumerable.Range(0, rows).Select(i => $"Label #{i}").ToArray(), null);
                return;
            case "Ratio":
                await rowGroup.WriteAsync<double>(field, Enumerable.Range(0, rows).Select(i => i / 100.0d).ToArray().AsMemory(), null, null, default);
                return;
        }

        throw new ArgumentException($"Unexpected field `{field.Name}` for schema '{scenario}'.", nameof(field));
    }
}