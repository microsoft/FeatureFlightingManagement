﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using static Microsoft.FeatureFlighting.Common.Constants;

namespace Microsoft.FeatureFlighting.Core.Operators
{
    /// <summary>
    /// Configured value must be present as one of comma-separated configured value
    /// </summary>
    public class InOperator : BaseOperator
    {
        public override Operator Operator => Operator.In;
        public override string[] SupportedFilters => new string[] { FilterKeys.Alias, FilterKeys.Country, FilterKeys.Region, FilterKeys.Role, FilterKeys.RoleGroup, FilterKeys.UserUpn, FilterKeys.Generic, FilterKeys.RulesEngine };

        protected override Task<EvaluationResult> Process(string configuredValue, string contextValue, string filterType, LoggerTrackingIds trackingIds)
        {
            if (string.IsNullOrWhiteSpace(configuredValue))
                return Task.FromResult(EvaluationResult.CreateFaultedResult(false, "Configured Value is empty", Operator, filterType));

            var configuredValues = configuredValue.Split(',').Select(p => p.Trim()).ToList();
            return Task.FromResult(new EvaluationResult(configuredValues.Any(value => value.ToLowerInvariant() == contextValue.ToLowerInvariant()), Operator, filterType));
        }
    }
}
