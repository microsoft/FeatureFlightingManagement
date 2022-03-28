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
            if (_optimizationRules == null || !_optimizationRules.Any())
                return;

            if (optimizationRules[0].ToLowerInvariant() == "*".ToLowerInvariant())
                optimizationRules = GetAllOptimizationRules().ToList();

            flag.Optimizations = new List<string>();
            foreach (string optimizationRuleName in optimizationRules)
            {
                IFlightOptimizationRule optimizationRule = _optimizationRules.FirstOrDefault(rule => rule.RuleName.ToLowerInvariant() == optimizationRuleName.ToLowerInvariant());
                if (optimizationRule == null)
                {
                    _logger.Log($"Invalid optimization rule with name {optimizationRuleName} cannot be evaluated");
                    continue;
                }
                bool isOptimizationRuleApplied = optimizationRule.Optimize(flag, trackingIds);
                if (isOptimizationRuleApplied)
                    flag.Optimizations.Add(optimizationRule.RuleName);
            }
            flag.IsFlagOptimized = flag.Optimizations != null && flag.Optimizations.Any();
        }

        private IEnumerable<string> GetAllOptimizationRules()
        {
            return _optimizationRules.Select(rule => rule.RuleName);
        }
    }
}
