namespace Microsoft.FeatureFlighting.Common.Config
{
    /// <summary>
    /// Configuration when feature flags are evaluated
    /// </summary>
    public class FlagEvaluationConfiguration
    {
        /// <summary>
        /// Flag to indicate if exceptions should be ignored when evaluating feature flags
        /// </summary>
        public bool IgnoreException { get; set; }
        
        /// <summary>
        /// Flag to indicate if additional messages should be added when feature flag is disabled
        /// </summary>
        public bool AddDisabledContext { get; set; }

        /// <summary>
        /// Gets a default <see cref="FlagEvaluationConfiguration"/>
        /// </summary>
        /// <returns></returns>
        public static FlagEvaluationConfiguration GetDefault()
        {
            return new FlagEvaluationConfiguration()
            {
                IgnoreException = false,
                AddDisabledContext = false
            };
        }
    }
}
