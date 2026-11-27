using ParquetViewer.Engine.Types;
using ParquetViewer.Tests.TestHelpers;
using System.Data;

namespace ParquetViewer.Tests;

/// <summary>
/// Covers <c>MainForm.WriteDataToCSVFileAsync</c>, the CSV export.
/// </summary>
[TestClass]
public class CsvExportTests
{
    private readonly List<string> _tempFiles = [];

    [TestCleanup]
    public void Cleanup()
    {
        foreach (var file in _tempFiles)
        {
            try { File.Delete(file); } catch { /*Swallow*/ }
        }
    }

    //Only culture independent values (no doubles or dates) so the expected text is the same on every machine
    private static DataTable BuildTable()
    {
        var table = new DataTable();
        table.Columns.Add("id", typeof(int));
        table.Columns.Add("name", typeof(string));
        table.Columns.Add("tags", typeof(ListValue));

        table.Rows.Add(1, "a", new ListValue([1, 2], typeof(int)));
        table.Rows.Add(2, DBNull.Value, DBNull.Value);
        table.Rows.Add(3, "say \"hi\", bye", new ListValue([], typeof(int)));
        table.Rows.Add(4, "d", new ListValue([3], typeof(int)));
        return table;
    }

    private async Task<string[]> ExportAsync(DataTable table, IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv");
        _tempFiles.Add(path);

        await MainForm.WriteDataToCSVFileAsync(table, path, cancellationToken, progress ?? new Progress<int>());

        return await File.ReadAllLinesAsync(path);
    }

    [TestMethod]
    public async Task EXPORTS_EVERY_ROW_AND_VALUE()
    {
        var lines = await ExportAsync(BuildTable());

        CollectionAssert.AreEqual(new[]
        {
            "id,name,tags",
            "1,a,\"[1,2]\"",                //Quoted because the list contains commas
            "2,,",                          //Nulls are empty
            "3,\"say \"\"hi\"\", bye\",[]", //Quotes are doubled and the value is quoted
            "4,d,[3]",
        }, lines);
    }

    [TestMethod]
    public async Task EXPORT_RESPECTS_DEFAULT_VIEW_SORT_AND_FILTER()
    {
        var table = BuildTable();
        table.DefaultView.RowFilter = "id > 1";
        table.DefaultView.Sort = "id DESC";

        var lines = await ExportAsync(table);

        var ids = lines.Skip(1 /*header*/).Select(line => line.Split(',')[0]).ToArray();
        CollectionAssert.AreEqual(new[] { "4", "3", "2" }, ids);
    }

    [TestMethod]
    public async Task PROGRESS_IS_REPORTED_ONCE_PER_ROW()
    {
        var progress = new ProgressRecorder();

        await ExportAsync(BuildTable(), progress);

        //One report per row, each covering all 3 columns, so the loading bar still reaches 100%
        CollectionAssert.AreEqual(new[] { 3, 3, 3, 3 }, progress.Reports);
    }

    [TestMethod]
    public async Task CANCELLING_STOPS_AT_THE_NEXT_ROW()
    {
        using var cancellation = new CancellationTokenSource();
        var progress = new ProgressRecorder(onReport: cancellation.Cancel); //Cancel as soon as the first row is written

        var lines = await ExportAsync(BuildTable(), progress, cancellation.Token);

        //Only the header and first row are written, rather than empty lines for every remaining row
        CollectionAssert.AreEqual(new[] { "id,name,tags", "1,a,\"[1,2]\"" }, lines);
    }
}