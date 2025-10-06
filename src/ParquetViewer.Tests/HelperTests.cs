using ParquetViewer.Analytics;
using ParquetViewer.Controls;
using ParquetViewer.Helpers;
using RichardSzalay.MockHttp;
using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ParquetViewer.Tests
{
    [TestClass]
    public class HelperTests
    {
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

        [TestMethod]
        public void ReturnsEmptyString_WhenInputIsNull()
        {
            Assert.IsEmpty(ParquetGridView.GenerateFilterQuery(null!));
        }

        [TestMethod]
        public void ReturnsEmptyString_WhenInputIsEmpty()
        {
            Assert.IsEmpty(ParquetGridView.GenerateFilterQuery(new()));
        }

        [TestMethod]
        public void SingleStringValue_GeneratesEqualsClause()
        {
            var query = ParquetGridView.GenerateFilterQuery(new()
            {
                ("Name", typeof(string), new object[] { "Alice" })
            });
            Assert.AreEqual("Name = 'Alice'", query);
        }

        [TestMethod]
        public void SingleIntValue_GeneratesEqualsClause()
        {
            var query = ParquetGridView.GenerateFilterQuery(new()
            {
                ("Age", typeof(int), new object[] { 42 })
            });
            Assert.AreEqual("Age = 42", query);
        }

        [TestMethod]
        public void SingleDateTimeValue_GeneratesEqualsClause()
        {
            var dt = new DateTime(2024, 1, 2, 3, 4, 5, 678);
            var query = ParquetGridView.GenerateFilterQuery(new()
            {
                ("Created", typeof(DateTime), new object[] { dt })
            });
            Assert.AreEqual($"Created = #{dt:yyyy-MM-dd HH:mm:ss.FFFFFFF}#", query);
        }

        [TestMethod]
        public void MultipleValues_GeneratesInClause()
        {
            var query = ParquetGridView.GenerateFilterQuery(new()
            {
                ("Age", typeof(int), new object[] { 1, 2, 3 })
            });
            Assert.AreEqual("Age IN (1,2,3)", query);
        }

        [TestMethod]
        public void MultipleStringValues_GeneratesInClauseWithQuotes()
        {
            var query = ParquetGridView.GenerateFilterQuery(new()
            {
                ("City", typeof(string), new object[] { "London", "Paris" })
            });
            Assert.AreEqual("City IN ('London','Paris')", query);
        }

        [TestMethod]
        public void HandlesNullValue_GeneratesIsNull()
        {
            var query = ParquetGridView.GenerateFilterQuery(new()
            {
                ("Name", typeof(string), new object[] { null! })
            });
            Assert.AreEqual("Name IS NULL", query);
        }

        [TestMethod]
        public void HandlesDBNullValue_GeneratesIsNull()
        {
            var query = ParquetGridView.GenerateFilterQuery(new()
            {
                ("Name", typeof(string), new object[] { DBNull.Value })
            });
            Assert.AreEqual("Name IS NULL", query);
        }

        [TestMethod]
        public void HandlesNullAndNonNullValues_GeneratesOrIsNull()
        {
            var query = ParquetGridView.GenerateFilterQuery(new()
            {
                ("Age", typeof(int), new object[] { 1, null })
            });
            Assert.AreEqual("(Age IN (1) OR Age IS NULL)", query);
        }

        [TestMethod]
        public void HandlesNullAndNonNullValues_GeneratesOrIsNullAndCombinesWithAnd()
        {
            var query = ParquetGridView.GenerateFilterQuery(new()
            {
                ("Age", typeof(int), new object[] { 1, null }),
                ("Name", typeof(string), new object[] { "Alice", "Alice" })
            });
            Assert.AreEqual("(Age IN (1) OR Age IS NULL) AND Name = 'Alice'", query);
        }

        [TestMethod]
        public void HandlesMultipleColumns_CombinesWithAnd()
        {
            var query = ParquetGridView.GenerateFilterQuery(new()
            {
                ("Name", typeof(string), new object[] { "Alice" }),
                ("Age", typeof(int), new object[] { 30 })
            });
            Assert.AreEqual("Name = 'Alice' AND Age = 30", query);
        }

        [TestMethod]
        public void ColumnNameWithSpaces_IsWrappedInBrackets()
        {
            var query = ParquetGridView.GenerateFilterQuery(new()
            {
                ("First Name", typeof(string), new object[] { "Bob" })
            });
            Assert.AreEqual("[First Name] = 'Bob'", query);
        }

        [TestMethod]
        public void FloatScientificNotation_IsWrappedInQuotes()
        {
            var query = ParquetGridView.GenerateFilterQuery(new()
            {
                ("Value", typeof(double), new object[] { 1.23e20 })
            });
            Assert.AreEqual("Value = '1.23E+20'", query);
        }
    }
}
