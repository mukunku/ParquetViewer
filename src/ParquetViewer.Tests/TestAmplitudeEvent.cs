using ParquetViewer.Analytics;
using System.Text.Json.Serialization;

namespace ParquetViewer.Tests
{
    public class TestAmplitudeEvent : AmplitudeEvent
    {
        public const string EVENT_TYPE = "unit.test.event";

        [JsonIgnore]
        public string? IgnoredProperty { get; set; }

        public string? RegularProperty { get; set; }

        public TestAmplitudeEvent(string dummyApiKey) : base(EVENT_TYPE)
        {
            base.AMPLITUDE_API_KEY = dummyApiKey;
        }

        public void SwapHttpClientHandler(HttpMessageHandler mockHandler)
        {
            HttpMessageHandler = mockHandler;
        }
    }
}
