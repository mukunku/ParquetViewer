using ParquetViewer.Engine.Exceptions;
using ParquetViewer.Engine.Types;
using ParquetViewer.Tests.TestAttributes;
using ParquetViewer.Tests.TestHelpers;
using System.Data;
using System.Text.Json;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
namespace ParquetViewer.Tests;

[TestClass]
public class ParquetNETEngineTests : EngineTests
{
    public ParquetNETEngineTests() : base(
        useDuckDBEngine: false,
        treatsTwoTierListAsStruct: true,
        "/")
    {

    }
}

[TestClass]
public class DuckDBEngineTests : EngineTests
{
    public DuckDBEngineTests() : base(
        useDuckDBEngine: true,
        treatsTwoTierListAsStruct: false,
        ", ")
    {

    }
}

public abstract class EngineTests
{
    private readonly bool _useDuckDBEngine;
    private readonly bool _treatsTwoTierListAsStruct;
    private readonly string _schemaPathSeperator;

    public EngineTests(bool useDuckDBEngine, bool treatsTwoTierListAsStruct, string schemaPathSeperator)
    {
        //Set a consistent date format for all tests
        ParquetEngineSettings.DateDisplayFormat = "yyyy-MM-dd HH:mm:ss";
        ParquetEngineSettings.DateOnlyDisplayFormat = "yyyy-MM-dd";
        ParquetEngineSettings.TimeOnlyDisplayFormat = "HH:mm:ss";

        _useDuckDBEngine = useDuckDBEngine;
        _treatsTwoTierListAsStruct = treatsTwoTierListAsStruct;
        _schemaPathSeperator = schemaPathSeperator;
    }

    private async Task<IParquetEngine> OpenFileOrFolderAsync(string path)
    {
        if (_useDuckDBEngine)
        {
            return await Engine.DuckDB.ParquetEngine.OpenFileOrFolderAsync(path);
        }
        else
        {
            return await Engine.ParquetNET.ParquetEngine.OpenFileOrFolderAsync(path);

        }
    }

    [SkippableTestMethod]
    public async Task DECIMALS_AND_BOOLS_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/DECIMALS_AND_BOOLS_TEST.parquet");

        Assert.AreEqual(30, parquetEngine.RecordCount);
        Assert.HasCount(337, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        Assert.AreEqual((UInt16)156, dataTable.Rows[0][0]);
        Assert.AreEqual(60.7376101, dataTable.Rows[1][10]);
        Assert.IsFalse((bool)dataTable.Rows[19][332]);
        Assert.IsTrue((bool)dataTable.Rows[20][336]);
        Assert.AreEqual(DBNull.Value, dataTable.Rows[21][334]);
    }

    [SkippableTestMethod]
    public async Task DATETIME_TEST1()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/DATETIME_TEST1.parquet");

        Assert.AreEqual(10, parquetEngine.RecordCount);
        Assert.HasCount(3, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        Assert.AreEqual("36/2015-16", dataTable.Rows[0][0]);
        Assert.AreEqual(new DateOnly(2015, 07, 14), dataTable.Rows[1][2]);
        Assert.AreEqual(new DateTime(2015, 07, 19, 18, 30, 0), dataTable.Rows[9][1]);
    }

    [SkippableTestMethod]
    public async Task DATETIME_TEST2()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/DATETIME_TEST2.parquet");

        Assert.AreEqual(1, parquetEngine.RecordCount);
        Assert.HasCount(11, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        Assert.AreEqual((long)1, dataTable.Rows[0][0]);
        Assert.AreEqual(new DateOnly(1985, 12, 31), dataTable.Rows[0][1]);
        Assert.AreEqual(new DateOnly(1, 1, 2), dataTable.Rows[0][2]);
        Assert.AreEqual(new DateOnly(9999, 12, 31), dataTable.Rows[0][3]);
        Assert.AreEqual(new DateOnly(9999, 12, 31), dataTable.Rows[0][4]);
        Assert.AreEqual(new DateOnly(1, 1, 1), dataTable.Rows[0][5]);
        Assert.AreEqual(new DateTime(1985, 4, 13, 13, 5, 0), dataTable.Rows[0][6]);
        Assert.AreEqual(new DateTime(1, 1, 2, 0, 0, 0), dataTable.Rows[0][7]);
        Assert.AreEqual(new DateTime(9999, 12, 31, 23, 59, 59), dataTable.Rows[0][8]);
        Assert.AreEqual(new DateTime(3155378975999999990), dataTable.Rows[0][9]);
        Assert.AreEqual(new DateTime(1, 1, 1, 0, 0, 0), dataTable.Rows[0][10]);
    }

    [SkippableTestMethod]
    public async Task RANDOM_TEST_FILE_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/RANDOM_TEST_FILE.parquet");

        Assert.AreEqual(5, parquetEngine.RecordCount);
        Assert.HasCount(42, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        Assert.AreEqual((long)798921, dataTable.Rows[0][0]);
        Assert.AreEqual("BW15", dataTable.Rows[1][2]);
        Assert.AreEqual((long)7155937, dataTable.Rows[2][3]);
        Assert.AreEqual(DBNull.Value, dataTable.Rows[3][14]);
        Assert.AreEqual((double)22208120523, dataTable.Rows[4][25]);
        Assert.AreEqual("2022-08-10", dataTable.Rows[2][31]);
        Assert.AreEqual("DLIx12_SHIPCONF_BW15_20220812020138531.DWL", dataTable.Rows[1][41]);
    }

    [SkippableTestMethod]
    [SkipWhen(typeof(DuckDBEngineTests), "DuckDB automatically appends _1 to the dupe column name")]
    public async Task SAME_COLUMN_NAME_DIFFERENT_CASING_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/SAME_COLUMN_NAME_DIFFERENT_CASING.parquet");

        Assert.AreEqual(14610, parquetEngine.RecordCount);
        Assert.HasCount(12, parquetEngine.Fields);

