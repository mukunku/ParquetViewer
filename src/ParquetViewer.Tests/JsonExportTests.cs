using ParquetViewer.Engine.Types;
using System.Collections;
using System.Data;
using System.Text.Json;

namespace ParquetViewer.Tests;

/// <summary>
/// Covers <c>MainForm.WriteDataToJSONFileAsync</c>, the JSON export.
/// </summary>
[TestClass]
public class JsonExportTests
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

    private static DataTable BuildTable()
    {
        var table = new DataTable();
        table.Columns.Add("id", typeof(int));
        table.Columns.Add("name", typeof(string));
        table.Columns.Add("score", typeof(double));
        table.Columns.Add("tags", typeof(ListValue));

        table.Rows.Add(1, "a", 1.5, new ListValue(new ArrayList { 1, 2 }, typeof(int)));
        table.Rows.Add(2, DBNull.Value, double.NaN, DBNull.Value);
        table.Rows.Add(3, "c", 1e300, new ListValue(new ArrayList(), typeof(int)));
        table.Rows.Add(4, "d", 0.25, new ListValue(new ArrayList { 3 }, typeof(int)));
        return table;
    }

    private async Task<JsonElement> ExportAsync(DataTable table, IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
        _tempFiles.Add(path);

        await MainForm.WriteDataToJSONFileAsync(table, path, cancellationToken, progress ?? new Progress<int>());

        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(path));
        return document.RootElement.Clone();
    }

    [TestMethod]
    public async Task EXPORTS_EVERY_ROW_AND_VALUE()
    {
        var json = await ExportAsync(BuildTable());

        Assert.AreEqual(4, json.GetArrayLength());

        var first = json[0];
        Assert.AreEqual(1, first.GetProperty("id").GetInt32());
        Assert.AreEqual("a", first.GetProperty("name").GetString());
        Assert.AreEqual(1.5, first.GetProperty("score").GetDouble());
        Assert.AreEqual("[1,2]", first.GetProperty("tags").GetRawText());

        var withNulls = json[1];
        Assert.AreEqual(JsonValueKind.Null, withNulls.GetProperty("name").ValueKind);
        Assert.AreEqual("NaN", withNulls.GetProperty("score").GetString()); //JSON has no NaN, so it's written as a string
        Assert.AreEqual(JsonValueKind.Null, withNulls.GetProperty("tags").ValueKind);

        var withHugeNumber = json[2];
        Assert.AreEqual(1e300, withHugeNumber.GetProperty("score").GetDouble()); //Too large for a decimal
        Assert.AreEqual("[]", withHugeNumber.GetProperty("tags").GetRawText());
    }

    [TestMethod]
    public async Task EXPORT_RESPECTS_DEFAULT_VIEW_SORT_AND_FILTER()
    {
        var table = BuildTable();
        table.DefaultView.RowFilter = "id > 1";
        table.DefaultView.Sort = "id DESC";

        var json = await ExportAsync(table);

        var ids = json.EnumerateArray().Select(row => row.GetProperty("id").GetInt32()).ToArray();
        CollectionAssert.AreEqual(new[] { 4, 3, 2 }, ids);
    }

    [TestMethod]
    public async Task PROGRESS_IS_REPORTED_ONCE_PER_ROW()
    {
        var progress = new RecordingProgress();

        await ExportAsync(BuildTable(), progress);

        //One report per row, each covering all 4 columns, so the loading bar still reaches 100%
        CollectionAssert.AreEqual(new[] { 4, 4, 4, 4 }, progress.Reports);
    }

    [TestMethod]
    public async Task CANCELLING_STOPS_AT_THE_NEXT_ROW()
    {
        using var cancellation = new CancellationTokenSource();
        var progress = new RecordingProgress(onReport: cancellation.Cancel); //Cancel as soon as the first row is written

        var json = await ExportAsync(BuildTable(), progress, cancellation.Token);

        //Only the first row is written, rather than empty objects for every remaining row
        Assert.AreEqual(1, json.GetArrayLength());
        Assert.AreEqual(1, json[0].GetProperty("id").GetInt32());
    }

    /// <summary>
    /// Records reports synchronously, unlike <see cref="Progress{T}"/> which posts them to a sync context.
    /// </summary>
    private sealed class RecordingProgress(Action? onReport = null) : IProgress<int>
    {
        public List<int> Reports { get; } = [];

        public void Report(int value)
        {
            Reports.Add(value);
            onReport?.Invoke();
        }
    }
}
