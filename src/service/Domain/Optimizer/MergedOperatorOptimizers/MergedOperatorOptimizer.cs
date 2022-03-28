using System.Linq;
using System.Text;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    public abstract class MergedOperatorOptimizer: IFlightOptimizationRule
    {   
        public abstract string RuleName { get; }
        protected abstract Operator DuplicateOperator { get; }
        protected abstract Operator OptimizedOperator { get; }
        protected abstract string EventName { get; }

        protected readonly ILogger _logger;

        protected MergedOperatorOptimizer(ILogger logger)
        {
            _logger = logger;
        }

        public bool Optimize(AzureFeatureFlag flag, LoggerTrackingIds trackingIds)
       {
            IEnumerable<AzureFilter> activeFilters = GetActiveFilters(flag);
            IEnumerable<IGrouping<string, AzureFilterGroup>> groupedDuplicateFilters = GroupFiltersWithDuplicateContextKey(activeFilters);
            IEnumerable<AzureFilter> optimizedFilters = OptimizeDuplicateGroups(groupedDuplicateFilters);
            RemoveDuplicateFilters(flag, groupedDuplicateFilters);
            AddOptimizedFilters(flag, optimizedFilters);
            LogOptimizationResults(optimizedFilters, groupedDuplicateFilters, flag, trackingIds);
            return optimizedFilters != null && optimizedFilters.Any();
        }

        private void LogOptimizationResults(IEnumerable<AzureFilter> optimizedFilters, IEnumerable<IGrouping<string, AzureFilterGroup>> groupedDuplicateFilters, AzureFeatureFlag flag, LoggerTrackingIds trackingIds)
        {
            if (groupedDuplicateFilters == null || optimizedFilters == null)
                return;

            IEnumerable<AzureFilterGroup> removedFilters = groupedDuplicateFilters.SelectMany(group => group).ToList();
            EventContext context = new(EventName, trackingIds.CorrelationId, trackingIds.TransactionId, "AzureFilterGroupingOptimizer:Optimize", "", flag.Id);
            context.AddProperty("FeatureFlagId", flag.Id);
            context.AddProperty("FiltersRemovedCount", removedFilters.Count());
            context.AddProperty("RemovedFilters", removedFilters);
            context.AddProperty("OptimizedFilters", flag.Conditions.Client_Filters);
            context.AddProperty("Description", new StringBuilder()
                .Append("Merged all filters with ")
                .Append(DuplicateOperator.ToString())
                .Append(" into a single filter with ")
                .Append(OptimizedOperator.ToString())
                .ToString());
            _logger.Log(context);
        }

        protected IEnumerable<AzureFilter> GetActiveFilters(AzureFeatureFlag flag)
        {
            if (flag.Conditions == null || flag.Conditions.Client_Filters == null || !flag.Conditions.Client_Filters.Any())
                return null;

            List<AzureFilter> activeFilters = flag.Conditions.Client_Filters.Where(filter => filter.IsActive()).ToList();
            if (activeFilters == null || !activeFilters.Any())
                return null;

            List<AzureFilter> activeFiltersWithDuplicateOperator = activeFilters.Where(filter => filter.Parameters.Operator == DuplicateOperator.ToString()).ToList();
            return activeFiltersWithDuplicateOperator;
        }

        protected IEnumerable<IGrouping<string, AzureFilterGroup>> GroupFiltersWithDuplicateContextKey(IEnumerable<AzureFilter> activeFilters)
        {
            if (activeFilters == null || !activeFilters.Any())
                return null;

            IEnumerable<IGrouping<string, AzureFilterGroup>> filterGroupByContextKey = activeFilters.GroupBy(
                filter => filter.Parameters.FlightContextKey,
                filter => new AzureFilterGroup
                {
                    ContextKey = filter.Parameters.FlightContextKey,
                    Filter = filter
                });

            IEnumerable<IGrouping<string, AzureFilterGroup>> duplicateFilters = filterGroupByContextKey
                .Where(group => group.Count() >= 2)
                .ToList();
            return duplicateFilters;
        }

        protected IEnumerable<AzureFilter> OptimizeDuplicateGroups(IEnumerable<IGrouping<string, AzureFilterGroup>> duplicateFilterGroups)
        {
            if (duplicateFilterGroups == null || !duplicateFilterGroups.Any())
                return null;

            List<AzureFilter> optimizedInFilters = new();
            foreach (var duplicateGroup in duplicateFilterGroups)
            {   
                AzureFilter optimizedInFilter = OptimizeDuplicates(duplicateGroup);
                optimizedInFilters.Add(optimizedInFilter);
            }
            return optimizedInFilters;
        }

        private AzureFilter OptimizeDuplicates(IGrouping<string, AzureFilterGroup> duplicateFilters)
        {
            string joinedFilterValue = JoinDuplicateValues(duplicateFilters);
            AzureFilter optimizedInFilter = new()
            {
                Name = duplicateFilters.First().Filter.Name,
                Parameters = new()
                {
                    FlightContextKey = duplicateFilters.Key,
                    IsActive = bool.TrueString,
                    Operator = OptimizedOperator.ToString(),
                    StageId = FlightOptimizer.OptimizedStageId,
                    StageName = FlightOptimizer.OptimizedStageName,
                    Value = joinedFilterValue
                }
            };
            return optimizedInFilter;
        }

        protected virtual string JoinDuplicateValues(IGrouping<string, AzureFilterGroup> duplicateFilters)
        {
            return string.Join(',', duplicateFilters.Select(group => group.Filter.Parameters.Value).Distinct());
        }

        protected void RemoveDuplicateFilters(AzureFeatureFlag flag, IEnumerable<IGrouping<string, AzureFilterGroup>> groupedDuplicateFilters)
        {
            if (groupedDuplicateFilters == null || !groupedDuplicateFilters.Any())
                return;

            List<string> groupedEqualOperatorFiltersContextKey = groupedDuplicateFilters.Select(group => group.Key).ToList();
            foreach (AzureFilter filter in flag.Conditions.Client_Filters.Where(azureFilter => azureFilter.Parameters.Operator == DuplicateOperator.ToString()))
            {
                if (groupedEqualOperatorFiltersContextKey.Contains(filter.Parameters.FlightContextKey))
                {
                    filter.Parameters.IsActive = bool.FalseString;
                }
            }
            flag.Conditions.Client_Filters = flag.Conditions.Client_Filters.Where(filter => filter.IsActive()).ToArray();
        }

        protected void AddOptimizedFilters(AzureFeatureFlag flag, IEnumerable<AzureFilter> optimizedFilters)
        {
            if (optimizedFilters == null || !optimizedFilters.Any())
                return;
            List<AzureFilter> updatedFilters = flag.Conditions.Client_Filters.ToList() ?? new();
            updatedFilters.AddRange(optimizedFilters);
            flag.Conditions.Client_Filters = updatedFilters.ToArray();
        }
    }
}