        var ex = await Assert.ThrowsAsync<NotSupportedException>(async ()
            => (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false));
        Assert.AreEqual("Duplicate column `schema/TransPlan_NORMAL_v2` detected. Column names are case insensitive and must be unique.", ex.Message);
    }

    [SkippableTestMethod]
    public async Task MULTIPLE_SCHEMAS_DETECTED_TEST()
    {
        var ex = await Assert.ThrowsAsync<MultipleSchemasFoundException>(() => OpenFileOrFolderAsync("Data"));
        Assert.AreEqual("Multiple schemas found in directory.", ex.Message);
    }

    [SkippableTestMethod]
    public async Task PARTITIONED_PARQUET_FILE_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/PARTITIONED_PARQUET_FILE_TEST");

        Assert.AreEqual(2000, parquetEngine.RecordCount);
        Assert.HasCount(9, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        Assert.AreEqual("SHAPEFILE_JSON", dataTable.Rows[0][0]);
        Assert.AreEqual("5022121000", dataTable.Rows[200][2]);
        Assert.AreEqual((double)450, dataTable.Rows[500][3]);
        Assert.AreEqual("B000CTP5G2P2", dataTable.Rows[1999][8]);
        Assert.AreEqual("USA", dataTable.Rows[500][1]);

        dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 200, 1, default))(false);
        Assert.AreEqual("5022121000", dataTable.Rows[0][2]);

        dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 500, 1, default))(false);
        Assert.AreEqual((double)450, dataTable.Rows[0][3]);

        dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 1999, 1, default))(false);
        Assert.AreEqual("B000CTP5G2P2", dataTable.Rows[0][8]);
    }

    [SkippableTestMethod]
    public async Task COLUMN_ENDING_IN_PERIOD_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/COLUMN_ENDING_IN_PERIOD_TEST.parquet");

        Assert.AreEqual(1, parquetEngine.RecordCount);
        Assert.HasCount(11, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        Assert.AreEqual(202252, dataTable.Rows[0][0]);
        Assert.IsFalse((bool)dataTable.Rows[0]["Output as FP"]);
        Assert.AreEqual((byte)0, dataTable.Rows[0]["Preorder FP equi."]);
    }

    [SkippableTestMethod]
    [SkipWhen(typeof(DuckDBEngineTests), "DuckDB can't handle lists with null in them?")]
    public async Task LIST_TYPE_TEST1()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/LIST_TYPE_TEST1.parquet");

        Assert.AreEqual(3, parquetEngine.RecordCount);
        Assert.HasCount(2, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        Assert.IsInstanceOfType<IListValue>(dataTable.Rows[0][0]);
        Assert.AreEqual("[1,2,3]", dataTable.Rows[0][0].ToString());
        Assert.IsInstanceOfType<IListValue>(dataTable.Rows[0][1]);
        Assert.AreEqual(@"[""abc"",""efg"",""hij""]", dataTable.Rows[0][1].ToString());
        Assert.IsInstanceOfType<IListValue>(dataTable.Rows[1][0]);
        Assert.AreEqual("[null,1]", dataTable.Rows[1][0].ToString());
        Assert.IsInstanceOfType<IListValue>(dataTable.Rows[2][1]);
        Assert.HasCount(4, (IListValue)dataTable.Rows[2][1]);
        Assert.AreEqual("efg", ((IListValue)dataTable.Rows[2][1]).Data![0]);
        Assert.AreEqual(DBNull.Value, ((IListValue)dataTable.Rows[2][1]).Data![1]);
        Assert.AreEqual("xyz", ((IListValue)dataTable.Rows[2][1]).Data![3]);

        //Also try reading with a record offset
        dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 1, 1, default))(false);
        Assert.IsInstanceOfType<IListValue>(dataTable.Rows[0][0]);
        Assert.AreEqual("[null,1]", dataTable.Rows[0][0].ToString());
    }

    [SkippableTestMethod]
    public async Task LIST_TYPE_TEST2()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/LIST_TYPE_TEST2.parquet");

        Assert.AreEqual(8, parquetEngine.RecordCount);
        Assert.HasCount(2, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 2, 4, default))(false);
        Assert.IsInstanceOfType<IListValue>(dataTable.Rows[0][1]);

        Assert.AreEqual("[1,2]", dataTable.Rows[0][1].ToString());
        Assert.AreEqual(1, ((IListValue)dataTable.Rows[0][1]).Data[0]);
        Assert.AreEqual(2, ((IListValue)dataTable.Rows[0][1]).Data[1]);

        Assert.AreEqual(string.Empty, dataTable.Rows[1][1].ToString());
        Assert.AreEqual(DBNull.Value, dataTable.Rows[1][1]);

        Assert.AreEqual("[]", dataTable.Rows[2][1].ToString());
        Assert.IsEmpty(((IListValue)dataTable.Rows[2][1]).Data.Cast<dynamic>());

        Assert.AreEqual("[3,4]", dataTable.Rows[3][1].ToString());
        Assert.AreEqual(3, ((IListValue)dataTable.Rows[3][1]).Data[0]);
        Assert.AreEqual(4, ((IListValue)dataTable.Rows[3][1]).Data[1]);
    }

    [SkippableTestMethod]
    public async Task MAP_TYPE_TEST1()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/MAP_TYPE_TEST1.parquet");

        Assert.AreEqual(2, parquetEngine.RecordCount);
        Assert.HasCount(2, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, 2, default))(false);

        Assert.IsInstanceOfType<IMapValue>(dataTable.Rows[0][0]);
        var row = (IMapValue)dataTable.Rows[0][0];
        Assert.AreEqual("id", row.FirstOrDefault().Key);
        Assert.AreEqual("something", row.FirstOrDefault().Value);
        Assert.AreEqual("value2", row.Skip(1).FirstOrDefault().Key);
        Assert.AreEqual("else", row.Skip(1).FirstOrDefault().Value);
        Assert.AreEqual("[(id,something),(value2,else)]", row.ToString());

        Assert.IsInstanceOfType<IMapValue>(dataTable.Rows[1][0]);
        row = (IMapValue)dataTable.Rows[1][0];
        Assert.AreEqual("id", row.FirstOrDefault().Key);
        Assert.AreEqual("something2", row.FirstOrDefault().Value);
        Assert.AreEqual("value", row.Skip(1).FirstOrDefault().Key);
        Assert.AreEqual("else2", row.Skip(1).FirstOrDefault().Value);
        Assert.AreEqual("[(id,something2),(value,else2)]", row.ToString());
    }

    [SkippableTestMethod]
    public async Task MAP_TYPE_TEST2()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/MAP_TYPE_TEST2.parquet");

        Assert.AreEqual(8, parquetEngine.RecordCount);
        Assert.HasCount(2, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 2, 4, default))(false);
        Assert.IsInstanceOfType<IMapValue>(dataTable.Rows[0][1]);

        Assert.AreEqual("[(1,1),(2,2)]", dataTable.Rows[0][1].ToString());
        Assert.AreEqual(1, ((IMapValue)dataTable.Rows[0][1]).Keys[0]);
        Assert.AreEqual(1, ((IMapValue)dataTable.Rows[0][1]).Values[0]);
        Assert.AreEqual(2, ((IMapValue)dataTable.Rows[0][1]).Keys[1]);
        Assert.AreEqual(2, ((IMapValue)dataTable.Rows[0][1]).Values[1]);

        Assert.AreEqual(string.Empty, dataTable.Rows[1][1].ToString());
        Assert.AreEqual(DBNull.Value, dataTable.Rows[1][1]);

        Assert.AreEqual("[]", dataTable.Rows[2][1].ToString());
        Assert.IsEmpty(((IMapValue)dataTable.Rows[2][1]).Keys.Cast<dynamic>());
        Assert.IsEmpty(((IMapValue)dataTable.Rows[2][1]).Values.Cast<dynamic>());

        Assert.AreEqual("[(3,3),(4,4)]", dataTable.Rows[3][1].ToString());
        Assert.AreEqual(3, ((IMapValue)dataTable.Rows[3][1]).Keys[0]);
        Assert.AreEqual(3, ((IMapValue)dataTable.Rows[3][1]).Values[0]);
        Assert.AreEqual(4, ((IMapValue)dataTable.Rows[3][1]).Keys[1]);
        Assert.AreEqual(4, ((IMapValue)dataTable.Rows[3][1]).Values[1]);
    }

    [SkippableTestMethod]
    public async Task STRUCT_TYPE_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/STRUCT_TYPE_TEST.parquet");

        Assert.AreEqual(10, parquetEngine.RecordCount);
        Assert.HasCount(6, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        Assert.AreEqual(DBNull.Value, dataTable.Rows[0][0]);
        Assert.IsInstanceOfType<IStructValue>(dataTable.Rows[2][0]);
        Assert.AreEqual("{\"appId\":\"e4a20b59-dd0e-4c50-b074-e8ae4786df30\",\"version\":0,\"lastUpdated\":1564524299648}", dataTable.Rows[2][0].ToString());
        Assert.IsInstanceOfType<IStructValue>(dataTable.Rows[5][1]);
        Assert.AreEqual("{\"path\":\"part-00000-cb6b150b-30b8-4662-ad28-ff32ddab96d2-c000.snappy.parquet\",\"partitionValues\":[],\"size\":404,\"modificationTime\":1564524299000,\"dataChange\":false,\"stats\":null,\"tags\":null}", dataTable.Rows[5][1].ToString());
        Assert.IsInstanceOfType<IStructValue>(dataTable.Rows[6][2]);
        Assert.AreEqual("{\"path\":\"part-00001-185eca06-e017-4dea-ae49-fc48b973e37e-c000.snappy.parquet\",\"deletionTimestamp\":1564524298214,\"dataChange\":false}", dataTable.Rows[6][2].ToString());
        Assert.IsInstanceOfType<IStructValue>(dataTable.Rows[1][3]);
        Assert.AreEqual("{\"id\":\"22ef18ba-191c-4c36-a606-3dad5cdf3830\",\"name\":null,\"description\":null,\"format\":{\"provider\":\"parquet\",\"options\":[]},\"schemaString\":\"{\\\"type\\\":\\\"struct\\\",\\\"fields\\\":[{\\\"name\\\":\\\"value\\\",\\\"type\\\":\\\"integer\\\",\\\"nullable\\\":true,\\\"metadata\\\":{}}]}\",\"partitionColumns\":[],\"configuration\":[],\"createdTime\":1564524294376}", dataTable.Rows[1][3].ToString());
        Assert.IsInstanceOfType<IStructValue>(dataTable.Rows[0][4]);
        Assert.AreEqual("{\"minReaderVersion\":1,\"minWriterVersion\":2}", dataTable.Rows[0][4].ToString());
    }

    [SkippableTestMethod]
    public async Task STRUCT_FIELD_PROGRESS_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/STRUCT_TYPE_TEST.parquet");

        var expectedReportedProgress = parquetEngine.Fields.Count * parquetEngine.RecordCount;

        var progress = new ProgressRecorder();
        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, progress))(false);

        Assert.AreEqual(expectedReportedProgress, progress.Total,
            "Total reported progress did not equal the number of loaded cells. The loading bar would never reach 100%.");
    }

    [SkippableTestMethod]
    public async Task NULLABLE_GUID_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/NULLABLE_GUID_TEST.parquet");

        Assert.AreEqual(1, parquetEngine.RecordCount);
        Assert.HasCount(33, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        Assert.IsFalse((bool)dataTable.Rows[0][22]);
        Assert.AreEqual(new Guid("fdcbf90c-20d3-d745-b29f-9c2de1baa979"), dataTable.Rows[0][1]);
        Assert.AreEqual(new DateTime(2019, 1, 1), dataTable.Rows[0][4]);
    }

    [SkippableTestMethod]
    public async Task MALFORMED_DATETIME_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/MALFORMED_DATETIME_TEST.parquet");

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        Assert.IsInstanceOfType<DateTime>(dataTable.Rows[0]["ds"]);
        Assert.AreEqual(new DateTime(2017, 1, 1), dataTable.Rows[0]["ds"]);
    }

    [SkippableTestMethod]
    public async Task COLUMN_NAME_WITH_FORWARD_SLASH_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/COLUMN_NAME_WITH_FORWARD_SLASH.parquet");

        Assert.AreEqual(1, parquetEngine.RecordCount);
        Assert.HasCount(320, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, 1, default))(false);
        Assert.AreEqual((byte)0, dataTable.Rows[0]["FLC K/L"]);
    }

    [SkippableTestMethod]
    public async Task ORACLE_MALFORMED_INT64_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/ORACLE_MALFORMED_INT64_TEST.parquet");

        Assert.AreEqual(126, parquetEngine.RecordCount);
        Assert.HasCount(2, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        Assert.AreEqual("DEPOSIT", dataTable.Rows[0][0]);
        Assert.AreEqual((long)1, dataTable.Rows[0][1]);
    }

    [SkippableTestMethod]
    public async Task LIST_OF_STRUCTS_TEST1()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/LIST_OF_STRUCTS1.parquet");
        Assert.AreEqual(2, parquetEngine.RecordCount);
        Assert.HasCount(2, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, 2, default))(false);

        Assert.AreEqual("Product1", dataTable.Rows[0][0]);
        Assert.AreEqual("Product2", dataTable.Rows[1][0]);

        Assert.IsInstanceOfType<IListValue>(dataTable.Rows[0][1]);
        Assert.AreEqual("[{\"DateTime\":\"2024-04-15 22:00:00\",\"Quantity\":10},{\"DateTime\":\"2024-04-16 22:00:00\",\"Quantity\":20}]", dataTable.Rows[0][1].ToString());
        Assert.IsInstanceOfType<IListValue>(dataTable.Rows[1][1]);
        Assert.AreEqual("[{\"DateTime\":\"2024-04-15 22:00:00\",\"Quantity\":30},{\"DateTime\":\"2024-04-16 22:00:00\",\"Quantity\":40}]", dataTable.Rows[1][1].ToString());
    }

    [SkippableTestMethod]
    public async Task LIST_OF_STRUCTS_TEST2()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/LIST_OF_STRUCTS2.parquet");
        Assert.AreEqual(1, parquetEngine.RecordCount);
        Assert.HasCount(29, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);

        Assert.IsInstanceOfType<IListValue>(dataTable.Rows[0][28]);
        Assert.AreEqual("[{\"purposeId\":\"HF85PyyGFprJXJvh5Pk9tg\",\"status\":\"Granted\",\"externalId\":\"General\",\"date\":\"2025-06-05 14:30:33\"}]", dataTable.Rows[0][28].ToString());
    }

    [SkippableTestMethod]
    public async Task EMPTY_LIST_OF_STRUCTS_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/EMPTY_LIST_OF_STRUCTS.parquet");
        Assert.AreEqual(2, parquetEngine.RecordCount);
        Assert.HasCount(2, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);

        Assert.AreEqual("Product1", dataTable.Rows[0][0]);
        Assert.AreEqual("Product2", dataTable.Rows[1][0]);

        Assert.IsInstanceOfType<IListValue>(dataTable.Rows[0][1]);
        Assert.AreEqual("[]", dataTable.Rows[0][1].ToString());
        Assert.IsInstanceOfType<IListValue>(dataTable.Rows[1][1]);
        Assert.AreEqual("[]", dataTable.Rows[1][1].ToString());
    }

    [SkippableTestMethod]
    public async Task PARQUET_MR_BREAKING_CHANGE_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/PARQUET-MR_1.15.0.parquet");
        Assert.AreEqual(5, parquetEngine.RecordCount);
        Assert.HasCount(7, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);

        Assert.AreEqual(1, dataTable.Rows[0][0]);
        Assert.AreEqual(5, dataTable.Rows[4][0]);

        Assert.AreEqual("John Doe", dataTable.Rows[0][1]);
        Assert.AreEqual("David Lee", dataTable.Rows[4][1]);

        Assert.IsTrue((bool)dataTable.Rows[0][4]);
        Assert.IsTrue((bool)dataTable.Rows[4][4]);
    }

    [SkippableTestMethod]
    [SkipWhen(typeof(DuckDBEngineTests), "DuckDB can't open this file")]
    public async Task DECIMALS_WITH_NO_SCALE_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/DECIMALS_WITH_NO_SCALE_TEST.parquet");
        Assert.AreEqual(10589, parquetEngine.RecordCount);
        Assert.HasCount(8, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);

        Assert.AreEqual(0.7072m, dataTable.Rows[0][5]);
        Assert.AreEqual(0m, dataTable.Rows[0][6]);
        Assert.AreEqual(0m, dataTable.Rows[0][7]);

        Assert.AreEqual(0.74527m, dataTable.Rows[100][5]);
        Assert.AreEqual(0m, dataTable.Rows[100][6]);
        Assert.AreEqual(0m, dataTable.Rows[100][7]);
    }

    [SkippableTestMethod]
    public async Task LIST_OF_LIST_OF_INT()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/LIST_OF_LIST_OF_INT.parquet");
        Assert.AreEqual(3, parquetEngine.RecordCount);
        Assert.HasCount(1, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        Assert.AreEqual("[[1],[1,2],[1,2,3],[1,2,3,4],[1,2,3,4,5]]", dataTable.Rows[0][0].ToString());
        Assert.AreEqual(DBNull.Value, dataTable.Rows[1][0]);
        Assert.AreEqual("[[1],[],[3],null,[5]]", dataTable.Rows[2][0].ToString());

        dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 1, 1, default))(false);
        Assert.AreEqual(DBNull.Value, dataTable.Rows[0][0]);

        dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 2, 1, default))(false);
        Assert.AreEqual("[[1],[],[3],null,[5]]", dataTable.Rows[0][0].ToString());
    }

    [SkippableTestMethod]
    public async Task LIST_OF_LIST_OF_LIST_OF_STRING()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/LIST_OF_LIST_OF_LIST_OF_STRING.parquet");
        Assert.AreEqual(3, parquetEngine.RecordCount);
        Assert.HasCount(2, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        Assert.AreEqual("[[[\"a\",\"b\"],[\"c\"]],[null,[\"d\"]]]", dataTable.Rows[0][0].ToString());
        Assert.AreEqual("[[[\"a\",\"b\"],[\"c\",\"d\"]],[null,[\"e\"]]]", dataTable.Rows[1][0].ToString());
        Assert.AreEqual("[[[\"a\",\"b\"],[\"c\",\"d\"],[\"e\"]],[null,[\"f\"]]]", dataTable.Rows[2][0].ToString());

        dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 1, 1, default))(false);
        Assert.AreEqual("[[[\"a\",\"b\"],[\"c\",\"d\"]],[null,[\"e\"]]]", dataTable.Rows[0][0].ToString());

        dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 2, 1, default))(false);
        Assert.AreEqual("[[[\"a\",\"b\"],[\"c\",\"d\"],[\"e\"]],[null,[\"f\"]]]", dataTable.Rows[0][0].ToString());
    }

    [SkippableTestMethod]
    public async Task NESTED_LIST_OF_UINTS()
    {
        //Regression test: `uint[]` was being matched by the `int[]` in ParquetColumnData (the CLR
        //treats signed/unsigned integer arrays of the same width as compatible), so nested uint lists were
        //read as `int` and threw an ArrayTypeMismatchException when building the ListValue.
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/NESTED_LIST_OF_UINTS.parquet");

        Assert.AreEqual(169, parquetEngine.RecordCount);
        Assert.HasCount(6, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);

        Assert.AreEqual((UInt32)0, dataTable.Rows[0][0]);
        Assert.AreEqual((UInt32)814, dataTable.Rows[0][3]);
        Assert.AreEqual((UInt32)576, dataTable.Rows[0][4]);
        Assert.AreEqual("LuCa-7color_[13860,52919]_1x1component_data", dataTable.Rows[0][5]);
        Assert.AreEqual("[[807,584],[805,581],[806,578],[805,575],[807,572],[810,571],[814,571],[818,571],[823,571],[822,574],[821,577],[819,579],[815,580],[812,581],[808,581],[807,584]]",
            dataTable.Rows[0][1].ToString());
        Assert.AreEqual("[[807,588],[805,584],[805,578],[805,572],[809,571],[815,571],[821,571],[827,571],[831,572],[830,577],[828,581],[823,582],[818,582],[813,583],[809,585],[807,588]]",
            dataTable.Rows[0][2].ToString());

        Assert.AreEqual((UInt32)100, dataTable.Rows[100][0]);
        Assert.AreEqual((UInt32)1022, dataTable.Rows[100][3]);
        Assert.AreEqual((UInt32)716, dataTable.Rows[100][4]);

        Assert.AreEqual((UInt32)168, dataTable.Rows[168][0]);
        Assert.AreEqual("[[1059,827],[1055,827],[1052,827],[1050,826],[1048,824],[1047,822],[1046,820],[1046,816],[1048,815],[1052,815],[1054,816],[1056,817],[1058,819],[1059,821],[1059,824],[1059,827]]",
            dataTable.Rows[168][1].ToString());

        var offset = Random.Shared.Next(0, (int)parquetEngine.RecordCount);
        var recordCount = Random.Shared.Next(1, (int)parquetEngine.RecordCount - offset + 1);
        var slice = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, offset, recordCount, default))(false);

        Assert.HasCount(recordCount, slice.Rows);
        for (var i = 0; i < recordCount; i++)
        {
            Assert.AreEqual(dataTable.Rows[offset + i][0], slice.Rows[i][0]);
            Assert.AreEqual(dataTable.Rows[offset + i][1].ToString(), slice.Rows[i][1].ToString());
        }
    }

    [SkippableTestMethod]
    public async Task LIST_OF_STRUCT_OF_LIST_OF_STRUCT()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/LIST_OF_STRUCT_OF_LIST_OF_STRUCT.parquet");
        Assert.AreEqual(1, parquetEngine.RecordCount);
        Assert.HasCount(1, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        const string expectedJson = @"[{""Labels"":[{""Name"":""PROFANITY"",""Score"":0.046799998730421066},{""Name"":""HATE_SPEECH"",""Score"":0.07109999656677246},{""Name"":""INSULT"",""Score"":0.11330000311136246},{""Name"":""GRAPHIC"",""Score"":0.01860000006854534},{""Name"":""HARASSMENT_OR_ABUSE"",""Score"":0.06069999933242798},{""Name"":""SEXUAL"",""Score"":0.07109999656677246},{""Name"":""VIOLENCE_OR_THREAT"",""Score"":0.026499999687075615}],""Toxicity"":0.08389999717473984},{""Labels"":[{""Name"":""PROFANITY"",""Score"":0.07029999792575836},{""Name"":""HATE_SPEECH"",""Score"":0.08839999884366989},{""Name"":""INSULT"",""Score"":0.13269999623298645},{""Name"":""GRAPHIC"",""Score"":0.01860000006854534},{""Name"":""HARASSMENT_OR_ABUSE"",""Score"":0.023900000378489494},{""Name"":""SEXUAL"",""Score"":0.07109999656677246},{""Name"":""VIOLENCE_OR_THREAT"",""Score"":0.026499999687075615}],""Toxicity"":0.09790000319480896},{""Labels"":[{""Name"":""PROFANITY"",""Score"":0.046799998730421066},{""Name"":""HATE_SPEECH"",""Score"":0.07970000058412552},{""Name"":""INSULT"",""Score"":0.11749999970197678},{""Name"":""GRAPHIC"",""Score"":0.019500000402331352},{""Name"":""HARASSMENT_OR_ABUSE"",""Score"":0.06069999933242798},{""Name"":""SEXUAL"",""Score"":0.14339999854564667},{""Name"":""VIOLENCE_OR_THREAT"",""Score"":0.027699999511241913}],""Toxicity"":0.10249999910593033},{""Labels"":[{""Name"":""PROFANITY"",""Score"":0.06040000170469284},{""Name"":""HATE_SPEECH"",""Score"":0.08749999850988388},{""Name"":""INSULT"",""Score"":0.1273999959230423},{""Name"":""GRAPHIC"",""Score"":0.019500000402331352},{""Name"":""HARASSMENT_OR_ABUSE"",""Score"":0.06069999933242798},{""Name"":""SEXUAL"",""Score"":0.14219999313354492},{""Name"":""VIOLENCE_OR_THREAT"",""Score"":0.026499999687075615}],""Toxicity"":0.10809999704360962},{""Labels"":[{""Name"":""PROFANITY"",""Score"":0.04780000075697899},{""Name"":""HATE_SPEECH"",""Score"":0.07970000058412552},{""Name"":""INSULT"",""Score"":0.12039999663829803},{""Name"":""GRAPHIC"",""Score"":0.019500000402331352},{""Name"":""HARASSMENT_OR_ABUSE"",""Score"":0.06069999933242798},{""Name"":""SEXUAL"",""Score"":0.1331000030040741},{""Name"":""VIOLENCE_OR_THREAT"",""Score"":0.026499999687075615}],""Toxicity"":0.0949999988079071},{""Labels"":[{""Name"":""PROFANITY"",""Score"":0.046799998730421066},{""Name"":""HATE_SPEECH"",""Score"":0.07109999656677246},{""Name"":""INSULT"",""Score"":0.11969999969005585},{""Name"":""GRAPHIC"",""Score"":0.019500000402331352},{""Name"":""HARASSMENT_OR_ABUSE"",""Score"":0.06069999933242798},{""Name"":""SEXUAL"",""Score"":0.13009999692440033},{""Name"":""VIOLENCE_OR_THREAT"",""Score"":0.026499999687075615}],""Toxicity"":0.08969999849796295},{""Labels"":[{""Name"":""PROFANITY"",""Score"":0.06939999759197235},{""Name"":""HATE_SPEECH"",""Score"":0.07859999686479568},{""Name"":""INSULT"",""Score"":0.12219999730587006},{""Name"":""GRAPHIC"",""Score"":0.019500000402331352},{""Name"":""HARASSMENT_OR_ABUSE"",""Score"":0.06069999933242798},{""Name"":""SEXUAL"",""Score"":0.14630000293254852},{""Name"":""VIOLENCE_OR_THREAT"",""Score"":0.027699999511241913}],""Toxicity"":0.08969999849796295},{""Labels"":[{""Name"":""PROFANITY"",""Score"":0.06040000170469284},{""Name"":""HATE_SPEECH"",""Score"":0.07029999792575836},{""Name"":""INSULT"",""Score"":0.10249999910593033},{""Name"":""GRAPHIC"",""Score"":0.019500000402331352},{""Name"":""HARASSMENT_OR_ABUSE"",""Score"":0.06069999933242798},{""Name"":""SEXUAL"",""Score"":0.18400000035762787},{""Name"":""VIOLENCE_OR_THREAT"",""Score"":0.026499999687075615}],""Toxicity"":0.07419999688863754}]";

        Assert.AreEqual(expectedJson, dataTable.Rows[0][0].ToString());
    }

    [SkippableTestMethod]
    public async Task LIST_OF_STRUCT_OF_LIST_OF_STRUCT_WITH_NULLS()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/LIST_OF_STRUCT_OF_LIST_OF_STRUCT_WITH_NULLS.parquet");
        Assert.AreEqual(2, parquetEngine.RecordCount);
        Assert.HasCount(1, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);

        Assert.AreEqual(
            @"[{""A"":[{""B"":[1,2],""C"":3,""D"":4}],""E"":0.1},{""A"":[],""E"":0.2}]",
            dataTable.Rows[0][0].ToString());
        Assert.AreEqual(
            @"[{""A"":null,""E"":0.3},{""A"":[{""B"":[],""C"":5,""D"":6},{""B"":[7],""C"":null,""D"":8}],""E"":0.4}]",
            dataTable.Rows[1][0].ToString());
    }

    [SkippableTestMethod]
    public async Task LIST_OF_STRUCT_OF_LIST_OF_STRUCT_NULL_ELEMENTS()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/LIST_OF_STRUCT_OF_LIST_OF_STRUCT_NULL_ELEMENTS.parquet");
        Assert.AreEqual(7, parquetEngine.RecordCount);
        Assert.HasCount(1, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);

        Assert.AreEqual(@"[{""A"":[{""B"":null,""C"":null,""D"":null}],""E"":0.1}]", dataTable.Rows[0][0].ToString());
        Assert.AreEqual(@"[{""A"":[{""B"":[],""C"":null,""D"":null}],""E"":0.2}]", dataTable.Rows[1][0].ToString());
        Assert.AreEqual(@"[{""A"":[{""B"":null,""C"":null,""D"":null},{""B"":[1,2],""C"":3,""D"":4}],""E"":0.3}]", dataTable.Rows[2][0].ToString());
        Assert.AreEqual(@"[{""A"":null,""E"":0.4}]", dataTable.Rows[3][0].ToString());
        Assert.AreEqual(@"[{""A"":[],""E"":0.5}]", dataTable.Rows[4][0].ToString());
        Assert.AreEqual(@"[{""A"":[{""B"":null,""C"":null,""D"":null}],""E"":0.6},{""A"":[{""B"":[9],""C"":8,""D"":7}],""E"":0.7}]", dataTable.Rows[5][0].ToString());
        Assert.AreEqual(@"[{""A"":[null,{""B"":null,""C"":null,""D"":null}],""E"":0.8}]", dataTable.Rows[6][0].ToString());
    }

    [SkippableTestMethod]
    public async Task LIST_OF_STRUCT_NULL_NESTED_LIST_AND_PRIMITIVE()
    {
        //Regression test: a null nested list in the first column of a struct used to crash the
        //pivot with an ArgumentOutOfRangeException when a later column had a value.
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/LIST_OF_STRUCT_NULL_NESTED_LIST_AND_PRIMITIVE.parquet");
        Assert.AreEqual(1, parquetEngine.RecordCount);
        Assert.HasCount(1, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        Assert.AreEqual(@"[{""A"":null,""E"":1}]", dataTable.Rows[0][0].ToString());
    }

    /// <summary>
    /// A LIST whose repeated node is the element itself because it isn't a group (an older writer layout):
    /// <c>optional group tags (LIST) { repeated int32 array; }</c>. Written by hand, pyarrow reads it as list&lt;int&gt;.
    /// </summary>
    [SkippableTestMethod]
    [SkipWhen(typeof(DuckDBEngineTests), "Our DuckDB engine can't read legacy two-level lists")]
    public async Task LEGACY_TWO_LEVEL_LIST_OF_PRIMITIVES_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/LEGACY_TWO_LEVEL_LIST_OF_PRIMITIVES_TEST.parquet");
        Assert.AreEqual(4, parquetEngine.RecordCount);
        Assert.HasCount(2, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        Assert.AreEqual(0, dataTable.Rows[0]["id"]);
        Assert.AreEqual("[1,2]", dataTable.Rows[0]["tags"].ToString());
        Assert.AreEqual(1, dataTable.Rows[1]["id"]);
        Assert.AreEqual(DBNull.Value, dataTable.Rows[1]["tags"]);
        Assert.AreEqual(2, dataTable.Rows[2]["id"]);
        Assert.AreEqual("[]", dataTable.Rows[2]["tags"].ToString());
        Assert.AreEqual(3, dataTable.Rows[3]["id"]);
        Assert.AreEqual("[3]", dataTable.Rows[3]["tags"].ToString());
    }

    /// <summary>
    /// A LIST whose repeated node is the element itself because it's a single field group named `array` (an older writer layout):
    /// <c>optional group tags (LIST) { repeated group array { optional int32 b; } }</c>. Written by hand, pyarrow reads it as
    /// list&lt;struct&lt;b&gt;&gt;. Without the legacy rule the struct is lost and it's read as list&lt;int&gt;.
    /// </summary>
    [SkippableTestMethod]
    [SkipWhen(typeof(DuckDBEngineTests), "Our DuckDB engine can't read legacy two-level lists")]
    public async Task LEGACY_TWO_LEVEL_LIST_OF_STRUCTS_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/LEGACY_TWO_LEVEL_LIST_OF_STRUCTS_TEST.parquet");
        Assert.AreEqual(4, parquetEngine.RecordCount);
        Assert.HasCount(2, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        Assert.AreEqual(0, dataTable.Rows[0]["id"]);
        Assert.AreEqual("[{\"b\":10},{\"b\":null}]", dataTable.Rows[0]["tags"].ToString());
        Assert.AreEqual(1, dataTable.Rows[1]["id"]);
        Assert.AreEqual(DBNull.Value, dataTable.Rows[1]["tags"]);
        Assert.AreEqual(2, dataTable.Rows[2]["id"]);
        Assert.AreEqual("[]", dataTable.Rows[2]["tags"].ToString());
        Assert.AreEqual(3, dataTable.Rows[3]["id"]);
        Assert.AreEqual("[{\"b\":30}]", dataTable.Rows[3]["tags"].ToString());
    }

    [SkippableTestMethod]
    public async Task TWO_TIER_REPEATED_LIST_FIELDS_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/TWO_TIER_TEPEATED_LIST_FIELDS_TEST.parquet");
        Assert.AreEqual(1, parquetEngine.RecordCount);
        Assert.HasCount(8, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);

        Assert.AreEqual(32, dataTable.Rows[0][0]);
        Assert.AreEqual((long)64, dataTable.Rows[0][1]);
        Assert.AreEqual(DBNull.Value, dataTable.Rows[0][2]);
        Assert.AreEqual("hello", dataTable.Rows[0][3]);
        Assert.AreEqual("[10,20]", dataTable.Rows[0][4].ToString());
        if (_treatsTwoTierListAsStruct)
            Assert.AreEqual("[{\"nested\":\"nested!\"}]", dataTable.Rows[0][5].ToString());
        else
            Assert.AreEqual(@"[""nested!""]", dataTable.Rows[0][5].ToString());

        Assert.AreEqual("096d06d7-e00b-4f70-ad5c-ca4da9a9630a", dataTable.Rows[0][6]);
        Assert.AreEqual("[\"element1\",\"element2\"]", dataTable.Rows[0][7].ToString());
    }

    [SkippableTestMethod]
    public async Task CUSTOM_METADATA_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/LIST_TYPE_TEST1.parquet");

        Assert.Contains("pandas", parquetEngine.CustomMetadata.Keys);
        const string expectedPandas = "{\"index_columns\":[{\"kind\":\"range\",\"name\":null,\"start\":0,\"stop\":3,\"step\":1}],\"column_indexes\":[{\"name\":null,\"field_name\":null,\"pandas_type\":\"unicode\",\"numpy_type\":\"object\",\"metadata\":{\"encoding\":\"UTF-8\"}}],\"columns\":[{\"name\":\"int64_list\",\"field_name\":\"int64_list\",\"pandas_type\":\"list[int64]\",\"numpy_type\":\"object\",\"metadata\":null},{\"name\":\"utf8_list\",\"field_name\":\"utf8_list\",\"pandas_type\":\"list[unicode]\",\"numpy_type\":\"object\",\"metadata\":null}],\"creator\":{\"library\":\"pyarrow\",\"version\":\"0.15.1\"},\"pandas_version\":\"0.25.3\"}";
        Assert.AreEqual(TryFormatJSON(expectedPandas), TryFormatJSON(parquetEngine.CustomMetadata["pandas"]));

        Assert.Contains("ARROW:schema", parquetEngine.CustomMetadata.Keys);
        const string expectedArrow = "/////4ADAAAQAAAAAAAKAA4ABgAFAAgACgAAAAABAwAQAAAAAAAKAAwAAAAEAAgACgAAAHQCAAAEAAAAAQAAAAwAAAAIAAwABAAIAAgAAABMAgAABAAAADwCAAB7ImluZGV4X2NvbHVtbnMiOiBbeyJraW5kIjogInJhbmdlIiwgIm5hbWUiOiBudWxsLCAic3RhcnQiOiAwLCAic3RvcCI6IDMsICJzdGVwIjogMX1dLCAiY29sdW1uX2luZGV4ZXMiOiBbeyJuYW1lIjogbnVsbCwgImZpZWxkX25hbWUiOiBudWxsLCAicGFuZGFzX3R5cGUiOiAidW5pY29kZSIsICJudW1weV90eXBlIjogIm9iamVjdCIsICJtZXRhZGF0YSI6IHsiZW5jb2RpbmciOiAiVVRGLTgifX1dLCAiY29sdW1ucyI6IFt7Im5hbWUiOiAiaW50NjRfbGlzdCIsICJmaWVsZF9uYW1lIjogImludDY0X2xpc3QiLCAicGFuZGFzX3R5cGUiOiAibGlzdFtpbnQ2NF0iLCAibnVtcHlfdHlwZSI6ICJvYmplY3QiLCAibWV0YWRhdGEiOiBudWxsfSwgeyJuYW1lIjogInV0ZjhfbGlzdCIsICJmaWVsZF9uYW1lIjogInV0ZjhfbGlzdCIsICJwYW5kYXNfdHlwZSI6ICJsaXN0W3VuaWNvZGVdIiwgIm51bXB5X3R5cGUiOiAib2JqZWN0IiwgIm1ldGFkYXRhIjogbnVsbH1dLCAiY3JlYXRvciI6IHsibGlicmFyeSI6ICJweWFycm93IiwgInZlcnNpb24iOiAiMC4xNS4xIn0sICJwYW5kYXNfdmVyc2lvbiI6ICIwLjI1LjMifQAAAAAGAAAAcGFuZGFzAAACAAAAYAAAAAQAAACE////AAABDEAAAAAQAAAABAAAAAEAAAAIAAAAqP///6T///8AAAEFFAAAAAwAAAAEAAAAAAAAAMT///8EAAAAaXRlbQAAAAAJAAAAdXRmOF9saXN0AAAA3P///wAAAQxkAAAAFAAAAAQAAAABAAAAHAAAAAQABAAEAAAAEAAUAAgABgAHAAwAAAAQABAAAAAAAAECJAAAABQAAAAEAAAAAAAAAAgADAAIAAcACAAAAAAAAAFAAAAABAAAAGl0ZW0AAAAACgAAAGludDY0X2xpc3QAAA==";
        Assert.AreEqual(expectedArrow, parquetEngine.CustomMetadata["ARROW:schema"]);
    }

    [SkippableTestMethod]
    public async Task DECIMALS_OUTOFRANGE_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/DECIMALS_OUTOFRANGE_TEST.parquet");
        Assert.AreEqual(12, parquetEngine.RecordCount);
        Assert.HasCount(51, parquetEngine.Fields);

        await Assert.ThrowsAsync<DecimalOverflowException>(() =>
            parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue));
    }

    [SkippableTestMethod]
    public async Task LIST_OF_NESTED_STRUCTS_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/LIST_OF_NESTED_STRUCTS_TEST.parquet");
        Assert.AreEqual(1, parquetEngine.RecordCount);
        Assert.HasCount(1, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);

        Assert.AreEqual("[{\"B\":{\"id\":1}},{\"B\":{\"id\":null}},{\"B\":null}]", dataTable.Rows[0][0].ToString());
    }

    [SkippableTestMethod]
    public async Task NESTED_MAPS_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/NESTED_MAPS_TEST.parquet");
        Assert.AreEqual(6, parquetEngine.RecordCount);
        Assert.HasCount(3, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, 3, default))(false);

        Assert.AreEqual("[(a,[(1,True),(2,False)])]", dataTable.Rows[0][0].ToString());
        Assert.AreEqual("[(b,[(1,True)])]", dataTable.Rows[1][0].ToString());
        Assert.AreEqual("[(c,)]", dataTable.Rows[2][0].ToString());

        dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 3, 3, default))(false);

        Assert.AreEqual("[(d,[])]", dataTable.Rows[0][0].ToString());
        Assert.AreEqual("[(e,[(1,True)])]", dataTable.Rows[1][0].ToString());
        Assert.AreEqual("[(f,[(3,True),(4,False),(5,True)])]", dataTable.Rows[2][0].ToString());
    }

    private static string TryFormatJSON(string possibleJSON)
    {
        try
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(possibleJSON);
#pragma warning disable CA1869 // Cache and reuse 'JsonSerializerOptions' instances
            return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions { WriteIndented = true });
