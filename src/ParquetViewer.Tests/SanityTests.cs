

namespace ParquetViewer.Tests
{
    public class SanityTests
    {
        [Fact]
        public async Task DECIMALS_AND_BOOLS_TEST()
        {
            var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/DECIMALS_AND_BOOLS_TEST.parquet", default);

            Assert.Equal(30, parquetEngine.RecordCount);
            Assert.Equal(337, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default);
            Assert.Equal((uint)156, dataTable.Rows[0][0]);
            Assert.Equal(60.7376101, dataTable.Rows[1][10]);
            Assert.False((bool)dataTable.Rows[19][332]);
            Assert.True((bool)dataTable.Rows[20][336]);
            Assert.Equal(DBNull.Value, dataTable.Rows[21][334]);
        }
    }
}