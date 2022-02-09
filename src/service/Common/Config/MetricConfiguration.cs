namespace Microsoft.FeatureFlighting.Common.Config
{
    /// <summary>
    /// Configuration to update the evaluation metrics of a feature flight
    /// </summary>
    public class MetricConfiguration
    {
        /// <summary>
        /// Flag to enable metrics update
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Source of the metric (AI, Kusto, Custom)
        /// </summary>
        public WebhookConfiguration MetricSource { get; set; }

        /// <summary>
        /// Query configuration
        /// </summary>
        public KustoConfiguraton Kusto { get; set; }

        /// <summary>
        /// Name of the App insights instance where telemetry is sent
        /// </summary>
        public string AppInsightsName { get; set; }

        /// <summary>
        /// Name of the event used for tracking feature flight usage
        /// </summary>
        public string TrackingEventName { get; set; }

        public void MergeWithDefault(MetricConfiguration defaultConfiguration) 
        {
            if (Enabled == false)
                return;

            MetricSource ??= defaultConfiguration.MetricSource;
            AppInsightsName ??= defaultConfiguration.AppInsightsName;
            TrackingEventName ??= defaultConfiguration.TrackingEventName;
        }

        public static MetricConfiguration GetDefault()
        {
            return new MetricConfiguration
            {
                Enabled = true,
                AppInsightsName = "ai-feature-flights-management-prod   ",
                TrackingEventName = "Flighting:FeatureFlags:Evaluated"
            };
        }
    }
}
