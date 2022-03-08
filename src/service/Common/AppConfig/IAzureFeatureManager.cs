using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Common.AppConfig
{
    /// <summary>
    /// Manages feature flags in Azure App Configuration
    /// </summary>
    public interface IAzureFeatureManager
    {
        /// <summary>
        /// Gets a feature flag from Azure App Configuration
        /// </summary>
        /// <param name="name">Name of the feature flight</param>
        /// <param name="tenant">Tenant name</param>
        /// <param name="environment">Environment name</param>
        /// <param name="trackingIds" cref="LoggerTrackingIds">Tracking IDs</param>
        /// <returns cref="AzureFeatureFlag">Azure feature flag</returns>
        Task<AzureFeatureFlag?> Get(string name, string tenant, string environment, LoggerTrackingIds trackingIds);

        /// <summary>
        /// Gets a collection of all feature flags in the given tenant and environment
        /// </summary>
        /// <param name="tenant">Name of the tenant</param>
        /// <param name="environment">Name of the environment</param>
        /// <returns cref="IEnumerable{AzureFeatureFlag}">Collection of feature flags</returns>
        Task<IEnumerable<AzureFeatureFlag>> Get(string tenant, string environment);

        /// <summary>
        /// Creates a feature flag in Azure App Config
        /// </summary>
        /// <param name="featureFlag" cref="AzureFeatureFlag">Azure feature flag to be created</param>
        /// <param name="trackingIds" cref="LoggerTrackingIds">Tracking Ids</param>
        Task Create(AzureFeatureFlag featureFlag, LoggerTrackingIds trackingIds);

        /// <summary>
        /// Updates an existing feature flag in Azure App Config
        /// </summary>
        /// <param name="featureFlag"c cref="AzureFeatureFlag">Updated feature flag</param>
        /// <param name="trackingIds" cref="LoggerTrackingIds">Tracking Ids</param>
        Task Update(AzureFeatureFlag featureFlag, LoggerTrackingIds trackingIds);

        /// <summary>
        /// Updates the status (Enabled or Disabled) of a feature flag in Azure App Config
        /// </summary>
        /// <param name="name">Name of the feature flight</param>
        /// <param name="tenant">Tenant name</param>
        /// <param name="environment">Environment name</param>
        /// <param name="newStatus">New status of the feature flag</param>
        /// <param name="trackingIds" cref="LoggerTrackingIds">Tracking IDs</param>
        Task ChangeStatus(string name, string tenant, string environment, bool newStatus, LoggerTrackingIds trackingIds);

        /// <summary>
        /// Deletes a feature flag from Azure App Config
        /// </summary>
        /// <param name="name">Name of the feature</param>
        /// <param name="tenant">Tenant</param>
        /// <param name="environment">Environment</param>
        /// <param name="trackingIds" cref="LoggerTrackingIds">Tracking Ids</param>
        Task Delete(string name, string tenant, string environment, LoggerTrackingIds trackingIds);
    }
}
