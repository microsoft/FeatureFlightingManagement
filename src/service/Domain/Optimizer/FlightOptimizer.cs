using System.Linq;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    public class FlightOptimizer : IFlightOptimizer
    {
        private readonly List<IFlightOptimizationRule> _optimizationRules;

        public static string OptimizedStageId = "-1";
        public static string OptimizedStageName = "OPTIMIZED";

        public FlightOptimizer(List<IFlightOptimizationRule> optimizationRules)
        {
            _optimizationRules = optimizationRules;
        }

        public void Optmize(AzureFeatureFlag flag, List<string> optimizationRules, LoggerTrackingIds trackingIds)
        {
            bool isFlagOptimized = false;
            if (_optimizationRules == null || !_optimizationRules.Any())
                return;
            
            foreach (string optimizationRuleName in optimizationRules)
            {
                IFlightOptimizationRule optimizationRule = _optimizationRules.FirstOrDefault(rule => rule.RuleName.ToLowerInvariant() == optimizationRuleName.ToLowerInvariant());
                isFlagOptimized |= optimizationRule.Optimize(flag, trackingIds);
            }
            flag.IsFlagOptimized = isFlagOptimized;
        }
    }
}
