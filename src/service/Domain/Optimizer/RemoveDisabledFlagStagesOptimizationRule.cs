using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    /// <summary>
    /// Removes all filters from a disabled feature flag
    /// </summary>
    internal class RemoveDisabledFlagStagesOptimizationRule: IFlightOptimizationRule
    {
        public string RuleName => nameof(RemoveDisabledFlagStagesOptimizationRule);

        private readonly ILogger _logger;

        public RemoveDisabledFlagStagesOptimizationRule(ILogger logger)
        {
            _logger = logger;
        }

        public bool Optimize(AzureFeatureFlag flag, LoggerTrackingIds trackingIds)
        {
            if (flag.Enabled)
                return false;
            
            EventContext context = new("FeatureFlagOptmized:AllFiltersRemovedForDisabledFlag", trackingIds.CorrelationId, trackingIds.TransactionId, "RemovedDisabledFlagStagesOptimizationRule:Optimize", "", flag.Id);
            context.AddProperty("Description", "Removed all filters since the flag is disabled");
            context.AddProperty("FeatureFlagId", flag.Id);
            context.AddProperty("FiltersRemovedCount", flag.Conditions.Client_Filters.Length);
            context.AddProperty("RemovedFilters", flag.Conditions.Client_Filters);

            flag.Conditions.Client_Filters = new AzureFilter[0];
            _logger.Log(context);
            return true;
        }
    }
}
