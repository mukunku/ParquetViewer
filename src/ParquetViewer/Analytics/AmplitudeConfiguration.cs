using System;
using System.Net.Http;

namespace ParquetViewer.Analytics
{
    public readonly struct AmplitudeConfiguration
    {
        internal readonly string ApiKey { get; }
        public readonly Func<HttpMessageHandler> HttpMessageHandlerProvider { get; }
        public readonly IConsentProvider ConsentProvider { get; }

        public AmplitudeConfiguration(string apiKey, Func<HttpMessageHandler> httpMessageHandlerProvider, IConsentProvider consentProvider)
        {
            if (apiKey is null) throw new ArgumentNullException(nameof(apiKey));
            if (httpMessageHandlerProvider is null) throw new ArgumentNullException(nameof(httpMessageHandlerProvider));
            if (consentProvider is null) throw new ArgumentNullException(nameof(consentProvider));

            ApiKey = apiKey;
            HttpMessageHandlerProvider = httpMessageHandlerProvider;
            ConsentProvider = consentProvider;
        }
    }
}