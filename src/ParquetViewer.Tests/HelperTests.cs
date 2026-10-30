using ParquetViewer.Analytics;
using ParquetViewer.Controls;
using ParquetViewer.Helpers;
using ParquetViewer.Tests.TestHelpers;
using RichardSzalay.MockHttp;
using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ParquetViewer.Tests;

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
#if RELEASE_SELFCONTAINED || DEBUG_SELFCONTAINED
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
            ""cultureInfo"": ""{AppSettings.UserSelectedCulture ?? CultureInfo.CurrentUICulture}"",
            ""selfContainedExecutable"": {(isSelfContainedExecutable ? "true" : "false")}
        }},
        ""event_properties"": {{
            ""regularProperty"": ""yyy""
        }},
        ""session_id"": {TestAmplitudeEvent.SessionId},
        ""language"": ""{CultureInfo.CurrentUICulture.Name}"",
        ""os_name"": ""{Environment.OSVersion.Platform}"",
        ""os_version"": ""{Environment.OSVersion.VersionString}"",
        ""app_version"": ""{Env.AssemblyVersion}""
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

        bool wasSuccess = await testEvent.RecordAsync();
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

        bool wasSuccess = await testEvent.RecordAsync();
        Assert.IsTrue(wasSuccess, "Sensitive data wasn't stripped out correctly");
    }

    [TestMethod]
    [DataRow(@"Access to the path 'C:\Documents and Settings' is denied.", "Access to the path ***** is denied.")]
    [DataRow(@"The request failed due to a fatal device hardware error. : 'D:\data\file.parquet'.", "The request failed due to a fatal device hardware error. : *****.")]
    [DataRow(@"Could not find file '\\server\share\file.parquet'.", "Could not find file *****.")]
    [DataRow("Unable to cast object of type 'System.String' to type 'System.Int32'.", "Unable to cast object of type 'System.String' to type 'System.Int32'.")]
    public async Task AMPLITUDE_EXCEPTION_FILE_PATH_MASKING_TEST(string exceptionMessage, string expectedMessage)
    {
        var testAmplitudeEvent = TestAmplitudeEvent.MockRequest(out var mockHttpHandler);
        var testEvent = new ExceptionEvent(new IOException(exceptionMessage), testAmplitudeEvent.CloneAmplitudeConfiguration());

        //mock the http response
        _ = mockHttpHandler.Expect(HttpMethod.Post, "*").Respond(async (request) =>
        {
            string requestJsonBody = await (request.Content?.ReadAsStringAsync() ?? Task.FromResult(string.Empty));

            var requestJson = JsonNode.Parse(requestJsonBody);
            if (requestJson?["events"]?[0]?["event_properties"]?["message"]?.GetValue<string>() == expectedMessage)
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            else
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
        });

        bool wasSuccess = await testEvent.RecordAsync();
        Assert.IsTrue(wasSuccess, "File path wasn't masked correctly");
    }

    [TestMethod]
    public async Task AMPLITUDE_EXCEPTION_INNER_EXCEPTION_MASKING_TEST()
    {
        var testAmplitudeEvent = TestAmplitudeEvent.MockRequest(out var mockHttpHandler);
        var innerException = new IOException(@"Could not read `secret_column` from 'C:\secret\file.parquet'.");
        var testEvent = new ExceptionEvent(new Exception("Outer exception", innerException), testAmplitudeEvent.CloneAmplitudeConfiguration());

        //mock the http response
        _ = mockHttpHandler.Expect(HttpMethod.Post, "*").Respond(async (request) =>
        {
            string requestJsonBody = await (request.Content?.ReadAsStringAsync() ?? Task.FromResult(string.Empty));

            var requestJson = JsonNode.Parse(requestJsonBody);
            var innerExceptionText = requestJson?["events"]?[0]?["event_properties"]?["innerException"]?.GetValue<string>();
            if (innerExceptionText is not null
                && innerExceptionText.Contains($"System.IO.IOException: Could not read {ExceptionEvent.MASK_SENTINEL} from {ExceptionEvent.MASK_SENTINEL}.")
                && !innerExceptionText.Contains("secret"))
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            else
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
        });

        bool wasSuccess = await testEvent.RecordAsync();
        Assert.IsTrue(wasSuccess, "Inner exception message wasn't masked correctly");
    }

    [TestMethod]
    public void AMPLITUDE_EXCEPTION_SENSITIVE_FILE_DETAILS_MASKING_TEST()
    {
        //"SOURCE" is a substring of "INVESTOR_SOURCE_ID" so it must not leave part of the longer field name unmasked
        var sensitiveFileDetails = new SensitiveFileDetails(@"C:\data\investors.parquet", ["SOURCE", "INVESTOR_SOURCE_ID"]);

        var maskedMessage = sensitiveFileDetails.Mask(
            @"column 'INVESTOR_SOURCE_ID' is missing data page header, file is corrupt: C:\data\investors.parquet");

        Assert.AreEqual($"column '{ExceptionEvent.MASK_SENTINEL}' is missing data page header, file is corrupt: {ExceptionEvent.MASK_SENTINEL}", maskedMessage);
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

        bool wasSuccess = await testEvent.RecordAsync();
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
    public void SEMANTIC_VERSION_PARSER_TESTS(string versionNumber, string? expectedParsedVersionNumber)
    {
        var isExpectedToBeValid = expectedParsedVersionNumber is not null;
        Assert.AreEqual(SemanticVersion.TryParse(versionNumber, out var semanticVersion), isExpectedToBeValid);
        if (isExpectedToBeValid)
        {
            Assert.AreEqual(expectedParsedVersionNumber, semanticVersion.ToString());
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
        Assert.IsEmpty(ParquetGridView.GenerateFilterQuery([]));
    }

    [TestMethod]
    public void SingleStringValue_GeneratesEqualsClause()
    {
        var query = ParquetGridView.GenerateFilterQuery([
            ("Name", typeof(string), new object[] { "Alice" })
        ]);
        Assert.AreEqual("Name = 'Alice'", query);
    }

    [TestMethod]
    public void SingleIntValue_GeneratesEqualsClause()
    {
        var query = ParquetGridView.GenerateFilterQuery([
            ("Age", typeof(int), new object[] { 42 })
        ]);
        Assert.AreEqual("Age = 42", query);
    }

    [TestMethod]
    public void SingleDateTimeValue_GeneratesEqualsClause()
    {
        var dt = new DateTime(2024, 1, 2, 3, 4, 5, 678);
        var query = ParquetGridView.GenerateFilterQuery(
        [
            ("Created", typeof(DateTime), new object[] { dt })
        ]);
        Assert.AreEqual($"Created = #{dt:yyyy-MM-dd HH:mm:ss.FFFFFFF}#", query);
    }

    [TestMethod]
    public void MultipleValues_GeneratesInClause()
    {
        var query = ParquetGridView.GenerateFilterQuery(
        [
            ("Age", typeof(int), new object[] { 1, 2, 3 })
        ]);
        Assert.AreEqual("Age IN (1,2,3)", query);
    }

    [TestMethod]
    public void MultipleStringValues_GeneratesInClauseWithQuotes()
    {
        var query = ParquetGridView.GenerateFilterQuery(
        [
            ("City", typeof(string), new object[] { "London", "Paris" })
        ]);
        Assert.AreEqual("City IN ('London','Paris')", query);
    }

    [TestMethod]
    public void HandlesNullValue_GeneratesIsNull()
    {
        var query = ParquetGridView.GenerateFilterQuery(
        [
            ("Name", typeof(string), new object[] { null! })
        ]);
        Assert.AreEqual("Name IS NULL", query);
    }

    [TestMethod]
    public void HandlesDBNullValue_GeneratesIsNull()
    {
        var query = ParquetGridView.GenerateFilterQuery(
        [
            ("Name", typeof(string), new object[] { DBNull.Value })
        ]);
        Assert.AreEqual("Name IS NULL", query);
    }

    [TestMethod]
    public void HandlesNullAndNonNullValues_GeneratesOrIsNull()
    {
        var query = ParquetGridView.GenerateFilterQuery(
        [
            ("Age", typeof(int), new object[] { 1, null! })
        ]);
        Assert.AreEqual("(Age IN (1) OR Age IS NULL)", query);
    }

    [TestMethod]
    public void HandlesNullAndNonNullValues_GeneratesOrIsNullAndCombinesWithAnd()
    {
        var query = ParquetGridView.GenerateFilterQuery(
        [
            ("Age", typeof(int), new object[] { 1, null! }),
            ("Name", typeof(string), new object[] { "Alice", "Alice" })
        ]);
        Assert.AreEqual("(Age IN (1) OR Age IS NULL) AND Name = 'Alice'", query);
    }

    [TestMethod]
    public void HandlesMultipleColumns_CombinesWithAnd()
    {
        var query = ParquetGridView.GenerateFilterQuery(
        [
            ("Name", typeof(string), new object[] { "Alice" }),
            ("Age", typeof(int), new object[] { 30 })
        ]);
        Assert.AreEqual("Name = 'Alice' AND Age = 30", query);
    }

    [TestMethod]
    public void ColumnNameWithSpaces_IsWrappedInBrackets()
    {
        var query = ParquetGridView.GenerateFilterQuery(
        [
            ("First Name", typeof(string), new object[] { "Bob" })
        ]);
        Assert.AreEqual("[First Name] = 'Bob'", query);
    }

    [TestMethod]
    public void FloatScientificNotation_IsWrappedInQuotes()
    {
        var query = ParquetGridView.GenerateFilterQuery(
        [
            ("Value", typeof(double), new object[] { 1.23e20 })
        ]);
        Assert.AreEqual("Value = '1.23E+20'", query);
    }

    [TestMethod]
    [DataRow("de-DE")] //comma decimal separator
    [DataRow("fr-FR")] //comma decimal separator + narrow no-break group separator
    [DataRow("th-TH")] //non-Gregorian calendar by default
    public void NumericAndDateFilters_AreCultureInvariant(string cultureName)
    {
        //DataView.RowFilter always expects '.' as the decimal separator and a Gregorian date,
        //regardless of the user's locale. Without invariant formatting, a culture that uses ','
        //would corrupt the filter since ',' also separates values inside an `IN (...)` clause.
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(cultureName);

            Assert.AreEqual("Value = 1.5", ParquetGridView.GenerateFilterQuery(
            [
                ("Value", typeof(double), new object[] { 1.5d })
            ]));

            Assert.AreEqual("Value = '1.23E+20'", ParquetGridView.GenerateFilterQuery(
            [
                ("Value", typeof(double), new object[] { 1.23e20 })
            ]));

            Assert.AreEqual("Value IN (1.5,2.5)", ParquetGridView.GenerateFilterQuery(
            [
                ("Value", typeof(double), new object[] { 1.5d, 2.5d })
            ]));

            Assert.AreEqual("Value = #2024-01-31 13:45:30#", ParquetGridView.GenerateFilterQuery(
            [
                ("Value", typeof(DateTime), new object[] { new DateTime(2024, 1, 31, 13, 45, 30) })
            ]));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [TestMethod]
    public void ByteArrayValue_IsCorrectlyTruncated()
    {
        var byteArrayValue = new Engine.Types.ByteArrayValue([0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10]);
        Assert.AreEqual("01[...]10", byteArrayValue.ToStringTruncated(1));
        Assert.AreEqual("01[...]10", byteArrayValue.ToStringTruncated(2));
        Assert.AreEqual("01-02[...]09-10", byteArrayValue.ToStringTruncated(11));
        Assert.AreEqual("01-02-03-04[...]07-08-09-10", byteArrayValue.ToStringTruncated(28));
        Assert.AreEqual("01-02-03-04-05-06-07-08-09-10", byteArrayValue.ToStringTruncated(29));
        Assert.AreEqual("01-02-03-04-05-06-07-08-09-10", byteArrayValue.ToString());
    }

    /// <summary>
    /// Verifies that the date/time format strings returned by <see cref="ExtensionMethods.GetDateOnlyFormat"/>
    /// and <see cref="ExtensionMethods.GetTimeOnlyFormat"/> (and the custom-format strip helpers they rely on)
    /// can actually be used to render a <see cref="DateOnly"/>/<see cref="TimeOnly"/> in many different cultures.
    /// </summary>
    /// <remarks>An empty formatting result is considered valid; only exceptions are treated as failures.</remarks>
    [TestMethod]
    public void GET_DATE_AND_TIME_ONLY_FORMATS_ARE_VALID_FOR_CULTURE()
    {
        string[] cultures = [
            //Invariant-ish / common western cultures
            "en-US", "en-GB", "en-CA", "en-AU", "en-IN",
            "de-DE", "de-AT", "de-CH",
            "fr-FR", "fr-CA", "fr-BE",
            "es-ES", "es-MX", "es-AR",
            "it-IT", "pt-BR", "pt-PT",
            "nl-NL", "nb-NO", "sv-SE", "da-DK", "fi-FI",
            //Central / eastern europe (some use '.' date separators)
            "pl-PL", "cs-CZ", "sk-SK", "hu-HU", "ro-RO", "bg-BG", "el-GR",
            "ru-RU", "uk-UA", "tr-TR",
            //Right-to-left / non-Gregorian calendars
            "ar-SA", "ar-EG", "ar-AE", "he-IL", "fa-IR",
            //Asia / pacific
            "hi-IN", "bn-IN", "ta-IN", "th-TH", "vi-VN", "id-ID", "ms-MY",
            "ja-JP", "ko-KR", "zh-CN", "zh-TW", "zh-HK",
            //Africa / other
            "af-ZA", "sw-KE", "am-ET", "ka-GE", "hy-AM", "az-AZ", "uz-UZ",
            "kk-KZ", "mn-MN", "ne-NP", "si-LK", "km-KH", "lo-LA", "my-MM",
            "ur-PK", "ps-AF",
        ];

        var originalCulture = CultureInfo.CurrentCulture;
        var failures = new List<string>();
        try
        {
            foreach (var cultureName in cultures)
            {
                var culture = new CultureInfo(cultureName);
                CultureInfo.CurrentCulture = culture;

                //The culture's own date+time layout, i.e. what a user of that culture would naturally
                //enter into the custom date format dialog.
                var customFormat =
                    $"{culture.DateTimeFormat.ShortDatePattern} {culture.DateTimeFormat.LongTimePattern}";

                foreach (var dateFormat in new[] { DateFormat.Default, DateFormat.ISO8601, DateFormat.Custom })
                {
                    var dateOnlyFormat = dateFormat == DateFormat.Custom
                        ? UtilityMethods.StripTimeComponentsFromDateTimeFormat(customFormat)
                        : dateFormat.GetDateOnlyFormat();
                    var timeOnlyFormat = dateFormat == DateFormat.Custom
                        ? UtilityMethods.StripDateComponentsFromDateTimeFormat(customFormat)
                        : dateFormat.GetTimeOnlyFormat();

                    try
                    {
                        //An empty result is fine; we only care that the format doesn't blow up.
                        _ = new DateOnly(2024, 1, 2).ToString(dateOnlyFormat);
                    }
                    catch (Exception ex)
                    {
                        failures.Add($"{cultureName} | {dateFormat} | DateOnly | '{dateOnlyFormat}' -> {ex.GetType().Name}: {ex.Message}");
                    }

                    try
                    {
                        //An empty result is fine; we only care that the format doesn't blow up.
                        _ = new TimeOnly(13, 45, 30).ToString(timeOnlyFormat);
                    }
                    catch (Exception ex)
                    {
                        failures.Add($"{cultureName} | {dateFormat} | TimeOnly | '{timeOnlyFormat}' -> {ex.GetType().Name}: {ex.Message}");
                    }
                }
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }

        Assert.IsEmpty(failures,
            $"The following culture/format combinations failed to render:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
    }
}