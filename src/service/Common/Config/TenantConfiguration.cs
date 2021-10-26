using System;
using Newtonsoft.Json;

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
        /// Configuration for using Business Rule Engine filters
        /// </summary>
        public BusinessRuleEngineConfiguration BusinessRuleEngine { get; set; }

        /// <summary>
        /// Configuration when feature flags are evaluated
        /// </summary>
        public FlagEvaluationConfiguration Evaluation { get; set; }

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
                Cache = new CacheConfiguration(),
                Evaluation = FlagEvaluationConfiguration.GetDefault()
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

            if (Evaluation == null)
                Evaluation = defaultTenantConfiguration.Evaluation;
        }

        public bool IsBusinessRuleEngineEnabled()
        {
            return BusinessRuleEngine != null && BusinessRuleEngine.Enabled && BusinessRuleEngine.Storage != null;
        }
    }
}
