namespace Microsoft.FeatureFlighting.Common.Config
{
    /// <summary>
    /// Configuration for setting up cache
    /// </summary>
    public class CacheConfiguration
    {
        public CacheConfiguration() { }

        /// <summary>
        /// Type of default cache
        /// </summary>
        /// <value>InMemory,Redis,URP</value>
        public string Type { get; set; }

        /// <summary>
        /// Type of cache for caching feature flags
        /// </summary>
        public string FeatureFlags { get; set; }

        /// <summary>
        /// Type of cache for caching graph data
        /// </summary>
        public string Graph { get; set; }

        /// <summary>
        /// Type of cache for caching the names of all feature flags
        /// </summary>
        public string FeatureFlagNames { get; set; }

        /// <summary>
        /// Type of cache for caching the filters and operator mapping
        /// </summary>
        public string OperatorMapping { get; set; }

        /// <summary>
        /// Type of cache for caching rule engine workflows
        /// </summary>
        public string RulesEngine { get; set; }

        /// <summary>
        /// Unified Redis Platform settings
        /// </summary>
        /// <remarks>https://github.com/microsoft/UnifiedRedisPlatform.Core</remarks>
        public UrpConfiguration URP { get; set; }

        /// <summary>
        /// Configuration to connect to Redis cache
        /// </summary>
        public RedisConfiguration Redis { get; set; }

        public string GetCacheType(string operation)
        {
            if (string.IsNullOrWhiteSpace(operation))
                return Type;

            if (operation.ToLowerInvariant() == nameof(FeatureFlags).ToLowerInvariant())
                return FeatureFlags;

            if (operation.ToLowerInvariant() == nameof(FeatureFlagNames).ToLowerInvariant())
                return FeatureFlagNames;

            if (operation.ToLowerInvariant() == nameof(Graph).ToLowerInvariant())
                return Graph;

            if (operation.ToLowerInvariant() == nameof(RulesEngine).ToLowerInvariant())
                return RulesEngine;

            if (operation.ToLowerInvariant() == nameof(OperatorMapping).ToLowerInvariant())
                return OperatorMapping;

            return Type;
        }

        /// <summary>
        /// Merges the cache configuration with default configuration
        /// </summary>
        /// <param name="defaultConfiguration" cref="CacheConfiguration">Configuration with default values</param>
        public void MergeWithDefault(CacheConfiguration defaultConfiguration)
        {
            if (defaultConfiguration == null)
                return;

            Type = !string.IsNullOrWhiteSpace(Type) ? Type : defaultConfiguration.Type;
            FeatureFlags = !string.IsNullOrWhiteSpace(FeatureFlags) ? FeatureFlags : defaultConfiguration.FeatureFlags;
            Graph = !string.IsNullOrWhiteSpace(Graph) ? Graph : defaultConfiguration.Graph;
            FeatureFlagNames = !string.IsNullOrWhiteSpace(FeatureFlagNames) ? FeatureFlagNames : defaultConfiguration.FeatureFlagNames;
            RulesEngine = !string.IsNullOrWhiteSpace(RulesEngine) ? RulesEngine : defaultConfiguration.RulesEngine;
            OperatorMapping = !string.IsNullOrWhiteSpace(OperatorMapping) ? OperatorMapping : defaultConfiguration.OperatorMapping;
            URP ??= defaultConfiguration.URP;
        }
    }

    public class UrpConfiguration
    {
        public UrpConfiguration() { }

        public string Cluster { get; set; }
        public string App { get; set; }
        public string Location { get; set; }
        public string SecretKey { get; set; }
    }

    public class RedisConfiguration
    {
        public RedisConfiguration() { }

        public string ConnectionStringLocation { get; set; }
        public int Timeout { get; set; }
    }
}
