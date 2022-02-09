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
        /// Configuration for flights database
        /// </summary>
        public CosmosDbConfiguration FlightsDatabase { get; set; }

        /// <summary>
        /// Configuration for optimizing feature flags when saving to Azure
        /// </summary>
        public FlightOptimizationConfiguration Optimization { get; set; }

        /// <summary>
        /// Configuration when feature flags are evaluated
        /// </summary>
        public FlagEvaluationConfiguration Evaluation { get; set; }

        /// <summary>
        /// Configuration to subscribe to change notifications
        /// </summary>
        public TenantChangeNotificationConfiguration ChangeNotificationSubscription { get; set; }

        /// <summary>
        /// Configuration to send alerts
        /// </summary>
        public IntelligentAlertConfiguration IntelligentAlerts { get; set; }

        /// <summary>
        /// Configuration to update the metrics
        /// </summary>
        public MetricConfiguration Metrics { get; set; }

        /// <summary>
        /// Email address for contacting the tenant
        /// </summary>
        public string Contact { get; set; }

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
                Evaluation = FlagEvaluationConfiguration.GetDefault(),
                FlightsDatabase = new CosmosDbConfiguration(),
                Optimization = FlightOptimizationConfiguration.GetDefault(),
                ChangeNotificationSubscription = TenantChangeNotificationConfiguration.GetDefault(),
                Metrics = MetricConfiguration.GetDefault()
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

            if (FlightsDatabase == null)
                FlightsDatabase = defaultTenantConfiguration.FlightsDatabase;
            else
                FlightsDatabase.MergeWithDefault(defaultTenantConfiguration.FlightsDatabase);

            if (Optimization == null)
                Optimization = defaultTenantConfiguration.Optimization;
            else
                Optimization.MergeWithDefault(defaultTenantConfiguration.Optimization);

            if (ChangeNotificationSubscription == null)
                ChangeNotificationSubscription = defaultTenantConfiguration.ChangeNotificationSubscription;
            else
                ChangeNotificationSubscription.MergeOrDefault(defaultTenantConfiguration.ChangeNotificationSubscription);

            if (Evaluation == null)
                Evaluation = defaultTenantConfiguration.Evaluation;

            if (IntelligentAlerts == null)
                IntelligentAlerts = defaultTenantConfiguration.IntelligentAlerts;
            else
                IntelligentAlerts.MergeWithDefault(defaultTenantConfiguration.IntelligentAlerts);

            if (Metrics == null)
                Metrics = defaultTenantConfiguration.Metrics;
            else
                Metrics.MergeWithDefault(defaultTenantConfiguration.Metrics);

        }

        /// <summary>
        /// Checks if BRE is enabled for the tenant
        /// </summary>
        /// <returns>True of BRE is enabled for the tenant</returns>
        public bool IsBusinessRuleEngineEnabled()
        {
            return BusinessRuleEngine != null && BusinessRuleEngine.Enabled && BusinessRuleEngine.Storage != null;
        }

        public bool IsReportingEnabled()
        {
            return IntelligentAlerts != null && IntelligentAlerts.Enabled &&
                (IntelligentAlerts.MaximumActivePeriodAlertEnabled || IntelligentAlerts.MaximumDisabledPeriodAlertEnabled || IntelligentAlerts.MaximumUnusedPeriodAlertEnabled);
        }
    }
}
