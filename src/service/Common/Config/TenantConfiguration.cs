using System;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Common.Config
{
    /// <summary>
    /// Configuration for tenants boarded to the Flighting Service
    /// </summary>
    public class TenantConfiguration: ICloneable
    {
        /// <summary>
        /// Name of the tenant
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Alternate/Short name of the tenant
        /// </summary>
        public string ShortName { get; set; }
        
        /// <summary>
        /// Configuration for authorizing apps to administer the tenant's feature flags in Azure App Configuration
        /// </summary>
        public AuthorizationConfiguration Authorization { get; set; }
        
        /// <summary>
        /// Configuration for setting up cache
        /// </summary>
        public CacheConfiguration Cache { get; set; }

        /// <summary>
        /// Gets a default <see cref="TenantConfiguration"/>
        /// </summary>
        /// <returns>Default tenant configuration</returns>
        public static TenantConfiguration GetDefault()
        {
            return new TenantConfiguration()
            {
                Name = "Default",
                ShortName = "Default",
                Authorization = new AuthorizationConfiguration(),
                Cache = new CacheConfiguration()
            };
        }

        /// <summary>
        /// Clones the configuration into another object
        /// </summary>
        /// <returns>Cloned configuration</returns>
        public object Clone()
        {
            string serializedConfiguration = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<TenantConfiguration>(serializedConfiguration);
        }

        /// <summary>
        /// Merges the tenant configuration values with the given default configuration
        /// </summary>
        /// <param name="defaultTenantConfiguration" cref="TenantConfiguration">Tenant configuration with default values</param>
        public void MergeWithDefault(TenantConfiguration defaultTenantConfiguration)
        {
            if (defaultTenantConfiguration == null)
                return;

            if (Authorization == null)
                Authorization = defaultTenantConfiguration.Authorization;
            else
                Authorization.MergeWithDefault(defaultTenantConfiguration.Authorization);

            if (Cache == null)
                Cache = defaultTenantConfiguration.Cache;
            else
                Cache.MergeWithDefault(defaultTenantConfiguration.Cache);
        }
    }

    /// <summary>
    /// Configuration for authorizing apps to administer the tenant's feature flags in Azure App Configuration
    /// </summary>
    public class AuthorizationConfiguration
    {
        public AuthorizationConfiguration() { }

        /// <summary>
        /// Type of authorization (configuration or service-based)
        /// </summary>
        /// <value>Configuration</value>
        public string Type { get; set; }
        
        /// <summary>
        /// Comma-separated UPN (for user) or App ID (for applications) of administrators
        /// </summary>
        public string Administrators { get; set; }
        
        /// <summary>
        /// Name of the current application
        /// </summary>
        public string SenderAppName { get; set; }

        /// <summary>
        /// Gets a list of administrators (split from comma-separated Administers field)
        /// </summary>
        /// <returns>UPN (for user) or App ID (for applications) of administrators</returns>
        public IEnumerable<string> GetAdministrators()
        {
            if (string.IsNullOrWhiteSpace(Administrators))
                return new List<string>();

            return Administrators.Split(",").ToList();
        }

        /// <summary>
        /// Merges the configuration values with default authorization configuration
        /// </summary>
        /// <param name="defaultConfiguration" cref="AuthorizationConfiguration">Configuration with default values</param>
        public void MergeWithDefault(AuthorizationConfiguration defaultConfiguration)
        {
            if (defaultConfiguration == null)
                return;

            Type = !string.IsNullOrWhiteSpace(Type) ? Type : defaultConfiguration.Type;
            Administrators = !string.IsNullOrWhiteSpace(Administrators) ? Administrators : defaultConfiguration.Administrators;
            SenderAppName = !string.IsNullOrWhiteSpace(SenderAppName) ? SenderAppName : defaultConfiguration.SenderAppName;
        }
    }

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
        public string FeatureFlagNames { get;set; }

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
