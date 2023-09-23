namespace ParquetViewer.Analytics
{
    public interface IConsentProvider
    {
        public bool AnalyticsDataGatheringConsent { get; }
    }
}
