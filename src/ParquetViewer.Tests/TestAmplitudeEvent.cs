using ParquetViewer.Analytics;
using RichardSzalay.MockHttp;
using System.Text.Json.Serialization;

namespace ParquetViewer.Tests
{
    public class TestAmplitudeEvent : AmplitudeEvent
    {
        public const string EVENT_TYPE = "unit.test.event";
        public const string API_KEY = "ZHVtbXk="; //dummy

        [JsonIgnore]
        public string? IgnoredProperty { get; set; }

        public string? RegularProperty { get; set; }

        [JsonIgnore]
        public HttpMessageHandler HttpMessageHandler { get; }

        private TestAmplitudeEvent(HttpMessageHandler httpMessageHandler)
            : base(EVENT_TYPE, new AmplitudeConfiguration(API_KEY, () => httpMessageHandler, new AlwaysTrueConsentProvider()))
        {
            HttpMessageHandler = httpMessageHandler;
        }

        public static TestAmplitudeEvent MockRequest(out MockHttpMessageHandler mockHttpHandler)
        {
            mockHttpHandler = new MockHttpMessageHandler();
            return new TestAmplitudeEvent(mockHttpHandler);
        }

        public AmplitudeConfiguration CloneAmplitudeConfiguration() =>
            new(API_KEY, () => HttpMessageHandler, new AlwaysTrueConsentProvider());

        /// <summary>
        /// Always provides consent. For testing purposes.
        /// </summary>
        private class AlwaysTrueConsentProvider : IConsentProvider
        {
            public bool AnalyticsDataGatheringConsent => true;
        }
    }
}