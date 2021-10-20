using System.Linq;
using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using static Microsoft.FeatureFlighting.Common.Constants;

namespace Microsoft.FeatureFlighting.Core.Evaluators
{
    public class NotInEvaluator : BaseOperatorEvaluator
    {
        public override Operator Operator => Operator.NotIn;
        public override string[] SupportedFilters => new string[] { FilterKeys.Alias, FilterKeys.Country, FilterKeys.Region, FilterKeys.Role, FilterKeys.RoleGroup, FilterKeys.UserUpn, FilterKeys.Generic };

        protected override Task<EvaluationResult> Process(string configuredValue, string contextValue, string filterType, LoggerTrackingIds trackingIds)
        {
            if (string.IsNullOrWhiteSpace(configuredValue))
                return Task.FromResult(new EvaluationResult(true, "Configured Value is empty"));

            var configuredValues = configuredValue.Split(',').Select(p => p.Trim()).ToList();
            return Task.FromResult(new EvaluationResult(!configuredValues.Any(value => value.ToLowerInvariant() == contextValue.ToLowerInvariant())));
        }
    }
}
