using System.Linq;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    internal class DuplicateFilterValuesOptimizationRule : IFlightOptimizationRule
    {
        public string RuleName => nameof(DuplicateFilterValuesOptimizationRule);

        public bool Optimize(AzureFeatureFlag flag, LoggerTrackingIds trackingIds)
        {
            if (flag.Conditions?.Client_Filters == null || !flag.Conditions.Client_Filters.Any())
                return false;

            IEnumerable<AzureFilter> activeInFilters = GetActiveFilters(flag, Operator.In);
            IEnumerable<AzureFilter> activeNotInFilters = GetActiveFilters(flag, Operator.NotIn);

            bool isOptimized = false;
            isOptimized |= RemoveDuplicateValues(activeInFilters);
            isOptimized |= RemoveDuplicateValues(activeNotInFilters);

            return isOptimized;
        }

        private IEnumerable<AzureFilter> GetActiveFilters(AzureFeatureFlag flag, Operator @operator)
        {
            return flag.Conditions.Client_Filters
                .Where(azureFilter => azureFilter.IsActive() && azureFilter.Parameters.Operator == @operator.ToString())
                .ToList();
        }

        private bool RemoveDuplicateValues(IEnumerable<AzureFilter> filters)
        {
            if (filters == null || !filters.Any())
                return false;

            bool isOptimized = false;
            foreach(AzureFilter filter in filters)
            {
                if (string.IsNullOrWhiteSpace(filter.Parameters.Value))
                    continue;

                List<string> values = filter.Parameters.Value.Split(',').Distinct().ToList();
                string optimizedFilterValue = string.Join(',', values);
                isOptimized |= filter.Parameters.Value != optimizedFilterValue;
                filter.Parameters.Value = string.Join(',', values);
            }
            return isOptimized;
        }
    }
}
