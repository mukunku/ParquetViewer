using ParquetViewer.Engine.Exceptions;
using ParquetViewer.Engine.Types;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
namespace ParquetViewer.Tests
{
    [TestClass]
    public class EngineTests
    {
        public EngineTests()
        {
            //Set a consistent date format for all tests
            ParquetEngineSettings.DateDisplayFormat = "yyyy-MM-dd HH:mm:ss";
        }

        [TestMethod]
        public async Task DECIMALS_AND_BOOLS_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/DECIMALS_AND_BOOLS_TEST.parquet", default);

            Assert.AreEqual(30, parquetEngine.RecordCount);
            Assert.HasCount(337, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.AreEqual((UInt16)156, dataTable.Rows[0][0]);
            Assert.AreEqual(60.7376101, dataTable.Rows[1][10]);
            Assert.IsFalse((bool)dataTable.Rows[19][332]);
            Assert.IsTrue((bool)dataTable.Rows[20][336]);
            Assert.AreEqual(DBNull.Value, dataTable.Rows[21][334]);
        }

        [TestMethod]
        public async Task DATETIME_TEST1()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/DATETIME_TEST1.parquet", default);

            Assert.AreEqual(10, parquetEngine.RecordCount);
            Assert.HasCount(3, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.AreEqual("36/2015-16", dataTable.Rows[0][0]);
            Assert.AreEqual(new DateTime(2015, 07, 14, 0, 0, 0), dataTable.Rows[1][2]);
            Assert.AreEqual(new DateTime(2015, 07, 19, 18, 30, 0), dataTable.Rows[9][1]);
        }

        [TestMethod]
        public async Task DATETIME_TEST2()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/DATETIME_TEST2.parquet", default);

            Assert.AreEqual(1, parquetEngine.RecordCount);
            Assert.HasCount(11, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.AreEqual((long)1, dataTable.Rows[0][0]);
            Assert.AreEqual(new DateTime(1985, 12, 31, 0, 0, 0), dataTable.Rows[0][1]);
            Assert.AreEqual(new DateTime(1, 1, 2, 0, 0, 0), dataTable.Rows[0][2]);
            Assert.AreEqual(new DateTime(9999, 12, 31, 0, 0, 0), dataTable.Rows[0][3]);
            Assert.AreEqual(new DateTime(9999, 12, 31, 0, 0, 0), dataTable.Rows[0][4]);
            Assert.AreEqual(new DateTime(1, 1, 1, 0, 0, 0), dataTable.Rows[0][5]);
            Assert.AreEqual(new DateTime(1985, 4, 13, 13, 5, 0), dataTable.Rows[0][6]);
            Assert.AreEqual(new DateTime(1, 1, 2, 0, 0, 0), dataTable.Rows[0][7]);
            Assert.AreEqual(new DateTime(9999, 12, 31, 23, 59, 59), dataTable.Rows[0][8]);
            Assert.AreEqual(new DateTime(3155378975999999990), dataTable.Rows[0][9]);
            Assert.AreEqual(new DateTime(1, 1, 1, 0, 0, 0), dataTable.Rows[0][10]);
        }

        [TestMethod]
        public async Task RANDOM_TEST_FILE_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/RANDOM_TEST_FILE.parquet", default);

            Assert.AreEqual(5, parquetEngine.RecordCount);
            Assert.HasCount(42, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.AreEqual((long)798921, dataTable.Rows[0][0]);
            Assert.AreEqual("BW15", dataTable.Rows[1][2]);
            Assert.AreEqual((long)7155937, dataTable.Rows[2][3]);
            Assert.AreEqual(DBNull.Value, dataTable.Rows[3][14]);
            Assert.AreEqual((double)22208120523, dataTable.Rows[4][25]);
            Assert.AreEqual("2022-08-10", dataTable.Rows[2][31]);
            Assert.AreEqual("DLIx12_SHIPCONF_BW15_20220812020138531.DWL", dataTable.Rows[1][41]);
        }

