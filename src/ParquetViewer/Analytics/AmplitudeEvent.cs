using ParquetViewer.Helpers;
using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ParquetViewer.Analytics
{
    public abstract class AmplitudeEvent
    {
        //The api key is meant to be public: https://www.docs.developers.amplitude.com/guides/amplitude-keys-guide/#api-key
        protected string AMPLITUDE_API_KEY = ""; //This will only be populated for official releases

        private static readonly long _sessionId = DateTime.UtcNow.ToMillisecondsSinceEpoch();
        private static readonly int _systemRAM = (int)(GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1048576.0 /*magic number*/);

        protected HttpMessageHandler HttpMessageHandler { get; set; } = new HttpClientHandler();

        [JsonIgnore]
        public string DeviceId => AppSettings.AnalyticsDeviceId.ToString();

        [JsonIgnore]
        public string EventType { get; }

        [JsonIgnore]
        public long SessionId => _sessionId;

        [JsonIgnore]
        public object UserProperties => new
        {
            AppSettings.RememberLastRowCount,
            AppSettings.LastRowCount,
            AppSettings.AlwaysSelectAllFields,
            AutoSizeColumnsMode = AppSettings.AutoSizeColumnsMode.ToString(),
            DateTimeDisplayFormat = AppSettings.DateTimeDisplayFormat.ToString(),
            SystemMemory = _systemRAM,
            Environment.ProcessorCount
        };

        protected AmplitudeEvent(string eventType)
        {
            EventType = eventType;
        }

        public async Task<bool> Record()
        {
            try
            {
                if (AMPLITUDE_API_KEY.Length == 0 || !AppSettings.AnalyticsDataGatheringConsent)
                    return false;

                var request = new
                {
                    api_key = string.Join(string.Empty, Base64Decode(AMPLITUDE_API_KEY)),
                    events = new[] {
                    new {
                        device_id = DeviceId,
                        event_type = EventType,
                        user_properties = UserProperties,
                        event_properties = Convert.ChangeType(this, GetType()), //If we don't cast it to the correct child type, its properties don't get picked up
                        session_id = _sessionId,
                        language = CultureInfo.CurrentUICulture.Name,
                        os_name = Environment.OSVersion.Platform.ToString(),
                        os_version = Environment.OSVersion.VersionString,
                        app_version = AboutBox.AssemblyVersion
                        }
                    }
                };

                var result = await new HttpClient(this.HttpMessageHandler).PostAsync("https://api2.amplitude.com/2/httpapi", JsonContent.Create(request));
                return result.IsSuccessStatusCode;
            }
            catch { /* Analytics is best effort. If it fails, it fails */ }

            return false;
        }

        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}