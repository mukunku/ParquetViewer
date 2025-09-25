using ParquetViewer.Analytics;
using ParquetViewer.Engine.Exceptions;
using ParquetViewer.Engine.Types;
using ParquetViewer.Helpers;
using RichardSzalay.MockHttp;
using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
namespace ParquetViewer.Tests
{
    [TestClass]
    public class SanityTests
    {
        public SanityTests()
        {
            //Set a consistent date format for all tests
            ParquetEngineSettings.DateDisplayFormat = "yyyy-MM-dd HH:mm:ss";
        }

        [TestMethod]
        public async Task DECIMALS_AND_BOOLS_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/DECIMALS_AND_BOOLS_TEST.parquet", default);

            Assert.AreEqual(30, parquetEngine.RecordCount);
            Assert.AreEqual(337, parquetEngine.Fields.Count);

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
            Assert.AreEqual(3, parquetEngine.Fields.Count);

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
            Assert.AreEqual(11, parquetEngine.Fields.Count);

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
            Assert.AreEqual(42, parquetEngine.Fields.Count);

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
            Assert.AreEqual(12, parquetEngine.Fields.Count);

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
            Assert.AreEqual(9, parquetEngine.Fields.Count);

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
            Assert.AreEqual(11, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.AreEqual(202252, dataTable.Rows[0][0]);
            Assert.AreEqual(false, dataTable.Rows[0]["Output as FP"]);
            Assert.AreEqual((byte)0, dataTable.Rows[0]["Preorder FP equi."]);
        }

        [TestMethod]
        public async Task LIST_TYPE_TEST1()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/LIST_TYPE_TEST1.parquet", default);

