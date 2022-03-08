using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    public interface IFlightOptimizationRule
    {   
        string RuleName { get; }
        bool Optimize(AzureFeatureFlag flag, LoggerTrackingIds trackingIds);
    }
}
