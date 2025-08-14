using ParquetViewer.Analytics;
using ParquetViewer.Engine.Exceptions;
using ParquetViewer.Engine.Types;
using RichardSzalay.MockHttp;
using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ParquetViewer.Tests
{
    public class SanityTests
    {
        public SanityTests()
        {
            //Set a consistent date format for all tests
            ParquetEngineSettings.DateDisplayFormat = "yyyy-MM-dd HH:mm:ss";
        }

        [Fact]
        public async Task DECIMALS_AND_BOOLS_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/DECIMALS_AND_BOOLS_TEST.parquet", default);

            Assert.Equal(30, parquetEngine.RecordCount);
            Assert.Equal(337, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.Equal((UInt16)156, dataTable.Rows[0][0]);
            Assert.Equal(60.7376101, dataTable.Rows[1][10]);
            Assert.False((bool)dataTable.Rows[19][332]);
            Assert.True((bool)dataTable.Rows[20][336]);
            Assert.Equal(DBNull.Value, dataTable.Rows[21][334]);
        }

        [Fact]
        public async Task DATETIME_TEST1()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/DATETIME_TEST1.parquet", default);

            Assert.Equal(10, parquetEngine.RecordCount);
            Assert.Equal(3, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.Equal("36/2015-16", dataTable.Rows[0][0]);
            Assert.Equal(new DateTime(2015, 07, 14, 0, 0, 0), dataTable.Rows[1][2]);
            Assert.Equal(new DateTime(2015, 07, 19, 18, 30, 0), dataTable.Rows[9][1]);
        }

        [Fact]
        public async Task DATETIME_TEST2()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/DATETIME_TEST2.parquet", default);

            Assert.Equal(1, parquetEngine.RecordCount);
            Assert.Equal(11, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
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
        public async Task RANDOM_TEST_FILE_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/RANDOM_TEST_FILE.parquet", default);

            Assert.Equal(5, parquetEngine.RecordCount);
            Assert.Equal(42, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
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
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/SAME_COLUMN_NAME_DIFFERENT_CASING.parquet", default);

            Assert.Equal(14610, parquetEngine.RecordCount);
            Assert.Equal(12, parquetEngine.Fields.Count);

            var ex = await Assert.ThrowsAsync<NotSupportedException>(async ()
                => (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false));
            Assert.Equal("Duplicate column 'schema/TransPlan_NORMAL_v2' detected. Column names are case insensitive and must be unique.", ex.Message);
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
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/PARTITIONED_PARQUET_FILE_TEST", default);

            Assert.Equal(2000, parquetEngine.RecordCount);
            Assert.Equal(9, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.Equal("SHAPEFILE_JSON", dataTable.Rows[0][0]);
            Assert.Equal("5022121000", dataTable.Rows[200][2]);
            Assert.Equal((double)450, dataTable.Rows[500][3]);
            Assert.Equal("B000CTP5G2P2", dataTable.Rows[1999][8]);
            Assert.Equal("USA", dataTable.Rows[500][1]);

            dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 200, 1, default))(false);
            Assert.Equal("5022121000", dataTable.Rows[0][2]);

            dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 500, 1, default))(false);
            Assert.Equal((double)450, dataTable.Rows[0][3]);

            dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 1999, 1, default))(false);
            Assert.Equal("B000CTP5G2P2", dataTable.Rows[0][8]);
        }

        [Fact]
        public async Task COLUMN_ENDING_IN_PERIOD_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/COLUMN_ENDING_IN_PERIOD_TEST.parquet", default);

            Assert.Equal(1, parquetEngine.RecordCount);
            Assert.Equal(11, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.Equal(202252, dataTable.Rows[0][0]);
            Assert.Equal(false, dataTable.Rows[0]["Output as FP"]);
            Assert.Equal((byte)0, dataTable.Rows[0]["Preorder FP equi."]);
        }

        [Fact]
        public async Task LIST_TYPE_TEST1()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/LIST_TYPE_TEST1.parquet", default);

            Assert.Equal(3, parquetEngine.RecordCount);
            Assert.Equal(2, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.IsType<ListValue>(dataTable.Rows[0][0]);
            Assert.Equal("[1,2,3]", dataTable.Rows[0][0].ToString());
            Assert.IsType<ListValue>(dataTable.Rows[0][1]);
            Assert.Equal("[abc,efg,hij]", dataTable.Rows[0][1].ToString());
            Assert.IsType<ListValue>(dataTable.Rows[1][0]);
            Assert.Equal("[,1]", dataTable.Rows[1][0].ToString());
            Assert.IsType<ListValue>(dataTable.Rows[2][1]);
            Assert.Equal(4, ((ListValue)dataTable.Rows[2][1]).Length);
            Assert.Equal("efg", ((ListValue)dataTable.Rows[2][1]).Data![0]);
            Assert.Equal(DBNull.Value, ((ListValue)dataTable.Rows[2][1]).Data![1]);
            Assert.Equal("xyz", ((ListValue)dataTable.Rows[2][1]).Data![3]);

            //Also try reading with a record offset
            dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 1, 1, default))(false);
            Assert.IsType<ListValue>(dataTable.Rows[0][0]);
            Assert.Equal("[,1]", dataTable.Rows[0][0].ToString());
        }

        [Fact]
        public async Task LIST_TYPE_TEST2()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/LIST_TYPE_TEST2.parquet", default);

            Assert.Equal(8, parquetEngine.RecordCount);
            Assert.Equal(2, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 2, 4, default))(false);
            Assert.IsType<ListValue>(dataTable.Rows[0][1]);

            Assert.Equal("[1,2]", dataTable.Rows[0][1].ToString());
            Assert.Equal(1, ((ListValue)dataTable.Rows[0][1]).Data[0]);
            Assert.Equal(2, ((ListValue)dataTable.Rows[0][1]).Data[1]);

            Assert.Equal(string.Empty, dataTable.Rows[1][1].ToString());
            Assert.Equal(DBNull.Value, dataTable.Rows[1][1]);

            Assert.Equal("[]", dataTable.Rows[2][1].ToString());
            Assert.Empty(((ListValue)dataTable.Rows[2][1]).Data);

            Assert.Equal("[3,4]", dataTable.Rows[3][1].ToString());
            Assert.Equal(3, ((ListValue)dataTable.Rows[3][1]).Data[0]);
            Assert.Equal(4, ((ListValue)dataTable.Rows[3][1]).Data[1]);
        }

        [Fact]
        public async Task MAP_TYPE_TEST1()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/MAP_TYPE_TEST1.parquet", default);

            Assert.Equal(2, parquetEngine.RecordCount);
            Assert.Equal(2, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, 2, default))(false);

            Assert.IsType<MapValue>(dataTable.Rows[0][0]);
            var row = (MapValue)dataTable.Rows[0][0];
            Assert.Equal("id", row.FirstOrDefault().Key);
            Assert.Equal("something", row.FirstOrDefault().Value);
            Assert.Equal("value2", row.Skip(1).FirstOrDefault().Key);
            Assert.Equal("else", row.Skip(1).FirstOrDefault().Value);
            Assert.Equal("[(id,something),(value2,else)]", row.ToString());

            Assert.IsType<MapValue>(dataTable.Rows[1][0]);
            row = (MapValue)dataTable.Rows[1][0];
            Assert.Equal("id", row.FirstOrDefault().Key);
            Assert.Equal("something2", row.FirstOrDefault().Value);
            Assert.Equal("value", row.Skip(1).FirstOrDefault().Key);
            Assert.Equal("else2", row.Skip(1).FirstOrDefault().Value);
            Assert.Equal("[(id,something2),(value,else2)]", row.ToString());
        }

        [Fact]
        public async Task MAP_TYPE_TEST2()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/MAP_TYPE_TEST2.parquet", default);

            Assert.Equal(8, parquetEngine.RecordCount);
            Assert.Equal(2, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 2, 4, default))(false);
            Assert.IsType<MapValue>(dataTable.Rows[0][1]);

            Assert.Equal("[(1,1),(2,2)]", dataTable.Rows[0][1].ToString());
            Assert.Equal(1, ((MapValue)dataTable.Rows[0][1]).Keys[0]);
            Assert.Equal(1, ((MapValue)dataTable.Rows[0][1]).Values[0]);
            Assert.Equal(2, ((MapValue)dataTable.Rows[0][1]).Keys[1]);
            Assert.Equal(2, ((MapValue)dataTable.Rows[0][1]).Values[1]);

            Assert.Equal(string.Empty, dataTable.Rows[1][1].ToString());
            Assert.Equal(DBNull.Value, dataTable.Rows[1][1]);

            Assert.Equal("[]", dataTable.Rows[2][1].ToString());
            Assert.Empty(((MapValue)dataTable.Rows[2][1]).Keys);
            Assert.Empty(((MapValue)dataTable.Rows[2][1]).Values);

            Assert.Equal("[(3,3),(4,4)]", dataTable.Rows[3][1].ToString());
            Assert.Equal(3, ((MapValue)dataTable.Rows[3][1]).Keys[0]);
            Assert.Equal(3, ((MapValue)dataTable.Rows[3][1]).Values[0]);
            Assert.Equal(4, ((MapValue)dataTable.Rows[3][1]).Keys[1]);
            Assert.Equal(4, ((MapValue)dataTable.Rows[3][1]).Values[1]);
        }

        [Fact]
        public async Task STRUCT_TYPE_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/STRUCT_TYPE_TEST.parquet", default);

            Assert.Equal(10, parquetEngine.RecordCount);
            Assert.Equal(6, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.IsType<StructValue>(dataTable.Rows[0][0]);
            Assert.Equal("{\"appId\":null,\"version\":0,\"lastUpdated\":null}", ((StructValue)dataTable.Rows[0][0]).ToString());
            Assert.IsType<StructValue>(dataTable.Rows[0][1]);
            Assert.Equal("{\"path\":null,\"partitionValues\":null,\"size\":404,\"modificationTime\":1564524299000,\"dataChange\":false,\"stats\":null,\"tags\":null}", ((StructValue)dataTable.Rows[0][1]).ToString());
            Assert.IsType<StructValue>(dataTable.Rows[0][2]);
            Assert.Equal("{\"path\":null,\"deletionTimestamp\":null,\"dataChange\":false}", ((StructValue)dataTable.Rows[0][2]).ToString());
            Assert.Equal(DBNull.Value, dataTable.Rows[0][3]);
            Assert.IsType<StructValue>(dataTable.Rows[0][4]);
            Assert.Equal("{\"minReaderVersion\":1,\"minWriterVersion\":2}", ((StructValue)dataTable.Rows[0][4]).ToString());
            Assert.Equal(DBNull.Value, dataTable.Rows[0][5]);
            Assert.IsType<DBNull>(dataTable.Rows[9][4]);
            Assert.Equal(DBNull.Value, dataTable.Rows[9][4]);
            Assert.Equal("{\"appId\":\"e4a20b59-dd0e-4c50-b074-e8ae4786df30\",\"version\":null,\"lastUpdated\":1564524299648}", ((StructValue)dataTable.Rows[2][0]).ToString());
        }

        [Fact]
        public async Task AMPLITUDE_EVENT_TEST()
        {
            var testEvent = TestAmplitudeEvent.MockRequest(out var mockHttpHandler);
            testEvent.IgnoredProperty = "xxx";
            testEvent.RegularProperty = "yyy";

            var isSelfContainedExecutable = false;
#if RELEASE_SELFCONTAINED
            isSelfContainedExecutable = true;
#endif

            string expectedRequestJson = @$"
{{
    ""api_key"": ""dummy"",
    ""events"": [{{
        ""device_id"": ""{AppSettings.AnalyticsDeviceId}"",
        ""event_type"": ""{TestAmplitudeEvent.EVENT_TYPE}"",
        ""user_properties"": {{
            ""alwaysLoadAllRecords"": {AppSettings.AlwaysLoadAllRecords.ToString().ToLower()},
            ""alwaysSelectAllFields"": {AppSettings.AlwaysSelectAllFields.ToString().ToLower()},
            ""autoSizeColumnsMode"": ""{AppSettings.AutoSizeColumnsMode}"",
            ""dateTimeDisplayFormat"": ""{AppSettings.DateTimeDisplayFormat}"",
            ""systemMemory"": {(int)(GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1048576.0 /*magic number*/)},
            ""processorCount"": {Environment.ProcessorCount},
            ""isDefaultParquetViewer"": {AboutBox.IsDefaultViewerForParquetFiles.ToString().ToLower()},
            ""darkMode"": {AppSettings.DarkMode.ToString().ToLower()},
            ""selfContainedExecutable"": {(isSelfContainedExecutable ? "true" : "false")}
        }},
        ""event_properties"": {{
            ""regularProperty"": ""yyy""
        }},
        ""session_id"": {testEvent.SessionId},
        ""language"": ""{CultureInfo.CurrentUICulture.Name}"",
        ""os_name"": ""{Environment.OSVersion.Platform}"",
        ""os_version"": ""{Environment.OSVersion.VersionString}"",
        ""app_version"": ""{AboutBox.AssemblyVersion}""
    }}]
}}";

            //mock the http response
            _ = mockHttpHandler.Expect(HttpMethod.Post, "*").Respond(async (request) =>
            {
                string requestJsonBody = await (request.Content?.ReadAsStringAsync() ?? Task.FromResult(string.Empty));

                if (Regex.Replace(requestJsonBody, "\\s", string.Empty)
                    .Equals(Regex.Replace(expectedRequestJson, "\\s", string.Empty)))
                    return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                else
                    return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            });

            bool wasSuccess = await testEvent.Record();
            Assert.True(wasSuccess, "The event json we would have sent to Amplitude didn't match the expected value");
        }

        [Fact]
        public async Task AMPLITUDE_EXCEPTION_SENSITIVE_TEXT_MASKING_TEST()
        {
            var testAmplitudeEvent = TestAmplitudeEvent.MockRequest(out var mockHttpHandler);
            var testEvent = new ExceptionEvent(new Exception("Exception with `sensitive` data"), testAmplitudeEvent.CloneAmplitudeConfiguration());

            //mock the http response
            _ = mockHttpHandler.Expect(HttpMethod.Post, "*").Respond(async (request) =>
            {
                string requestJsonBody = await (request.Content?.ReadAsStringAsync() ?? Task.FromResult(string.Empty));

                if (requestJsonBody.Contains($"Exception with {ExceptionEvent.MASK_SENTINEL} data"))
                    return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                else
                    return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            });

            bool wasSuccess = await testEvent.Record();
            Assert.True(wasSuccess, "Sensitive data wasn't stripped out correctly");
        }

        [Fact]
        public async Task AMPLITUDE_EXCEPTION_ADDITIONAL_DATA_IS_SERIALIZED_TEST()
        {
            var testAmplitudeEvent = TestAmplitudeEvent.MockRequest(out var mockHttpHandler);

            var testException = new Exception("Exception with additional data");
            testException.Data["key1"] = "value1";
            testException.Data["key2"] = "value2";
            var testEvent = new ExceptionEvent(testException, testAmplitudeEvent.CloneAmplitudeConfiguration());

            //mock the http response
            _ = mockHttpHandler.Expect(HttpMethod.Post, "*").Respond(async (request) =>
            {
                string requestJsonBody = await (request.Content?.ReadAsStringAsync() ?? Task.FromResult(string.Empty));

                var requestJson = JsonNode.Parse(requestJsonBody);
                if (requestJson?["events"]?[0]?["event_properties"]?["key1"]?.GetValue<string>() == "value1"
                    && requestJson?["events"]?[0]?["event_properties"]?["key2"]?.GetValue<string>() == "value2"
                    && requestJson?["events"]?[0]?["event_properties"]?["message"]?.GetValue<string>() == "Exception with additional data")
                    return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                else
                    return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            });

            bool wasSuccess = await testEvent.Record();
            Assert.True(wasSuccess, "Additional exception data wasn't added to the amplitude event as expected");
        }

        [Fact]
        public async Task NULLABLE_GUID_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/NULLABLE_GUID_TEST.parquet", default);

            Assert.Equal(1, parquetEngine.RecordCount);
            Assert.Equal(33, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.Equal(false, dataTable.Rows[0][22]);
            Assert.Equal(new Guid("fdcbf90c-20d3-d745-b29f-9c2de1baa979"), dataTable.Rows[0][1]);
            Assert.Equal(new DateTime(2019, 1, 1), dataTable.Rows[0][4]);
        }

        [Fact]
        public async Task MALFORMED_DATETIME_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/MALFORMED_DATETIME_TEST.parquet", default);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.Equal(typeof(DateTime), dataTable.Rows[0]["ds"]?.GetType());
            Assert.Equal(new DateTime(2017, 1, 1), dataTable.Rows[0]["ds"]);
        }

        [Fact]
        public async Task COLUMN_NAME_WITH_FORWARD_SLASH_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/COLUMN_NAME_WITH_FORWARD_SLASH.parquet", default);

            Assert.Equal(1, parquetEngine.RecordCount);
            Assert.Equal(320, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, 1, default))(false);
            Assert.Equal((byte)0, dataTable.Rows[0]["FLC K/L"]);
        }

        [Fact]
        public async Task ORACLE_MALFORMED_INT64_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/ORACLE_MALFORMED_INT64_TEST.parquet", default);

            Assert.Equal(126, parquetEngine.RecordCount);
            Assert.Equal(2, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.Equal("DEPOSIT", dataTable.Rows[0][0]);
            Assert.Equal((long)1, dataTable.Rows[0][1]);
        }

        [Fact]
        public async Task LIST_OF_STRUCTS_TEST1()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/LIST_OF_STRUCTS1.parquet", default);
            Assert.Equal(2, parquetEngine.RecordCount);
            Assert.Equal(2, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);

            Assert.Equal("Product1", dataTable.Rows[0][0]);
            Assert.Equal("Product2", dataTable.Rows[1][0]);

            Assert.IsType<ListValue>(dataTable.Rows[0][1]);
            Assert.Equal("[{\"DateTime\":\"2024-04-15 22:00:00\",\"Quantity\":10},{\"DateTime\":\"2024-04-16 22:00:00\",\"Quantity\":20}]", dataTable.Rows[0][1].ToString());
            Assert.IsType<ListValue>(dataTable.Rows[1][1]);
            Assert.Equal("[{\"DateTime\":\"2024-04-15 22:00:00\",\"Quantity\":30},{\"DateTime\":\"2024-04-16 22:00:00\",\"Quantity\":40}]", dataTable.Rows[1][1].ToString());
        }

        [Fact]
        public async Task LIST_OF_STRUCTS_TEST2()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/LIST_OF_STRUCTS2.parquet", default);
            Assert.Equal(1, parquetEngine.RecordCount);
            Assert.Equal(29, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);

            Assert.IsType<ListValue>(dataTable.Rows[0][28]);
            Assert.Equal("[{\"purposeId\":\"HF85PyyGFprJXJvh5Pk9tg\",\"status\":\"Granted\",\"externalId\":\"General\",\"date\":\"2025-06-05 14:30:33\"}]", dataTable.Rows[0][28].ToString());
        }

        [Fact]
        public async Task EMPTY_LIST_OF_STRUCTS_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/EMPTY_LIST_OF_STRUCTS.parquet", default);
            Assert.Equal(2, parquetEngine.RecordCount);
            Assert.Equal(2, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);

            Assert.Equal("Product1", dataTable.Rows[0][0]);
            Assert.Equal("Product2", dataTable.Rows[1][0]);

            Assert.IsType<ListValue>(dataTable.Rows[0][1]);
            Assert.Equal("[]", dataTable.Rows[0][1].ToString());
            Assert.IsType<ListValue>(dataTable.Rows[1][1]);
            Assert.Equal("[]", dataTable.Rows[1][1].ToString());
        }

        [Fact]
        public async Task PARQUET_MR_BREAKING_CHANGE_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/PARQUET-MR_1.15.0.parquet", default);
            Assert.Equal(5, parquetEngine.RecordCount);
            Assert.Equal(7, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);

            Assert.Equal(1, dataTable.Rows[0][0]);
            Assert.Equal(5, dataTable.Rows[4][0]);

            Assert.Equal("John Doe", dataTable.Rows[0][1]);
            Assert.Equal("David Lee", dataTable.Rows[4][1]);

            Assert.Equal(true, dataTable.Rows[0][4]);
            Assert.Equal(true, dataTable.Rows[4][4]);
        }

        [Fact]
        public async Task DECIMALS_WITH_NO_SCALE_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/DECIMALS_WITH_NO_SCALE_TEST.parquet", default);
            Assert.Equal(10589, parquetEngine.RecordCount);
            Assert.Equal(8, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);

            Assert.Equal(0.7072m, dataTable.Rows[0][5]);
            Assert.Equal(0m, dataTable.Rows[0][6]);
            Assert.Equal(0m, dataTable.Rows[0][7]);

            Assert.Equal(0.74527m, dataTable.Rows[100][5]);
            Assert.Equal(0m, dataTable.Rows[100][6]);
            Assert.Equal(0m, dataTable.Rows[100][7]);
        }
    }
}