            Assert.AreEqual(3, parquetEngine.RecordCount);
            Assert.AreEqual(2, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[0][0]);
            Assert.AreEqual("[1,2,3]", dataTable.Rows[0][0].ToString());
            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[0][1]);
            Assert.AreEqual("[abc,efg,hij]", dataTable.Rows[0][1].ToString());
            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[1][0]);
            Assert.AreEqual("[,1]", dataTable.Rows[1][0].ToString());
            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[2][1]);
            Assert.AreEqual(4, ((ListValue)dataTable.Rows[2][1]).Length);
            Assert.AreEqual("efg", ((ListValue)dataTable.Rows[2][1]).Data![0]);
            Assert.AreEqual(DBNull.Value, ((ListValue)dataTable.Rows[2][1]).Data![1]);
            Assert.AreEqual("xyz", ((ListValue)dataTable.Rows[2][1]).Data![3]);

            //Also try reading with a record offset
            dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 1, 1, default))(false);
            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[0][0]);
            Assert.AreEqual("[,1]", dataTable.Rows[0][0].ToString());
        }

        [TestMethod]
        public async Task LIST_TYPE_TEST2()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/LIST_TYPE_TEST2.parquet", default);

            Assert.AreEqual(8, parquetEngine.RecordCount);
            Assert.AreEqual(2, parquetEngine.Fields.Count);

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
            Assert.AreEqual(2, parquetEngine.Fields.Count);

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
            Assert.AreEqual(2, parquetEngine.Fields.Count);

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
            Assert.AreEqual(6, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.IsInstanceOfType<StructValue>(dataTable.Rows[0][0]);
            Assert.AreEqual("{\"appId\":null,\"version\":0,\"lastUpdated\":null}", ((StructValue)dataTable.Rows[0][0]).ToString());
            Assert.IsInstanceOfType<StructValue>(dataTable.Rows[0][1]);
            Assert.AreEqual("{\"path\":null,\"partitionValues\":null,\"size\":404,\"modificationTime\":1564524299000,\"dataChange\":false,\"stats\":null,\"tags\":null}", ((StructValue)dataTable.Rows[0][1]).ToString());
            Assert.IsInstanceOfType<StructValue>(dataTable.Rows[0][2]);
            Assert.AreEqual("{\"path\":null,\"deletionTimestamp\":null,\"dataChange\":false}", ((StructValue)dataTable.Rows[0][2]).ToString());
            Assert.AreEqual(DBNull.Value, dataTable.Rows[0][3]);
            Assert.IsInstanceOfType<StructValue>(dataTable.Rows[0][4]);
            Assert.AreEqual("{\"minReaderVersion\":1,\"minWriterVersion\":2}", ((StructValue)dataTable.Rows[0][4]).ToString());
            Assert.AreEqual(DBNull.Value, dataTable.Rows[0][5]);
            Assert.IsInstanceOfType<DBNull>(dataTable.Rows[9][4]);
            Assert.AreEqual(DBNull.Value, dataTable.Rows[9][4]);
            Assert.AreEqual("{\"appId\":\"e4a20b59-dd0e-4c50-b074-e8ae4786df30\",\"version\":null,\"lastUpdated\":1564524299648}", ((StructValue)dataTable.Rows[2][0]).ToString());
        }

        [TestMethod]
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
        ""app_version"": ""{Helpers.Env.AssemblyVersion.ToString()}""
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
            Assert.IsTrue(wasSuccess, "The event json we would have sent to Amplitude didn't match the expected value");
        }

        [TestMethod]
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
            Assert.IsTrue(wasSuccess, "Sensitive data wasn't stripped out correctly");
        }

        [TestMethod]
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
            Assert.IsTrue(wasSuccess, "Additional exception data wasn't added to the amplitude event as expected");
        }

        [TestMethod]
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

        [TestMethod]
        public async Task NULLABLE_GUID_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/NULLABLE_GUID_TEST.parquet", default);

            Assert.AreEqual(1, parquetEngine.RecordCount);
            Assert.AreEqual(33, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.AreEqual(false, dataTable.Rows[0][22]);
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
            Assert.AreEqual(320, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, 1, default))(false);
            Assert.AreEqual((byte)0, dataTable.Rows[0]["FLC K/L"]);
        }

        [TestMethod]
        public async Task ORACLE_MALFORMED_INT64_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/ORACLE_MALFORMED_INT64_TEST.parquet", default);

            Assert.AreEqual(126, parquetEngine.RecordCount);
            Assert.AreEqual(2, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);
            Assert.AreEqual("DEPOSIT", dataTable.Rows[0][0]);
            Assert.AreEqual((long)1, dataTable.Rows[0][1]);
        }

        [TestMethod]
        public async Task LIST_OF_STRUCTS_TEST1()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/LIST_OF_STRUCTS1.parquet", default);
            Assert.AreEqual(2, parquetEngine.RecordCount);
            Assert.AreEqual(2, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);

            Assert.AreEqual("Product1", dataTable.Rows[0][0]);
            Assert.AreEqual("Product2", dataTable.Rows[1][0]);

            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[0][1]);
            Assert.AreEqual("[{\"DateTime\":\"2024-04-15 22:00:00\",\"Quantity\":10},{\"DateTime\":\"2024-04-16 22:00:00\",\"Quantity\":20}]", dataTable.Rows[0][1].ToString());
            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[1][1]);
            Assert.AreEqual("[{\"DateTime\":\"2024-04-15 22:00:00\",\"Quantity\":30},{\"DateTime\":\"2024-04-16 22:00:00\",\"Quantity\":40}]", dataTable.Rows[1][1].ToString());
        }

        [TestMethod]
        public async Task LIST_OF_STRUCTS_TEST2()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/LIST_OF_STRUCTS2.parquet", default);
            Assert.AreEqual(1, parquetEngine.RecordCount);
            Assert.AreEqual(29, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);

            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[0][28]);
            Assert.AreEqual("[{\"purposeId\":\"HF85PyyGFprJXJvh5Pk9tg\",\"status\":\"Granted\",\"externalId\":\"General\",\"date\":\"2025-06-05 14:30:33\"}]", dataTable.Rows[0][28].ToString());
        }

        [TestMethod]
        public async Task EMPTY_LIST_OF_STRUCTS_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/EMPTY_LIST_OF_STRUCTS.parquet", default);
            Assert.AreEqual(2, parquetEngine.RecordCount);
            Assert.AreEqual(2, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);

            Assert.AreEqual("Product1", dataTable.Rows[0][0]);
            Assert.AreEqual("Product2", dataTable.Rows[1][0]);

            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[0][1]);
            Assert.AreEqual("[]", dataTable.Rows[0][1].ToString());
            Assert.IsInstanceOfType<ListValue>(dataTable.Rows[1][1]);
            Assert.AreEqual("[]", dataTable.Rows[1][1].ToString());
        }

        [TestMethod]
        public async Task PARQUET_MR_BREAKING_CHANGE_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/PARQUET-MR_1.15.0.parquet", default);
            Assert.AreEqual(5, parquetEngine.RecordCount);
            Assert.AreEqual(7, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);

            Assert.AreEqual(1, dataTable.Rows[0][0]);
            Assert.AreEqual(5, dataTable.Rows[4][0]);

            Assert.AreEqual("John Doe", dataTable.Rows[0][1]);
            Assert.AreEqual("David Lee", dataTable.Rows[4][1]);

            Assert.AreEqual(true, dataTable.Rows[0][4]);
            Assert.AreEqual(true, dataTable.Rows[4][4]);
        }

        [TestMethod]
        public async Task DECIMALS_WITH_NO_SCALE_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/DECIMALS_WITH_NO_SCALE_TEST.parquet", default);
            Assert.AreEqual(10589, parquetEngine.RecordCount);
            Assert.AreEqual(8, parquetEngine.Fields.Count);

            var dataTable = (await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default))(false);

            Assert.AreEqual(0.7072m, dataTable.Rows[0][5]);
            Assert.AreEqual(0m, dataTable.Rows[0][6]);
            Assert.AreEqual(0m, dataTable.Rows[0][7]);

            Assert.AreEqual(0.74527m, dataTable.Rows[100][5]);
            Assert.AreEqual(0m, dataTable.Rows[100][6]);
            Assert.AreEqual(0m, dataTable.Rows[100][7]);
        }

        [TestMethod]
        [DataRow("1.0", null)]
        [DataRow("1.0.0", "1.0.0.0")]
        [DataRow("1.0.0.0", "1.0.0.0")]
        [DataRow("1.0.0.0.0", null)]
        [DataRow("v1.0.0", "1.0.0.0")]
        [DataRow("v1.0.0.0", "1.0.0.0")]
        [DataRow("99.99.99", "99.99.99.0")]
        [DataRow("99.99.99.99", "99.99.99.99")]
        public void SEMANTIC_VERSION_PARSER_TEST(string versionNumber, string? expectedParsedVersionNumber)
        {
            var isExpectedToBeValid = expectedParsedVersionNumber is not null;
            Assert.AreEqual(SemanticVersion.TryParse(versionNumber, out var semanticVersion), isExpectedToBeValid);
            if (isExpectedToBeValid)
            {
                Assert.AreEqual(semanticVersion.ToString(), expectedParsedVersionNumber);
            }
        }

        [TestMethod]
        [DataRow("1.0.0", "1.0.1")]
        [DataRow("1.0.0.0", "1.0.0.1")]
        [DataRow("2.3.4", "3.0.1")]
        [DataRow("2.3.4.5", "3.0.0.1")]
        [DataRow("v1.2.3", "1.2.4")]
        [DataRow("v1.0.0.99", "1.0.1")]
        [DataRow("v99.98.99.99", "99.99.0.0")]
        public void SEMANTIC_VERSION_COMPARISON_TESTS(string smallerVersionNumber, string higherVersionNumber)
        {
            Assert.IsTrue(SemanticVersion.TryParse(smallerVersionNumber, out var smallerSemanticVersion), $"{smallerVersionNumber} is not a valid semantic version");
            Assert.IsTrue(SemanticVersion.TryParse(higherVersionNumber, out var higherSemanticVersion), $"{higherVersionNumber} is not a valid semantic version");
            Assert.IsTrue(smallerSemanticVersion < higherSemanticVersion, $"{smallerSemanticVersion} should have been lesser than {higherSemanticVersion}");
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
