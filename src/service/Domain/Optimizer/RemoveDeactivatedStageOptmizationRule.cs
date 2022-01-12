using System.Linq;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common;
using Microsoft.Extensions.Configuration;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    public class RemoveInactiveStageOptmizationRule : IFlightOptimizationRule
    {
        public int Priority => 1;

        public bool Enabled => _configuration.GetValue<bool>("FlightOptimizationRules:RemoveInactiveFilters");

        public string RuleName => nameof(RemoveInactiveStageOptmizationRule);

        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public RemoveInactiveStageOptmizationRule(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public bool Optimize(AzureFeatureFlag flag, LoggerTrackingIds trackingIds)
        {   
            if (flag.Conditions == null || flag.Conditions.Client_Filters == null || !flag.Conditions.Client_Filters.Any())
                return false;

            List<AzureFilter> inactiveFilters = flag.Conditions.Client_Filters.Where(filter => filter.Parameters.IsActive.ToLowerInvariant() == bool.FalseString.ToLowerInvariant()).ToList();
            if (inactiveFilters == null || !inactiveFilters.Any())
                return false;

            List<AzureFilter> activeFilters = flag.Conditions.Client_Filters.Where(filter => filter.Parameters.IsActive.ToLowerInvariant() == bool.TrueString.ToLowerInvariant()).ToList();
            flag.Conditions.Client_Filters = activeFilters.ToArray();

            EventContext context = new("FeatureFlagOptmized:InactiveFiltersRemoved", trackingIds.CorrelationId, trackingIds.TransactionId, "RemoveInactiveStageOptmizationRule:Optimize", "", flag.Id);
            context.AddProperty("FeatureFlagId", flag.Id);
            context.AddProperty("FiltersRemovedCount", inactiveFilters.Count);
            context.AddProperty("RemovedFilters", inactiveFilters);
            context.AddProperty("OptimizedFilters", flag.Conditions.Client_Filters);
            _logger.Log(context);
            return true;
        }
    }
}
