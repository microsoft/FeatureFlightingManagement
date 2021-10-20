using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Model;

namespace Microsoft.FeatureFlighting.Core.Spec
{
    /// <summary>
    /// Manages feature flags
    /// </summary>
    public interface IFeatureFlagManager
    {
        /// <summary>
        /// Creates a feature flag
        /// </summary>
        /// <param name="appName">Tenant</param>
        /// <param name="envName">Environment</param>
        /// <param name="featureFlag" cref="FeatureFlag">Feature flag to be created</param>
        /// <param name="trackingIds">Tracking ID</param>
        Task CreateFeatureFlag(string appName, string envName, FeatureFlag featureFlag, LoggerTrackingIds trackingIds);


        /// <summary>
        /// Updates an existing feature flag
        /// </summary>
        /// <param name="appName">Tenant</param>
        /// <param name="envName">Environment</param>
        /// <param name="featureFlag" cref="FeatureFlag">Updated Feature flag</param>
        /// <param name="trackingIds">Tracking ID</param>
        Task UpdateFeatureFlag(string appName, string envName, FeatureFlag featureFlag, LoggerTrackingIds trackingIds);

        /// <summary>
        /// Updates the status of a feature flag
        /// </summary>
        /// <param name="appName">Tenant</param>
        /// <param name="envName">Environment</param>
        /// <param name="featureFlag" cref="FeatureFlag">Updated Feature flag</param>
        /// <param name="status">New status of the feature flag</param>
        /// <param name="trackingIds">Tracking IDs</param>
        Task UpdateFeatureFlagStatus(string appName, string envName, FeatureFlag featureFlag, bool status, LoggerTrackingIds trackingIds);

        /// <summary>
        /// Gets a feature flag by feature flag name
        /// </summary>
        /// <param name="appName">Tenant</param>
        /// <param name="envName">Environment</param>
        /// <param name="name">Name of the feature flag</param>
        /// <param name="trackingIds">Tracking ID</param>
        /// <returns cref="FeatureFlag">Feature flag</returns>
        Task<FeatureFlag> GetFeatureFlag(string appName, string envName, string name, LoggerTrackingIds trackingIds);

        /// <summary>
        /// Gets all feature flags for a tenant
        /// </summary>
        /// <param name="appName">Tenant</param>
        /// <param name="envName">Environment</param>
        /// <param name="trackingIds">Tracking ID</param>
        /// <returns cref="IList{FeatureFlagDto}">List of feature flags</returns>
        Task<IList<FeatureFlagDto>> GetFeatureFlags(string appName, string envName, LoggerTrackingIds trackingIds);

        /// <summary>
        /// Gets the names of all the feature flags for a tenant
        /// </summary>
        /// <param name="appName">Tenant</param>
        /// <param name="envName">Environment</param>
        /// <param name="trackingIds">Tracking ID</param>
        /// <param name="burstCache">Flag to clear the cache</param>
        /// <returns cref="IList{string}">List of feature flag names</returns>
        Task<IList<string>> GetFeatures(string appName, string envName, LoggerTrackingIds trackingIds, bool burstCache = false);

        /// <summary>
        /// Deletes a feature flag
        /// </summary>
        /// <param name="appName">Tenant</param>
        /// <param name="envName">Environment</param>
        /// <param name="featureName">Name of the feature </param>
        /// <param name="trackingIds">Tracking ID</param>
        Task DeleteFeatureFlag(string appName, string envName, string featureName, LoggerTrackingIds trackingIds);

        /// <summary>
        /// Activates the stage of a feature flag
        /// </summary>
        /// <param name="appName">Tenant</param>
        /// <param name="envName">Environment</param>
        /// <param name="featureFlag" cref="FeatureFlag">Feature flag whose stage has to be updated</param>
        /// <param name="stage">Stage to be activated</param>
        /// <param name="trackingIds">Tracking ID</param>
        Task ActivateStage(string appName, string envName, FeatureFlag featureFlag, string stage, LoggerTrackingIds trackingIds);
    }
}
