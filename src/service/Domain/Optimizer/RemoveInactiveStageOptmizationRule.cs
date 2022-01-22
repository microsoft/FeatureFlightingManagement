using System.Linq;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    /// <summary>
    /// Removes filters from stages which are inactive
    /// </summary>
    internal class RemoveInactiveStageOptmizationRule : IFlightOptimizationRule
    {
        public string RuleName => nameof(RemoveInactiveStageOptmizationRule);

        private readonly ILogger _logger;

        public RemoveInactiveStageOptmizationRule(ILogger logger)
        {
            _logger = logger;
        }

        public bool Optimize(AzureFeatureFlag flag, LoggerTrackingIds trackingIds)
        {   
            if (flag.Conditions == null || flag.Conditions.Client_Filters == null || !flag.Conditions.Client_Filters.Any())
                return false;

            List<AzureFilter> inactiveFilters = flag.Conditions.Client_Filters.Where(filter => !filter.IsActive()).ToList();
            if (inactiveFilters == null || !inactiveFilters.Any())
                return false;

            List<AzureFilter> activeFilters = flag.Conditions.Client_Filters.Where(filter => filter.IsActive()).ToList();
            flag.Conditions.Client_Filters = activeFilters.ToArray();

            EventContext context = new("FeatureFlagOptmized:InactiveFiltersRemoved", trackingIds.CorrelationId, trackingIds.TransactionId, "RemoveInactiveStageOptmizationRule:Optimize", "", flag.Id);
            context.AddProperty("Description", "Removed all inactive filters");
            context.AddProperty("FeatureFlagId", flag.Id);
            context.AddProperty("FiltersRemovedCount", inactiveFilters.Count);
            context.AddProperty("RemovedFilters", inactiveFilters);
            context.AddProperty("OptimizedFilters", flag.Conditions.Client_Filters);
            _logger.Log(context);
            return true;
        }
    }
}
