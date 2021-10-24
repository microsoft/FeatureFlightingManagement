namespace Microsoft.FeatureFlighting.Common.Config
{
    /// <summary>
    /// Configuration for using business rule engine as filter
    /// </summary>
    public class BusinessRuleEngineConfiguration
    {
        /// <summary>
        /// Flag to indicate if BRE evaluation is enabledd
        /// </summary>
        public bool Enabled { get; set; }
        
        /// <summary>
        /// Storage configuration where BRE workflows are located
        /// </summary>
        public StorageConfiguration Storage { get; set; }

        /// <summary>
        /// Mins to cache the business rule workflow (values lower than 1 mean no-cache)
        /// </summary>
        public int CacheDuration { get; set; }
    }
}
