using System.Threading.Tasks;
using Microsoft.PS.FlightingService.Common;
using Microsoft.PS.FlightingService.Domain.FeatureFilters;
using static Microsoft.PS.FlightingService.Common.Constants;

namespace Microsoft.PS.FlightingService.Domain.Evaluators
{
    public class EqualEvaluator : BaseOperatorEvaluator
    {
        public override Operator Operator => Operator.Equals;
        public override string[] SupportedFilters => new string[] { FilterKeys.Alias,FilterKeys.Country,FilterKeys.Region,FilterKeys.Role,FilterKeys.RoleGroup,FilterKeys.UserUpn,FilterKeys.Generic };

        protected override Task<EvaluationResult> Process(string configuredValue, string contextValue, string filterType, LoggerTrackingIds trackingIds)
        {
            var isEqual = contextValue.ToLowerInvariant().Equals(configuredValue.ToLowerInvariant());
            return Task.FromResult(new EvaluationResult(isEqual));
        }
    }
}
