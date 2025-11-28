using ParquetViewer.Engine.Exceptions;
using ParquetViewer.Engine.Types;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
namespace ParquetViewer.Tests
{
    [TestClass]
    public class ParquetNETEngineTests : EngineTests
    {
        public ParquetNETEngineTests() : base(useDuckDBEngine: false)
        {
            
        }
    }

    [TestClass]
    public class DuckDBEngineTests : EngineTests
    {
        public DuckDBEngineTests() : base(useDuckDBEngine: true)
        {

        }
    }

    public abstract class EngineTests
    {
        private bool _useDuckDBEngine;

        public EngineTests(bool useDuckDBEngine)
        {
            //Set a consistent date format for all tests
            ParquetEngineSettings.DateDisplayFormat = "yyyy-MM-dd HH:mm:ss";
            ParquetEngineSettings.DateOnlyDisplayFormat = "yyyy-MM-dd";

            this._useDuckDBEngine = useDuckDBEngine;
        }

        private async Task<IParquetEngine> OpenFileOrFolderAsync(string path, CancellationToken cancellationToken)
        {
            if (this._useDuckDBEngine)
            {
                return await Engine.DuckDB.ParquetEngine.OpenFileAsync(path, cancellationToken);
            }
            else
            {
                return await Engine.ParquetNET.ParquetEngine.OpenFileAsync(path, cancellationToken);

            }
        }

