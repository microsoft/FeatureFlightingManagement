using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    public interface IFlightOptimizer
    {
        void Optmize(AzureFeatureFlag flag, List<string> optimizationRules, LoggerTrackingIds trackingIds);
    }
}