#pragma warning restore CA1869 // This is a test class, not important
        }
        catch (Exception)
        {
            //malformed json detected
            return possibleJSON;
        }
    }

    [SkippableTestMethod]
    public async Task BYTEARRAY_VALUE_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/BYTEARRAY_VALUE_TEST.parquet");
        Assert.AreEqual(1, parquetEngine.RecordCount);
        Assert.HasCount(1, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        Assert.IsInstanceOfType<IByteArrayValue>(dataTable.Rows[0][0]);

        const string expected = "67-33-73-68-61-72-70-5F-73-74-6C-20-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00";
        Assert.AreEqual(expected, dataTable.Rows[0][0].ToString());
    }

    /// <summary>
    /// Raw FIXED_LEN_BYTE_ARRAY columns that aren't 16 bytes long (so they can't be read as Guids)
    /// </summary>
    [SkippableTestMethod]
    public async Task FIXED_LEN_BYTE_ARRAY_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/FIXED_LEN_BYTE_ARRAY_TEST.parquet");
        Assert.AreEqual(3, parquetEngine.RecordCount);
        Assert.HasCount(3, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        Assert.IsInstanceOfType<IByteArrayValue>(dataTable.Rows[0]["hash"]);
        Assert.AreEqual("00-01-02-03-04-05-06-07-08-09-0A-0B-0C-0D-0E-0F-10-11-12-13", dataTable.Rows[0]["hash"].ToString());
        Assert.AreEqual(DBNull.Value, dataTable.Rows[1]["hash"]);

        Assert.IsInstanceOfType<IByteArrayValue>(dataTable.Rows[1]["code"]);
        Assert.AreEqual("AA-BB-CC-DD", dataTable.Rows[1]["code"].ToString());
    }

    /// <summary>
    /// The outer LIST group must be optional or required according to the Parquet spec. This file was written by hand
    /// (pyarrow refuses to write or read it) with a top-level `repeated group my_list (LIST)`.
    /// </summary>
    [SkippableTestMethod]
    [SkipWhen(typeof(DuckDBEngineTests), "DuckDB reads repeated LIST groups as a list of lists, which our DuckDB engine doesn't handle")]
    public async Task REPEATED_LIST_GROUP_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/REPEATED_LIST_GROUP_TEST.parquet");
        Assert.AreEqual(3, parquetEngine.RecordCount);
        Assert.HasCount(2, parquetEngine.Fields);

        var ex = await Assert.ThrowsAsync<MalformedFieldException>(async ()
            => (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false));
        Assert.Contains("`my_list`", ex.Message);
    }

    /// <summary>
    /// The outer MAP group must be optional or required according to the Parquet spec. This file was written by hand
    /// (pyarrow refuses to write or read it) with a top-level `repeated group my_map (MAP)`.
    /// </summary>
    [SkippableTestMethod]
    [SkipWhen(typeof(DuckDBEngineTests), "DuckDB reads repeated MAP groups as a list of maps, which our DuckDB engine doesn't handle")]
    public async Task REPEATED_MAP_GROUP_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/REPEATED_MAP_GROUP_TEST.parquet");
        Assert.AreEqual(3, parquetEngine.RecordCount);
        Assert.HasCount(2, parquetEngine.Fields);

        var ex = await Assert.ThrowsAsync<MalformedFieldException>(async ()
            => (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false));
        Assert.Contains("`my_map`", ex.Message);
    }

    [SkippableTestMethod]
    public async Task NESTED_STRUCTS_AND_LISTS()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/NESTED_STRUCTS_AND_LISTS.parquet");
        Assert.AreEqual(552, parquetEngine.RecordCount);
        Assert.HasCount(20, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, 1, default))(false);

        Assert.IsInstanceOfType<IListValue>(dataTable.Rows[0][1]);
        Assert.AreEqual("[{\"explicit\":null,\"ref_reco\":3,\"text\":\"it is not the case that routine child vaccinations should be mandatory.\"}]", dataTable.Rows[0][1].ToString());

        Assert.IsInstanceOfType<IListValue>(dataTable.Rows[0][11]);
        Assert.AreEqual("[[\"p\",\"routine child vaccinations, or their side effects, are dangerous\"],[\"q\",\"routine child vaccinations should be mandatory\"]]", dataTable.Rows[0][11].ToString());

        Assert.IsInstanceOfType<IListValue>(dataTable.Rows[0][19]);
        Assert.AreEqual("[[\"id\",\"argkp_1feffc6a-01eb-4f64-a42f-db898627fbc8\"]]", dataTable.Rows[0][19].ToString());
    }

    [SkippableTestMethod]
    public async Task METADATA_TEST1()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/NESTED_STRUCTS_AND_LISTS.parquet");
        Assert.AreEqual(1, parquetEngine.Metadata.ParquetVersion);
        Assert.AreEqual(552, parquetEngine.Metadata.RowCount);
        Assert.AreEqual(1, parquetEngine.Metadata.RowGroupCount);
        Assert.AreEqual("parquet-cpp-arrow version 4.0.1", parquetEngine.Metadata.CreatedBy);
        Assert.HasCount(1, parquetEngine.Metadata.RowGroups);
        var rowGroup = parquetEngine.Metadata.RowGroups.First();
        Assert.AreEqual(33, rowGroup.ColumnCount);
        Assert.AreEqual(552, rowGroup.RowCount);
        Assert.AreEqual(2704, rowGroup.FileOffset);
        Assert.AreEqual(0, rowGroup.Ordinal);
        Assert.AreEqual(134465, rowGroup.TotalByteSize);
        Assert.AreEqual(61314, rowGroup.TotalCompressedSize);

        Assert.IsNotNull(rowGroup.Columns);
        Assert.HasCount(33, rowGroup.Columns);

        var firstColumn = rowGroup.Columns.First();
        Assert.IsNull(firstColumn.BloomFilterLength);
        Assert.IsNull(firstColumn.BloomFilterOffset);
        Assert.AreEqual(0, firstColumn.ColumnId);
        Assert.AreEqual(1801, firstColumn.DataPageOffset);
        Assert.AreEqual(4, firstColumn.DictionaryPageOffset);
        Assert.IsNull(firstColumn.IndexPageOffset);
        Assert.AreEqual(552, firstColumn.NumValues);
        Assert.AreEqual("argdown_reconstruction", firstColumn.PathInSchema);
        Assert.AreEqual(2700, firstColumn.TotalCompressedSize);
        Assert.AreEqual(10114, firstColumn.TotalUncompressedSize);
        Assert.AreEqual("BYTE_ARRAY", firstColumn.Type);

        Assert.IsNotNull(firstColumn.Statistics);
        Assert.IsNull(firstColumn.Statistics.Min);
        Assert.IsNull(firstColumn.Statistics.Max);
        Assert.IsNull(firstColumn.Statistics.DistinctCount);
        Assert.AreEqual(0, firstColumn.Statistics.NullCount);
        Assert.AreEqual("(1) child vaccination saves lives. (2) if child vaccination saves lives then routine child vaccinations should be mandatory. -- with modus ponens from (1) (2) -- (3) routine child vaccinations should be mandatory.", firstColumn.Statistics.MinValue);
        Assert.AreEqual("(1) the us offers great opportunities for individuals. (2) if the us offers great opportunities for individuals then the usa is a good country to live in. -- with modus ponens from (1) (2) -- (3) the usa is a good country to live in.", firstColumn.Statistics.MaxValue);
        Assert.IsNull(firstColumn.Statistics.IsMinValueExact);
        Assert.IsNull(firstColumn.Statistics.IsMinValueExact);

        var lastColumn = rowGroup.Columns.Last();
        Assert.IsNull(lastColumn.BloomFilterLength);
        Assert.IsNull(lastColumn.BloomFilterOffset);
        Assert.AreEqual(32, lastColumn.ColumnId);
        Assert.AreEqual(63771, lastColumn.DataPageOffset);
        Assert.AreEqual(43433, lastColumn.DictionaryPageOffset);
        Assert.IsNull(lastColumn.IndexPageOffset);
        Assert.AreEqual(1104, lastColumn.NumValues);
        Assert.AreEqual($"metadata{_schemaPathSeperator}list{_schemaPathSeperator}item{_schemaPathSeperator}list{_schemaPathSeperator}item", lastColumn.PathInSchema);
        Assert.AreEqual(21830, lastColumn.TotalCompressedSize);
        Assert.AreEqual(27163, lastColumn.TotalUncompressedSize);
        Assert.AreEqual("BYTE_ARRAY", lastColumn.Type);

        Assert.IsNotNull(lastColumn.Statistics);
        Assert.IsNull(lastColumn.Statistics.Min);
        Assert.IsNull(lastColumn.Statistics.Max);
        Assert.IsNull(lastColumn.Statistics.DistinctCount);
        Assert.AreEqual(0, lastColumn.Statistics.NullCount);
        Assert.AreEqual("argkp_007a45bc-7a3b-4030-8178-33d7c5fa5cb8", lastColumn.Statistics.MinValue);
        Assert.AreEqual("id", lastColumn.Statistics.MaxValue);
        Assert.IsNull(lastColumn.Statistics.IsMinValueExact);
        Assert.IsNull(lastColumn.Statistics.IsMinValueExact);
    }

    [SkippableTestMethod]
    public async Task DATETTIME_ONLY_TYPE_PYARROW_V22()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/TIME_ONLY_TYPE_PYARROW_V22.parquet");

        Assert.AreEqual(4626, parquetEngine.RecordCount);
        Assert.HasCount(2, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);
        Assert.AreEqual(new DateOnly(2024, 1, 1), dataTable.Rows[0][0]);
        Assert.AreEqual(new TimeOnly(215720000000), dataTable.Rows[0][1]);
    }

    //Note this test originally existed because we used a custom build of Parquet.Net. We're back on the
    //official releases now, lets keep it around for now as a regression test for Parquet.Net v6+ data page parsing.
    [SkippableTestMethod]
    public async Task NULL_DATA_PAGE_HEADER_PYARROW_V25()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/NULL_DATA_PAGE_HEADER_PYARROW_V25.parquet");

        Assert.AreEqual(46, parquetEngine.RecordCount);
        Assert.HasCount(2, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, 1, default))(false);
        Assert.AreEqual(DBNull.Value, dataTable.Rows[0][0]);
        Assert.AreEqual((double)0, dataTable.Rows[0][1]);
    }

    [SkippableTestMethod]
    [SkipWhen(typeof(DuckDBEngineTests), "DuckDB can't read TIME_NS types right now: https://github.com/Giorgi/DuckDB.NET/issues/383")]
    public async Task TIME_ONLY_MILLIS_MICROS_NANOS_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/TIME_ONLY_MILLIS_MICROS_NANOS_TEST.parquet");

        Assert.AreEqual(9, parquetEngine.RecordCount);
        Assert.HasCount(4, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
        Assert.AreEqual(new DateOnly(2026, 10, 24), dataTable.Rows[2][0]);
        Assert.AreEqual(new TimeOnly(), dataTable.Rows[2][1]);
        Assert.AreEqual(new TimeOnly(10), dataTable.Rows[2][2]);
        Assert.AreEqual(new TimeOnly(10), dataTable.Rows[2][3]);

        Assert.AreEqual(new TimeOnly(12340000), dataTable.Rows[4][1]);
        Assert.AreEqual(new TimeOnly(12345670), dataTable.Rows[4][2]);
        Assert.AreEqual(new TimeOnly(12345678), dataTable.Rows[4][3]);
    }

    [SkippableTestMethod]
    [SkipWhen(typeof(DuckDBEngineTests), "DuckDB.NET can't read null map values inside a list")]
    public async Task NESTED_COMBINATIONS_TEST()
    {
        await using var parquetEngine = await OpenFileOrFolderAsync("Data/NESTED_COMBINATIONS_TEST.parquet");
        Assert.AreEqual(5, parquetEngine.RecordCount);
        Assert.HasCount(5, parquetEngine.Fields);

        var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);

        //list_of_maps
        Assert.AreEqual(@"[[{""key"":""a"",""value"":1},{""key"":""b"",""value"":2}],[]]", dataTable.Rows[0][1].ToString());
        Assert.AreEqual(DBNull.Value, dataTable.Rows[1][1]);
        Assert.AreEqual(@"[null,[{""key"":""c"",""value"":3}]]", dataTable.Rows[2][1].ToString());
        Assert.AreEqual("[]", dataTable.Rows[3][1].ToString());
        Assert.AreEqual(@"[[{""key"":""d"",""value"":null}]]", dataTable.Rows[4][1].ToString());

        //map_of_structs
        Assert.AreEqual(@"[(k,{""x"":1,""y"":""one""})]", dataTable.Rows[0][2].ToString());
        Assert.AreEqual("[(k,)]", dataTable.Rows[1][2].ToString());
        Assert.AreEqual("[]", dataTable.Rows[2][2].ToString());
        Assert.AreEqual(DBNull.Value, dataTable.Rows[3][2]);
        Assert.AreEqual(@"[(k,{""x"":null,""y"":null})]", dataTable.Rows[4][2].ToString());

        //map_of_lists
        Assert.AreEqual("[(p,[1,2]),(q,[])]", dataTable.Rows[0][3].ToString());
        Assert.AreEqual("[(p,)]", dataTable.Rows[1][3].ToString());
        Assert.AreEqual(DBNull.Value, dataTable.Rows[2][3]);
        Assert.AreEqual("[]", dataTable.Rows[3][3].ToString());
        Assert.AreEqual("[(r,[null,3])]", dataTable.Rows[4][3].ToString());

        //struct_of_map
        Assert.AreEqual(@"{""m"":[{""key"":""z"",""value"":9}],""n"":1}", dataTable.Rows[0][4].ToString());
        Assert.AreEqual(DBNull.Value, dataTable.Rows[1][4]);
        Assert.AreEqual(@"{""m"":null,""n"":2}", dataTable.Rows[2][4].ToString());
        Assert.AreEqual(@"{""m"":[],""n"":null}", dataTable.Rows[3][4].ToString());
        Assert.AreEqual(@"{""m"":[{""key"":""w"",""value"":null}],""n"":4}", dataTable.Rows[4][4].ToString());
    }

    [SkippableTestMethod]
    public async Task NESTED_FIELDS_READ_CONSISTENTLY_WITH_AN_OFFSET()
    {
        string[] files =
        [
            "Data/STRUCT_TYPE_TEST.parquet",
            "Data/LIST_OF_STRUCT_OF_LIST_OF_STRUCT_NULL_ELEMENTS.parquet",
            "Data/LIST_OF_STRUCT_OF_LIST_OF_STRUCT_WITH_NULLS.parquet",
            "Data/NESTED_MAPS_TEST.parquet",
            .. _useDuckDBEngine ? Array.Empty<string>() : ["Data/NESTED_COMBINATIONS_TEST.parquet"]
        ];

        foreach (var file in files)
        {
            await using var parquetEngine = await OpenFileOrFolderAsync(file);
            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue))(false);

            //Reading each row on its own means skipping every row before it
            for (var offset = 0; offset < parquetEngine.RecordCount; offset++)
            {
                var slice = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, offset, 1))(false);
                Assert.HasCount(1, slice.Rows);
                for (var column = 0; column < dataTable.Columns.Count; column++)
                {
                    Assert.AreEqual(dataTable.Rows[offset][column].ToString(), slice.Rows[0][column].ToString(),
                        $"`{file}` row {offset} column {column} differs when read with an offset");
                }
            }
        }
    }
}