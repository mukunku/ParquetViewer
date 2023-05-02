using ParquetViewer.Engine.Exceptions;

namespace ParquetViewer.Tests
{
    public class SanityTests
    {
        [Fact]
        public async Task DECIMALS_AND_BOOLS_TEST()
        {
            var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/DECIMALS_AND_BOOLS_TEST1.parquet", default);

            Assert.Equal(30, parquetEngine.RecordCount);
            Assert.Equal(337, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default);
            Assert.Equal((UInt16)156, dataTable.Rows[0][0]);
            Assert.Equal(60.7376101, dataTable.Rows[1][10]);
            Assert.False((bool)dataTable.Rows[19][332]);
            Assert.True((bool)dataTable.Rows[20][336]);
            Assert.Equal(DBNull.Value, dataTable.Rows[21][334]);
        }

        [Fact]
        public async Task DATETIME_TEST1_TEST()
        {
            var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/DATETIME_TEST1.parquet", default);

            Assert.Equal(10, parquetEngine.RecordCount);
            Assert.Equal(3, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default);
            Assert.Equal("36/2015-16", dataTable.Rows[0][0]);
            Assert.Equal(new DateTime(2015, 07, 14, 0, 0, 0), dataTable.Rows[1][2]);
            Assert.Equal(new DateTime(2015, 07, 19, 18, 30, 0), dataTable.Rows[9][1]);
        }

        [Fact]
        public async Task DATETIME_TEST2_TEST()
        {
            var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/DATETIME_TEST2.parquet", default);

            Assert.Equal(1, parquetEngine.RecordCount);
            Assert.Equal(11, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default);
            Assert.Equal((long)1, dataTable.Rows[0][0]);
            Assert.Equal(new DateTime(1985, 12, 31, 0, 0, 0), dataTable.Rows[0][1]);
            Assert.Equal(new DateTime(1, 1, 2, 0, 0, 0), dataTable.Rows[0][2]);
            Assert.Equal(new DateTime(9999, 12, 31, 0, 0, 0), dataTable.Rows[0][3]);
            Assert.Equal(new DateTime(9999, 12, 31, 0, 0, 0), dataTable.Rows[0][4]);
            Assert.Equal(new DateTime(1, 1, 1, 0, 0, 0), dataTable.Rows[0][5]);
            Assert.Equal(new DateTime(1985, 4, 13, 13, 5, 0), dataTable.Rows[0][6]);
            Assert.Equal(new DateTime(1, 1, 2, 0, 0, 0), dataTable.Rows[0][7]);
            Assert.Equal(new DateTime(9999, 12, 31, 23, 59, 59), dataTable.Rows[0][8]);
            Assert.Equal(new DateTime(3155378975999999990), dataTable.Rows[0][9]);
            Assert.Equal(new DateTime(1, 1, 1, 0, 0, 0), dataTable.Rows[0][10]);
        }

        [Fact]
        public async Task RANDOM_TEST_FILE1_TEST()
        {
            var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/RANDOM_TEST_FILE1.parquet", default);

            Assert.Equal(5, parquetEngine.RecordCount);
            Assert.Equal(42, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default);
            Assert.Equal((long)798921, dataTable.Rows[0][0]);
            Assert.Equal("BW15", dataTable.Rows[1][2]);
            Assert.Equal((long)7155937, dataTable.Rows[2][3]);
            Assert.Equal(DBNull.Value, dataTable.Rows[3][14]);
            Assert.Equal((double)22208120523, dataTable.Rows[4][25]);
            Assert.Equal("2022-08-10", dataTable.Rows[2][31]);
            Assert.Equal("DLIx12_SHIPCONF_BW15_20220812020138531.DWL", dataTable.Rows[1][41]);
        }

        [Fact]
        public async Task SAME_COLUMN_NAME_DIFFERENT_CASING_TEST()
        {
            var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/SAME_COLUMN_NAME_DIFFERENT_CASING1.parquet", default);

            Assert.Equal(14610, parquetEngine.RecordCount);
            Assert.Equal(12, parquetEngine.Fields.Count);

            var ex = await Assert.ThrowsAsync<NotSupportedException>(() => parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default));
            Assert.Equal("Duplicate column 'TransPlan_NORMAL_v2' detected. Column names are case insensitive and must be unique.", ex.Message);
        }