        [TestMethod]
        public async Task SAME_COLUMN_NAME_DIFFERENT_CASING_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/SAME_COLUMN_NAME_DIFFERENT_CASING.parquet", default);

            Assert.AreEqual(14610, parquetEngine.RecordCount);
            Assert.HasCount(12, parquetEngine.Fields);

            var ex = await Assert.ThrowsAsync<NotSupportedException>(async ()
                => (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false));
            Assert.AreEqual("Duplicate column 'schema/TransPlan_NORMAL_v2' detected. Column names are case insensitive and must be unique.", ex.Message);
        }

        [TestMethod]
        public async Task MULTIPLE_SCHEMAS_DETECTED_TEST()
        {
            var ex = await Assert.ThrowsAsync<MultipleSchemasFoundException>(() => ParquetEngine.OpenFileOrFolderAsync("Data", default));
            Assert.AreEqual("Multiple schemas found in directory.", ex.Message);
        }

        [TestMethod]
        public async Task PARTITIONED_PARQUET_FILE_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/PARTITIONED_PARQUET_FILE_TEST", default);

            Assert.AreEqual(2000, parquetEngine.RecordCount);
            Assert.HasCount(9, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
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

        [TestMethod]
        public async Task COLUMN_ENDING_IN_PERIOD_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/COLUMN_ENDING_IN_PERIOD_TEST.parquet", default);

            Assert.AreEqual(1, parquetEngine.RecordCount);
            Assert.HasCount(11, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.AreEqual(202252, dataTable.Rows[0][0]);
            Assert.IsFalse(dataTable.Rows[0]["Output as FP"] as bool?);
            Assert.AreEqual((byte)0, dataTable.Rows[0]["Preorder FP equi."]);
        }

        [TestMethod]
        public async Task LIST_TYPE_TEST1()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/LIST_TYPE_TEST1.parquet", default);

            Assert.AreEqual(3, parquetEngine.RecordCount);
            Assert.HasCount(2, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[0][0]);
            Assert.AreEqual("[1,2,3]", dataTable.Rows[0][0].ToString());
            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[0][1]);
            Assert.AreEqual("[\"abc\",\"efg\",\"hij\"]", dataTable.Rows[0][1].ToString());
            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[1][0]);
            Assert.AreEqual("[null,1]", dataTable.Rows[1][0].ToString());
            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[2][1]);
            Assert.AreEqual(4, ((ListValue)dataTable.Rows[2][1]).Length);
            Assert.AreEqual("efg", ((ListValue)dataTable.Rows[2][1]).Data![0]);
            Assert.AreEqual(DBNull.Value, ((ListValue)dataTable.Rows[2][1]).Data![1]);
            Assert.AreEqual("xyz", ((ListValue)dataTable.Rows[2][1]).Data![3]);

            //Also try reading with a record offset
            dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 1, 1, default))(false);
            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[0][0]);
            Assert.AreEqual("[null,1]", dataTable.Rows[0][0].ToString());
        }

        [TestMethod]
        public async Task LIST_TYPE_TEST2()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/LIST_TYPE_TEST2.parquet", default);

            Assert.AreEqual(8, parquetEngine.RecordCount);
            Assert.HasCount(2, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 2, 4, default))(false);
            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[0][1]);

            Assert.AreEqual("[1,2]", dataTable.Rows[0][1].ToString());
            Assert.AreEqual(1, ((ListValue)dataTable.Rows[0][1]).Data[0]);
            Assert.AreEqual(2, ((ListValue)dataTable.Rows[0][1]).Data[1]);

            Assert.AreEqual(string.Empty, dataTable.Rows[1][1].ToString());
            Assert.AreEqual(DBNull.Value, dataTable.Rows[1][1]);

            Assert.AreEqual("[]", dataTable.Rows[2][1].ToString());
            Assert.IsEmpty(((ListValue)dataTable.Rows[2][1]).Data.Cast<dynamic>());

            Assert.AreEqual("[3,4]", dataTable.Rows[3][1].ToString());
            Assert.AreEqual(3, ((ListValue)dataTable.Rows[3][1]).Data[0]);
            Assert.AreEqual(4, ((ListValue)dataTable.Rows[3][1]).Data[1]);
        }

        [TestMethod]
        public async Task MAP_TYPE_TEST1()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/MAP_TYPE_TEST1.parquet", default);

            Assert.AreEqual(2, parquetEngine.RecordCount);
            Assert.HasCount(2, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, 2, default))(false);

            Assert.IsInstanceOfType<MapValue>(dataTable.Rows[0][0]);
            var row = (MapValue)dataTable.Rows[0][0];
            Assert.AreEqual("id", row.FirstOrDefault().Key);
            Assert.AreEqual("something", row.FirstOrDefault().Value);
            Assert.AreEqual("value2", row.Skip(1).FirstOrDefault().Key);
            Assert.AreEqual("else", row.Skip(1).FirstOrDefault().Value);
            Assert.AreEqual("[(id,something),(value2,else)]", row.ToString());

            Assert.IsInstanceOfType<MapValue>(dataTable.Rows[1][0]);
            row = (MapValue)dataTable.Rows[1][0];
            Assert.AreEqual("id", row.FirstOrDefault().Key);
            Assert.AreEqual("something2", row.FirstOrDefault().Value);
            Assert.AreEqual("value", row.Skip(1).FirstOrDefault().Key);
            Assert.AreEqual("else2", row.Skip(1).FirstOrDefault().Value);
            Assert.AreEqual("[(id,something2),(value,else2)]", row.ToString());
        }

        [TestMethod]
        public async Task MAP_TYPE_TEST2()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/MAP_TYPE_TEST2.parquet", default);

            Assert.AreEqual(8, parquetEngine.RecordCount);
            Assert.HasCount(2, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 2, 4, default))(false);
            Assert.IsInstanceOfType<MapValue>(dataTable.Rows[0][1]);

            Assert.AreEqual("[(1,1),(2,2)]", dataTable.Rows[0][1].ToString());
            Assert.AreEqual(1, ((MapValue)dataTable.Rows[0][1]).Keys[0]);
            Assert.AreEqual(1, ((MapValue)dataTable.Rows[0][1]).Values[0]);
            Assert.AreEqual(2, ((MapValue)dataTable.Rows[0][1]).Keys[1]);
            Assert.AreEqual(2, ((MapValue)dataTable.Rows[0][1]).Values[1]);

            Assert.AreEqual(string.Empty, dataTable.Rows[1][1].ToString());
            Assert.AreEqual(DBNull.Value, dataTable.Rows[1][1]);

            Assert.AreEqual("[]", dataTable.Rows[2][1].ToString());
            Assert.IsEmpty(((MapValue)dataTable.Rows[2][1]).Keys.Cast<dynamic>());
            Assert.IsEmpty(((MapValue)dataTable.Rows[2][1]).Values.Cast<dynamic>());

            Assert.AreEqual("[(3,3),(4,4)]", dataTable.Rows[3][1].ToString());
            Assert.AreEqual(3, ((MapValue)dataTable.Rows[3][1]).Keys[0]);
            Assert.AreEqual(3, ((MapValue)dataTable.Rows[3][1]).Values[0]);
            Assert.AreEqual(4, ((MapValue)dataTable.Rows[3][1]).Keys[1]);
            Assert.AreEqual(4, ((MapValue)dataTable.Rows[3][1]).Values[1]);
        }

        [TestMethod]
        public async Task STRUCT_TYPE_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/STRUCT_TYPE_TEST.parquet", default);

            Assert.AreEqual(10, parquetEngine.RecordCount);
            Assert.HasCount(6, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.AreEqual(DBNull.Value, dataTable.Rows[0][0]);
            Assert.IsInstanceOfType<StructValue>(dataTable.Rows[2][0]);
            Assert.AreEqual("{\"appId\":\"e4a20b59-dd0e-4c50-b074-e8ae4786df30\",\"version\":0,\"lastUpdated\":1564524299648}", ((StructValue)dataTable.Rows[2][0]).ToString());
            Assert.AreEqual(DBNull.Value, dataTable.Rows[0][1]);
            Assert.IsInstanceOfType<StructValue>(dataTable.Rows[5][1]);
            Assert.AreEqual("{\"path\":\"part-00000-cb6b150b-30b8-4662-ad28-ff32ddab96d2-c000.snappy.parquet\",\"partitionValues\":[],\"size\":404,\"modificationTime\":1564524299000,\"dataChange\":false,\"stats\":null,\"tags\":null}", ((StructValue)dataTable.Rows[5][1]).ToString());
            Assert.IsInstanceOfType<StructValue>(dataTable.Rows[3][2]);
            Assert.AreEqual("{\"path\":\"part-00000-512e1537-8aaa-4193-b8b4-bef3de0de409-c000.snappy.parquet\",\"deletionTimestamp\":1564524298213,\"dataChange\":false}", ((StructValue)dataTable.Rows[3][2]).ToString());
            Assert.AreEqual(DBNull.Value, dataTable.Rows[0][3]);
            Assert.IsInstanceOfType<StructValue>(dataTable.Rows[1][3]);
            Assert.AreEqual("{\"id\":\"22ef18ba-191c-4c36-a606-3dad5cdf3830\",\"name\":null,\"description\":null,\"format\":{\"provider\":\"parquet\",\"options\":[]},\"schemaString\":\"{\\\"type\\\":\\\"struct\\\",\\\"fields\\\":[{\\\"name\\\":\\\"value\\\",\\\"type\\\":\\\"integer\\\",\\\"nullable\\\":true,\\\"metadata\\\":{}}]}\",\"partitionColumns\":null,\"configuration\":[],\"createdTime\":1564524294376}", ((StructValue)dataTable.Rows[1][3]).ToString());
            Assert.IsInstanceOfType<StructValue>(dataTable.Rows[0][4]);
            Assert.AreEqual("{\"minReaderVersion\":1,\"minWriterVersion\":2}", ((StructValue)dataTable.Rows[0][4]).ToString());
            Assert.AreEqual(DBNull.Value, dataTable.Rows[0][5]);
        }

        [TestMethod]
        public async Task NULLABLE_GUID_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/NULLABLE_GUID_TEST.parquet", default);

            Assert.AreEqual(1, parquetEngine.RecordCount);
            Assert.HasCount(33, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.IsFalse(dataTable.Rows[0][22] as bool?);
            Assert.AreEqual(new Guid("fdcbf90c-20d3-d745-b29f-9c2de1baa979"), dataTable.Rows[0][1]);
            Assert.AreEqual(new DateTime(2019, 1, 1), dataTable.Rows[0][4]);
        }

        [TestMethod]
        public async Task MALFORMED_DATETIME_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/MALFORMED_DATETIME_TEST.parquet", default);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.AreEqual(typeof(DateTime), dataTable.Rows[0]["ds"]?.GetType());
            Assert.AreEqual(new DateTime(2017, 1, 1), dataTable.Rows[0]["ds"]);
        }

        [TestMethod]
        public async Task COLUMN_NAME_WITH_FORWARD_SLASH_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/COLUMN_NAME_WITH_FORWARD_SLASH.parquet", default);

            Assert.AreEqual(1, parquetEngine.RecordCount);
            Assert.HasCount(320, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, 1, default))(false);
            Assert.AreEqual((byte)0, dataTable.Rows[0]["FLC K/L"]);
        }

        [TestMethod]
        public async Task ORACLE_MALFORMED_INT64_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/ORACLE_MALFORMED_INT64_TEST.parquet", default);

            Assert.AreEqual(126, parquetEngine.RecordCount);
            Assert.HasCount(2, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.AreEqual("DEPOSIT", dataTable.Rows[0][0]);
            Assert.AreEqual((long)1, dataTable.Rows[0][1]);
        }

        [TestMethod]
        public async Task LIST_OF_STRUCTS_TEST1()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/LIST_OF_STRUCTS1.parquet", default);
            Assert.AreEqual(2, parquetEngine.RecordCount);
            Assert.HasCount(2, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, 1, default))(false);

            Assert.AreEqual("Product1", dataTable.Rows[0][0]);
            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[0][1]);
            Assert.AreEqual("[{\"DateTime\":\"2024-04-15 22:00:00\",\"Quantity\":10},{\"DateTime\":\"2024-04-16 22:00:00\",\"Quantity\":20}]", dataTable.Rows[0][1].ToString());

            dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 1, 1, default))(false);

            Assert.AreEqual("Product2", dataTable.Rows[0][0]);
            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[0][1]);
            Assert.AreEqual("[{\"DateTime\":\"2024-04-15 22:00:00\",\"Quantity\":30},{\"DateTime\":\"2024-04-16 22:00:00\",\"Quantity\":40}]", dataTable.Rows[0][1].ToString());
        }

        [TestMethod]
        public async Task LIST_OF_STRUCTS_TEST2()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/LIST_OF_STRUCTS2.parquet", default);
            Assert.AreEqual(1, parquetEngine.RecordCount);
            Assert.HasCount(29, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);

            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[0][28]);
            Assert.AreEqual("[{\"purposeId\":\"HF85PyyGFprJXJvh5Pk9tg\",\"status\":\"Granted\",\"externalId\":\"General\",\"date\":\"2025-06-05 14:30:33\"}]", dataTable.Rows[0][28].ToString());
        }

        [TestMethod]
        public async Task EMPTY_LIST_OF_STRUCTS_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/EMPTY_LIST_OF_STRUCTS.parquet", default);
            Assert.AreEqual(2, parquetEngine.RecordCount);
            Assert.HasCount(2, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);

            Assert.AreEqual("Product1", dataTable.Rows[0][0]);
            Assert.AreEqual("Product2", dataTable.Rows[1][0]);

            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[0][1]);
            Assert.IsEmpty(((ListValue)dataTable.Rows[0][1]).Data);
            Assert.AreEqual("[]", dataTable.Rows[0][1].ToString());
            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[1][1]);
            Assert.IsEmpty(((ListValue)dataTable.Rows[1][1]).Data);
            Assert.AreEqual("[]", dataTable.Rows[1][1].ToString());
        }

        [TestMethod]
        public async Task PARQUET_MR_BREAKING_CHANGE_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/PARQUET-MR_1.15.0.parquet", default);
            Assert.AreEqual(5, parquetEngine.RecordCount);
            Assert.HasCount(7, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);

            Assert.AreEqual(1, dataTable.Rows[0][0]);
            Assert.AreEqual(5, dataTable.Rows[4][0]);

            Assert.AreEqual("John Doe", dataTable.Rows[0][1]);
            Assert.AreEqual("David Lee", dataTable.Rows[4][1]);

            Assert.IsTrue(dataTable.Rows[0][4] as bool?);
            Assert.IsTrue(dataTable.Rows[4][4] as bool?);
        }

        [TestMethod]
        public async Task DECIMALS_WITH_NO_SCALE_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/DECIMALS_WITH_NO_SCALE_TEST.parquet", default);
            Assert.AreEqual(10589, parquetEngine.RecordCount);
            Assert.HasCount(8, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);

            Assert.AreEqual(0.7072m, dataTable.Rows[0][5]);
            Assert.AreEqual(0m, dataTable.Rows[0][6]);
            Assert.AreEqual(0m, dataTable.Rows[0][7]);

            Assert.AreEqual(0.74527m, dataTable.Rows[100][5]);
            Assert.AreEqual(0m, dataTable.Rows[100][6]);
            Assert.AreEqual(0m, dataTable.Rows[100][7]);
        }

        [TestMethod]
        public async Task LIST_OF_LIST_OF_INT()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/LIST_OF_LIST_OF_INT.parquet", default);
            Assert.AreEqual(3, parquetEngine.RecordCount);
            Assert.HasCount(1, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.AreEqual("[[1],[1,2],[1,2,3],[1,2,3,4],[1,2,3,4,5]]", dataTable.Rows[0][0].ToString());
            Assert.AreEqual(DBNull.Value, dataTable.Rows[1][0]);
            Assert.AreEqual("[[1],[],[3],null,[5]]", dataTable.Rows[2][0].ToString());

            dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 1, 1, default))(false);
            Assert.AreEqual(DBNull.Value, dataTable.Rows[0][0]);

            dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 2, 1, default))(false);
            Assert.AreEqual("[[1],[],[3],null,[5]]", dataTable.Rows[0][0].ToString());
        }

        [TestMethod]
        public async Task LIST_OF_LIST_OF_LIST_OF_STRING()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/LIST_OF_LIST_OF_LIST_OF_STRING.parquet", default);
            Assert.AreEqual(3, parquetEngine.RecordCount);
            Assert.HasCount(2, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.AreEqual("[[[\"a\",\"b\"],[\"c\"]],[null,[\"d\"]]]", dataTable.Rows[0][0].ToString());
            Assert.AreEqual("[[[\"a\",\"b\"],[\"c\",\"d\"]],[null,[\"e\"]]]", dataTable.Rows[1][0].ToString());
            Assert.AreEqual("[[[\"a\",\"b\"],[\"c\",\"d\"],[\"e\"]],[null,[\"f\"]]]", dataTable.Rows[2][0].ToString());

            dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 1, 1, default))(false);
            Assert.AreEqual("[[[\"a\",\"b\"],[\"c\",\"d\"]],[null,[\"e\"]]]", dataTable.Rows[0][0].ToString());

            dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 2, 1, default))(false);
            Assert.AreEqual("[[[\"a\",\"b\"],[\"c\",\"d\"],[\"e\"]],[null,[\"f\"]]]", dataTable.Rows[0][0].ToString());
        }

        [TestMethod]
        public async Task LIST_OF_STRUCT_OF_LIST_OF_STRUCT()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/LIST_OF_STRUCT_OF_LIST_OF_STRUCT.parquet", default);
            Assert.AreEqual(1, parquetEngine.RecordCount);
            Assert.HasCount(1, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            const string expectedJson = @"[{""Labels"":[{""Name"":""PROFANITY"",""Score"":0.0467999987304211},{""Name"":""HATE_SPEECH"",""Score"":0.0710999965667725},{""Name"":""INSULT"",""Score"":0.113300003111362},{""Name"":""GRAPHIC"",""Score"":0.0186000000685453},{""Name"":""HARASSMENT_OR_ABUSE"",""Score"":0.060699999332428},{""Name"":""SEXUAL"",""Score"":0.0710999965667725},{""Name"":""VIOLENCE_OR_THREAT"",""Score"":0.0264999996870756}],""Toxicity"":0.0838999971747398},{""Labels"":[{""Name"":""PROFANITY"",""Score"":0.0702999979257584},{""Name"":""HATE_SPEECH"",""Score"":0.0883999988436699},{""Name"":""INSULT"",""Score"":0.132699996232986},{""Name"":""GRAPHIC"",""Score"":0.0186000000685453},{""Name"":""HARASSMENT_OR_ABUSE"",""Score"":0.0239000003784895},{""Name"":""SEXUAL"",""Score"":0.0710999965667725},{""Name"":""VIOLENCE_OR_THREAT"",""Score"":0.0264999996870756}],""Toxicity"":0.097900003194809},{""Labels"":[{""Name"":""PROFANITY"",""Score"":0.0467999987304211},{""Name"":""HATE_SPEECH"",""Score"":0.0797000005841255},{""Name"":""INSULT"",""Score"":0.117499999701977},{""Name"":""GRAPHIC"",""Score"":0.0195000004023314},{""Name"":""HARASSMENT_OR_ABUSE"",""Score"":0.060699999332428},{""Name"":""SEXUAL"",""Score"":0.143399998545647},{""Name"":""VIOLENCE_OR_THREAT"",""Score"":0.0276999995112419}],""Toxicity"":0.10249999910593},{""Labels"":[{""Name"":""PROFANITY"",""Score"":0.0604000017046928},{""Name"":""HATE_SPEECH"",""Score"":0.0874999985098839},{""Name"":""INSULT"",""Score"":0.127399995923042},{""Name"":""GRAPHIC"",""Score"":0.0195000004023314},{""Name"":""HARASSMENT_OR_ABUSE"",""Score"":0.060699999332428},{""Name"":""SEXUAL"",""Score"":0.142199993133545},{""Name"":""VIOLENCE_OR_THREAT"",""Score"":0.0264999996870756}],""Toxicity"":0.10809999704361},{""Labels"":[{""Name"":""PROFANITY"",""Score"":0.047800000756979},{""Name"":""HATE_SPEECH"",""Score"":0.0797000005841255},{""Name"":""INSULT"",""Score"":0.120399996638298},{""Name"":""GRAPHIC"",""Score"":0.0195000004023314},{""Name"":""HARASSMENT_OR_ABUSE"",""Score"":0.060699999332428},{""Name"":""SEXUAL"",""Score"":0.133100003004074},{""Name"":""VIOLENCE_OR_THREAT"",""Score"":0.0264999996870756}],""Toxicity"":0.0949999988079071},{""Labels"":[{""Name"":""PROFANITY"",""Score"":0.0467999987304211},{""Name"":""HATE_SPEECH"",""Score"":0.0710999965667725},{""Name"":""INSULT"",""Score"":0.119699999690056},{""Name"":""GRAPHIC"",""Score"":0.0195000004023314},{""Name"":""HARASSMENT_OR_ABUSE"",""Score"":0.060699999332428},{""Name"":""SEXUAL"",""Score"":0.1300999969244},{""Name"":""VIOLENCE_OR_THREAT"",""Score"":0.0264999996870756}],""Toxicity"":0.089699998497963},{""Labels"":[{""Name"":""PROFANITY"",""Score"":0.0693999975919724},{""Name"":""HATE_SPEECH"",""Score"":0.0785999968647957},{""Name"":""INSULT"",""Score"":0.12219999730587},{""Name"":""GRAPHIC"",""Score"":0.0195000004023314},{""Name"":""HARASSMENT_OR_ABUSE"",""Score"":0.060699999332428},{""Name"":""SEXUAL"",""Score"":0.146300002932549},{""Name"":""VIOLENCE_OR_THREAT"",""Score"":0.0276999995112419}],""Toxicity"":0.089699998497963},{""Labels"":[{""Name"":""PROFANITY"",""Score"":0.0604000017046928},{""Name"":""HATE_SPEECH"",""Score"":0.0702999979257584},{""Name"":""INSULT"",""Score"":0.10249999910593},{""Name"":""GRAPHIC"",""Score"":0.0195000004023314},{""Name"":""HARASSMENT_OR_ABUSE"",""Score"":0.060699999332428},{""Name"":""SEXUAL"",""Score"":0.184000000357628},{""Name"":""VIOLENCE_OR_THREAT"",""Score"":0.0264999996870756}],""Toxicity"":0.0741999968886376}]";

            Assert.AreEqual(expectedJson, dataTable.Rows[0][0].ToString());
        }

        [TestMethod]
        public async Task TWO_TIER_TEPEATED_LIST_FIELDS_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/TWO_TIER_TEPEATED_LIST_FIELDS_TEST.parquet", default);
            Assert.AreEqual(1, parquetEngine.RecordCount);
            Assert.HasCount(8, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);

            Assert.AreEqual(32, dataTable.Rows[0][0]);
            Assert.AreEqual((long)64, dataTable.Rows[0][1]);
            Assert.AreEqual(DBNull.Value, dataTable.Rows[0][2]);
            Assert.AreEqual("hello", dataTable.Rows[0][3]);
            Assert.AreEqual("[10,20]", dataTable.Rows[0][4].ToString());
            Assert.AreEqual("{\"nested\":\"nested!\"}", dataTable.Rows[0][5].ToString()); 
            Assert.AreEqual("096d06d7-e00b-4f70-ad5c-ca4da9a9630a", dataTable.Rows[0][6]);
            Assert.AreEqual("[\"element1\",\"element2\"]", dataTable.Rows[0][7].ToString());
        }
    }
}
