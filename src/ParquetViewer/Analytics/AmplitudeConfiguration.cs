using System;
using System.Net.Http;

namespace ParquetViewer.Analytics;

public readonly struct AmplitudeConfiguration
{
    internal readonly string ApiKey { get; }
    public readonly Func<HttpMessageHandler> HttpMessageHandlerProvider { get; }
    public readonly IConsentProvider ConsentProvider { get; }

    public AmplitudeConfiguration(string apiKey, Func<HttpMessageHandler> httpMessageHandlerProvider, IConsentProvider consentProvider)
    {
        ArgumentNullException.ThrowIfNull(apiKey);
        ArgumentNullException.ThrowIfNull(httpMessageHandlerProvider);
        ArgumentNullException.ThrowIfNull(consentProvider);

        ApiKey = apiKey;
        HttpMessageHandlerProvider = httpMessageHandlerProvider;
        ConsentProvider = consentProvider;
    }
}