        [Fact]
        public async Task MULTIPLE_SCHEMAS_DETECTED_TEST()
        {
            var ex = await Assert.ThrowsAsync<MultipleSchemasFoundException>(() => ParquetEngine.OpenFileOrFolderAsync("Data", default));
            Assert.Equal("Multiple schemas found in directory.", ex.Message);
        }

        [Fact]
        public async Task PARTITIONED_PARQUET_FILE_TEST()
        {
            var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/PARTITIONED_PARQUET_FILE_TEST1", default);

            Assert.Equal(2000, parquetEngine.RecordCount);
            Assert.Equal(9, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default);
            Assert.Equal("SHAPEFILE_JSON", dataTable.Rows[0][0]);
            Assert.Equal("5022121000", dataTable.Rows[200][2]);
            Assert.Equal((double)450, dataTable.Rows[500][3]);
            Assert.Equal("B000CTP5G2P2", dataTable.Rows[1999][8]);
            Assert.Equal("USA", dataTable.Rows[500][1]);

            dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 200, 1, default);
            Assert.Equal("5022121000", dataTable.Rows[0][2]);

            dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 500, 1, default);
            Assert.Equal((double)450, dataTable.Rows[0][3]);

            dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 1999, 1, default);
            Assert.Equal("B000CTP5G2P2", dataTable.Rows[0][8]);
        }

        [Fact]
        public async Task COLUMN_ENDING_IN_PERIOD_TEST1()
        {
            var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/COLUMN_ENDING_IN_PERIOD_TEST1.parquet", default);

            Assert.Equal(1, parquetEngine.RecordCount);
            Assert.Equal(11, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default);
            Assert.Equal(202252, dataTable.Rows[0][0]);
            Assert.Equal(false, dataTable.Rows[0]["Output as FP"]);
            Assert.Equal((byte)0, dataTable.Rows[0]["Preorder FP equi."]);
        }

        [Fact]
        public async Task LIST_TYPE_TEST()
        {
            var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/LIST_TYPE_TEST1.parquet", default);

            Assert.Equal(3, parquetEngine.RecordCount);
            Assert.Equal(2, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default);
            Assert.IsType<ListValue>(dataTable.Rows[0][0]);
            Assert.Equal("[1,2,3]", ((ListValue)dataTable.Rows[0][0]).ToString());
            Assert.IsType<ListValue>(dataTable.Rows[0][1]);
            Assert.Equal("[abc,efg,hij]", ((ListValue)dataTable.Rows[0][1]).ToString());
            Assert.IsType<ListValue>(dataTable.Rows[1][0]);
            Assert.Equal("[,1]", ((ListValue)dataTable.Rows[1][0]).ToString());
            Assert.IsType<ListValue>(dataTable.Rows[2][1]);
            Assert.Equal(4, ((ListValue)dataTable.Rows[2][1]).Data?.Count);
            Assert.Equal("efg", ((ListValue)dataTable.Rows[2][1]).Data[0]);
            Assert.Equal(DBNull.Value, ((ListValue)dataTable.Rows[2][1]).Data[1]);
            Assert.Equal("xyz", ((ListValue)dataTable.Rows[2][1]).Data[3]);
        }

        [Fact]
        public async Task MAP_TYPE_TEST1()
        {
            var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/MAP_TYPE_TEST1.parquet", default);

            Assert.Equal(2, parquetEngine.RecordCount);
            Assert.Equal(2, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, 2, default);
            Assert.IsType<MapValue>(dataTable.Rows[0][0]);
            Assert.Equal("(id,something)", ((MapValue)dataTable.Rows[0][0]).ToString());
            Assert.IsType<MapValue>(dataTable.Rows[0][0]);
            Assert.Equal("id", ((MapValue)dataTable.Rows[0][0]).Key);
            Assert.Equal("something", ((MapValue)dataTable.Rows[0][0]).Value);
            Assert.IsType<MapValue>(dataTable.Rows[1][0]);
            Assert.Equal("value2", ((MapValue)dataTable.Rows[1][0]).Key);
            Assert.Equal("else", ((MapValue)dataTable.Rows[1][0]).Value);
        }
    }
}
