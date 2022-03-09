namespace Microsoft.FeatureFlighting.Common.Config
{
    /// <summary>
    /// Configuration for optimizing feature flights when saving to Azure App Configuration
    /// </summary>
    public class FlightOptimizationConfiguration
    {
        /// <summary>
        /// Enables optimization
        /// </summary>
        public bool EnableOptimization { get; set; }
        
        /// <summary>
        /// Optimization rules (colon ; separater) to be applied for optimizing the feature flight
        /// </summary>
        public string OptimizationRules { get; set; }

        public void MergeWithDefault(FlightOptimizationConfiguration defaultConfiguration)
        {
            if (defaultConfiguration == null)
                return;
                
            if (string.IsNullOrWhiteSpace(OptimizationRules))
                OptimizationRules = defaultConfiguration.OptimizationRules;
        }

        public static FlightOptimizationConfiguration GetDefault()
        {
            return new FlightOptimizationConfiguration
            {
                EnableOptimization = false,
                OptimizationRules = string.Empty
            };
        }
    }
}
