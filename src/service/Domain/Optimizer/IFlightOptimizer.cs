using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    /// <summary>
    /// Optimizes a feature flag
    /// </summary>
    internal interface IFlightOptimizer
    {
        /// <summary>
        /// Optimizes a <see cref="AzureFeatureFlag"/> to its most basic form
        /// </summary>
        /// <param name="flag" cref="AzureFeatureFlag">Azure feature flag</param>
        /// <param name="optimizationRules" cref="List{string}">Names of all optimization rules</param>
        /// <param name="trackingIds" cref="LoggerTrackingIds">Tracking IDs</param>
        void Optmize(AzureFeatureFlag flag, List<string> optimizationRules, LoggerTrackingIds trackingIds);
    }
}