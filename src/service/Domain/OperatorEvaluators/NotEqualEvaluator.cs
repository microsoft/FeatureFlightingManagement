﻿using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using static Microsoft.FeatureFlighting.Common.Constants;

namespace Microsoft.FeatureFlighting.Core.Evaluators
{
    public class NotEqualEvaluator : BaseOperatorEvaluator
    {
        public override Operator Operator => Operator.NotEquals;
        public override string[] SupportedFilters => new string[] { FilterKeys.Alias, FilterKeys.Country, FilterKeys.Region, FilterKeys.Role, FilterKeys.RoleGroup, FilterKeys.UserUpn, FilterKeys.Generic };

        protected override Task<EvaluationResult> Process(string configuredValue, string contextValue, string filterType, LoggerTrackingIds trackingIds)
        {
            var isEqual = contextValue.ToLowerInvariant().Equals(configuredValue.ToLowerInvariant());
            return Task.FromResult(new EvaluationResult(!isEqual));
        }
    }
}
