using System.Linq;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{   
    // <inheritdoc/>
    internal class FlightOptimizer : IFlightOptimizer
    {
        private readonly List<IFlightOptimizationRule> _optimizationRules;

        public static string OptimizedStageId = "-1";
        public static string OptimizedStageName = "OPTIMIZED";
        private readonly ILogger _logger;

        public FlightOptimizer(IEnumerable<IFlightOptimizationRule> optimizationRules, ILogger logger)
        {
            _optimizationRules = optimizationRules?.ToList();
            _logger = logger;
        }

        // <inheritdoc/>
        public void Optmize(AzureFeatureFlag flag, List<string> optimizationRules, LoggerTrackingIds trackingIds)
        {
            bool isFlagOptimized = false;
            if (_optimizationRules == null || !_optimizationRules.Any())
                return;

            if (optimizationRules[0].ToLowerInvariant() == "*".ToLowerInvariant())
                optimizationRules = GetAllOptimizationRules().ToList();


            foreach (string optimizationRuleName in optimizationRules)
            {
                IFlightOptimizationRule optimizationRule = _optimizationRules.FirstOrDefault(rule => rule.RuleName.ToLowerInvariant() == optimizationRuleName.ToLowerInvariant());
                if (optimizationRule == null)
                {
                    _logger.Log($"Invalid optimization rule with name {optimizationRuleName} cannot be evaluated");
                }
                isFlagOptimized |= optimizationRule.Optimize(flag, trackingIds);
            }
            flag.IsFlagOptimized = isFlagOptimized;
        }

        private IEnumerable<string> GetAllOptimizationRules()
        {
            return _optimizationRules.Select(rule => rule.RuleName);
        }
    }
}
