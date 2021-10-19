using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Domain.Configuration;

namespace Microsoft.FeatureFlighting.Domain.Interfaces
{
    public interface IFeatureFlagManager
    {
        Task CreateFeatureFlag(string appName, string envName, FeatureFlag featureFlag, LoggerTrackingIds trackingIds);
        Task UpdateFeatureFlag(string appName, string envName, FeatureFlag featureFlag, LoggerTrackingIds trackingIds);
        Task UpdateFeatureFlagStatus(string appName, string envName, FeatureFlag featureFlag, bool status, LoggerTrackingIds trackingIds);
        Task<FeatureFlag> GetFeatureFlag(string appName, string envName, string name, LoggerTrackingIds trackingIds);
        Task<IList<FeatureFlagDto>> GetFeatureFlags(string appName, string envName, LoggerTrackingIds trackingIds);
        Task<IList<string>> GetFeatures(string appName, string envName, LoggerTrackingIds trackingIds, bool burstCache = false);
        Task DeleteFeatureFlag(string appName, string envName, string featureName, LoggerTrackingIds trackingIds);
        Task ActivateStage(string appName, string envName, FeatureFlag featureFlag, string stage, LoggerTrackingIds trackingIds);
    }
}