        [SkippableTestMethod]
        public async Task DECIMALS_AND_BOOLS_TEST()
        {
            using var parquetEngine = await OpenFileOrFolderAsync("Data/DECIMALS_AND_BOOLS_TEST.parquet", default);

            Assert.AreEqual(30, parquetEngine.RecordCount);
            Assert.HasCount(337, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.AreEqual((UInt16)156, dataTable.Rows[0][0]);
            Assert.AreEqual(60.7376101, dataTable.Rows[1][10]);
            Assert.IsFalse((bool)dataTable.Rows[19][332]);
            Assert.IsTrue((bool)dataTable.Rows[20][336]);
            Assert.AreEqual(DBNull.Value, dataTable.Rows[21][334]);
        }

        [SkippableTestMethod]
        public async Task DATETIME_TEST1()
        {
            using var parquetEngine = await OpenFileOrFolderAsync("Data/DATETIME_TEST1.parquet", default);

            Assert.AreEqual(10, parquetEngine.RecordCount);
            Assert.HasCount(3, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.AreEqual("36/2015-16", dataTable.Rows[0][0]);
            Assert.AreEqual(new DateOnly(2015, 07, 14), dataTable.Rows[1][2]);
            Assert.AreEqual(new DateTime(2015, 07, 19, 18, 30, 0), dataTable.Rows[9][1]);
        }

        [SkippableTestMethod]
        public async Task DATETIME_TEST2()
        {
            using var parquetEngine = await OpenFileOrFolderAsync("Data/DATETIME_TEST2.parquet", default);

            Assert.AreEqual(1, parquetEngine.RecordCount);
            Assert.HasCount(11, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
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
            using var parquetEngine = await OpenFileOrFolderAsync("Data/RANDOM_TEST_FILE.parquet", default);

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

        [SkippableTestMethod]
        [SkipWhen(typeof(DuckDBEngineTests), "DuckDB automatically appends _1 to the dupe column name")]
        public async Task SAME_COLUMN_NAME_DIFFERENT_CASING_TEST()
        {
            using var parquetEngine = await OpenFileOrFolderAsync("Data/SAME_COLUMN_NAME_DIFFERENT_CASING.parquet", default);

            Assert.AreEqual(14610, parquetEngine.RecordCount);
            Assert.HasCount(12, parquetEngine.Fields);

            var ex = await Assert.ThrowsAsync<NotSupportedException>(async ()
                => (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false));
            Assert.AreEqual("Duplicate column 'schema/TransPlan_NORMAL_v2' detected. Column names are case insensitive and must be unique.", ex.Message);
        }

        [SkippableTestMethod]
        public async Task MULTIPLE_SCHEMAS_DETECTED_TEST()
        {
            var ex = await Assert.ThrowsAsync<MultipleSchemasFoundException>(() => OpenFileOrFolderAsync("Data", default));
            Assert.AreEqual("Multiple schemas found in directory.", ex.Message);
        }

        [SkippableTestMethod]
        public async Task PARTITIONED_PARQUET_FILE_TEST()
        {
            using var parquetEngine = await OpenFileOrFolderAsync("Data/PARTITIONED_PARQUET_FILE_TEST", default);

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

        [SkippableTestMethod]
        public async Task COLUMN_ENDING_IN_PERIOD_TEST()
        {
            using var parquetEngine = await OpenFileOrFolderAsync("Data/COLUMN_ENDING_IN_PERIOD_TEST.parquet", default);

            Assert.AreEqual(1, parquetEngine.RecordCount);
            Assert.HasCount(11, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.AreEqual(202252, dataTable.Rows[0][0]);
            Assert.IsFalse((bool)dataTable.Rows[0]["Output as FP"]);
            Assert.AreEqual((byte)0, dataTable.Rows[0]["Preorder FP equi."]);
        }

        [SkippableTestMethod]
        public async Task LIST_TYPE_TEST1()
        {
            using var parquetEngine = await OpenFileOrFolderAsync("Data/LIST_TYPE_TEST1.parquet", default);

            Assert.AreEqual(3, parquetEngine.RecordCount);
            Assert.HasCount(2, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.IsInstanceOfType<IListValue>(dataTable.Rows[0][0]);
            Assert.AreEqual("[1,2,3]", dataTable.Rows[0][0].ToString());
            Assert.IsInstanceOfType<IListValue>(dataTable.Rows[0][1]);
            Assert.AreEqual("[abc,efg,hij]", dataTable.Rows[0][1].ToString());
            Assert.IsInstanceOfType<IListValue>(dataTable.Rows[1][0]);
            Assert.AreEqual("[,1]", dataTable.Rows[1][0].ToString());
            Assert.IsInstanceOfType<IListValue>(dataTable.Rows[2][1]);
            Assert.HasCount(4, (IListValue)dataTable.Rows[2][1]);
            Assert.AreEqual("efg", ((IListValue)dataTable.Rows[2][1]).Data![0]);
            Assert.AreEqual(DBNull.Value, ((IListValue)dataTable.Rows[2][1]).Data![1]);
            Assert.AreEqual("xyz", ((IListValue)dataTable.Rows[2][1]).Data![3]);

            //Also try reading with a record offset
            dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 1, 1, default))(false);
            Assert.IsInstanceOfType<IListValue>(dataTable.Rows[0][0]);
            Assert.AreEqual("[,1]", dataTable.Rows[0][0].ToString());
        }

        [SkippableTestMethod]
        public async Task LIST_TYPE_TEST2()
        {
            using var parquetEngine = await OpenFileOrFolderAsync("Data/LIST_TYPE_TEST2.parquet", default);

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
            using var parquetEngine = await OpenFileOrFolderAsync("Data/MAP_TYPE_TEST1.parquet", default);

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
            using var parquetEngine = await OpenFileOrFolderAsync("Data/MAP_TYPE_TEST2.parquet", default);

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
            using var parquetEngine = await OpenFileOrFolderAsync("Data/STRUCT_TYPE_TEST.parquet", default);

            Assert.AreEqual(10, parquetEngine.RecordCount);
            Assert.HasCount(6, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.IsInstanceOfType<IStructValue>(dataTable.Rows[0][0]);
            Assert.AreEqual("{\"appId\":null,\"version\":0,\"lastUpdated\":null}", ((IStructValue)dataTable.Rows[0][0]).ToString());
            Assert.IsInstanceOfType<IStructValue>(dataTable.Rows[0][1]);
            Assert.AreEqual("{\"path\":null,\"partitionValues\":null,\"size\":404,\"modificationTime\":1564524299000,\"dataChange\":false,\"stats\":null,\"tags\":null}", ((IStructValue)dataTable.Rows[0][1]).ToString());
            Assert.IsInstanceOfType<IStructValue>(dataTable.Rows[0][2]);
            Assert.AreEqual("{\"path\":null,\"deletionTimestamp\":null,\"dataChange\":false}", ((IStructValue)dataTable.Rows[0][2]).ToString());
            Assert.AreEqual(DBNull.Value, dataTable.Rows[0][3]);
            Assert.IsInstanceOfType<IStructValue>(dataTable.Rows[0][4]);
            Assert.AreEqual("{\"minReaderVersion\":1,\"minWriterVersion\":2}", ((IStructValue)dataTable.Rows[0][4]).ToString());
            Assert.AreEqual(DBNull.Value, dataTable.Rows[0][5]);
            Assert.IsInstanceOfType<DBNull>(dataTable.Rows[9][4]);
            Assert.AreEqual(DBNull.Value, dataTable.Rows[9][4]);
            Assert.AreEqual("{\"appId\":\"e4a20b59-dd0e-4c50-b074-e8ae4786df30\",\"version\":null,\"lastUpdated\":1564524299648}", ((IStructValue)dataTable.Rows[2][0]).ToString());
        }

        [SkippableTestMethod]
        public async Task NULLABLE_GUID_TEST()
        {
            using var parquetEngine = await OpenFileOrFolderAsync("Data/NULLABLE_GUID_TEST.parquet", default);

            Assert.AreEqual(1, parquetEngine.RecordCount);
            Assert.HasCount(33, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.IsFalse((bool)dataTable.Rows[0][22]);
            Assert.AreEqual(new Guid("fdcbf90c-20d3-d745-b29f-9c2de1baa979"), dataTable.Rows[0][1]);
            Assert.AreEqual(new DateTime(2019, 1, 1), dataTable.Rows[0][4]);
        }

        [SkippableTestMethod]
        public async Task MALFORMED_DATETIME_TEST()
        {
            using var parquetEngine = await OpenFileOrFolderAsync("Data/MALFORMED_DATETIME_TEST.parquet", default);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.IsInstanceOfType<DateTime>(dataTable.Rows[0]["ds"]);
            Assert.AreEqual(new DateTime(2017, 1, 1), dataTable.Rows[0]["ds"]);
        }

        [SkippableTestMethod]
        public async Task COLUMN_NAME_WITH_FORWARD_SLASH_TEST()
        {
            using var parquetEngine = await OpenFileOrFolderAsync("Data/COLUMN_NAME_WITH_FORWARD_SLASH.parquet", default);

            Assert.AreEqual(1, parquetEngine.RecordCount);
            Assert.HasCount(320, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, 1, default))(false);
            Assert.AreEqual((byte)0, dataTable.Rows[0]["FLC K/L"]);
        }

        [SkippableTestMethod]
        public async Task ORACLE_MALFORMED_INT64_TEST()
        {
            using var parquetEngine = await OpenFileOrFolderAsync("Data/ORACLE_MALFORMED_INT64_TEST.parquet", default);

            Assert.AreEqual(126, parquetEngine.RecordCount);
            Assert.HasCount(2, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.AreEqual("DEPOSIT", dataTable.Rows[0][0]);
            Assert.AreEqual((long)1, dataTable.Rows[0][1]);
        }

        [SkippableTestMethod]
        public async Task LIST_OF_STRUCTS_TEST1()
        {
            using var parquetEngine = await OpenFileOrFolderAsync("Data/LIST_OF_STRUCTS1.parquet", default);
            Assert.AreEqual(2, parquetEngine.RecordCount);
            Assert.HasCount(2, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);

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
            using var parquetEngine = await OpenFileOrFolderAsync("Data/LIST_OF_STRUCTS2.parquet", default);
            Assert.AreEqual(1, parquetEngine.RecordCount);
            Assert.HasCount(29, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);

            Assert.IsInstanceOfType<IListValue>(dataTable.Rows[0][28]);
            Assert.AreEqual("[{\"purposeId\":\"HF85PyyGFprJXJvh5Pk9tg\",\"status\":\"Granted\",\"externalId\":\"General\",\"date\":\"2025-06-05 14:30:33\"}]", dataTable.Rows[0][28].ToString());
        }

        [SkippableTestMethod]
        public async Task EMPTY_LIST_OF_STRUCTS_TEST()
        {
            using var parquetEngine = await OpenFileOrFolderAsync("Data/EMPTY_LIST_OF_STRUCTS.parquet", default);
            Assert.AreEqual(2, parquetEngine.RecordCount);
            Assert.HasCount(2, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);

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
            using var parquetEngine = await OpenFileOrFolderAsync("Data/PARQUET-MR_1.15.0.parquet", default);
            Assert.AreEqual(5, parquetEngine.RecordCount);
            Assert.HasCount(7, parquetEngine.Fields);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);

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
            using var parquetEngine = await OpenFileOrFolderAsync("Data/DECIMALS_WITH_NO_SCALE_TEST.parquet", default);
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
    }
}
