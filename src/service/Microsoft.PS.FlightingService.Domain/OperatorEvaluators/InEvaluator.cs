using System.Linq;
using System.Threading.Tasks;
using Microsoft.PS.FlightingService.Common;
using Microsoft.PS.FlightingService.Domain.FeatureFilters;
using static Microsoft.PS.FlightingService.Common.Constants;

namespace Microsoft.PS.FlightingService.Domain.Evaluators
{
    public class InEvaluator : BaseOperatorEvaluator
    {
        public override Operator Operator => Operator.In;
        public override string[] SupportedFilters => new string[] { FilterKeys.Alias, FilterKeys.Country, FilterKeys.Region, FilterKeys.Role, FilterKeys.RoleGroup, FilterKeys.UserUpn, FilterKeys.Generic };

        protected override Task<EvaluationResult> Process(string configuredValue, string contextValue, string filterType, LoggerTrackingIds trackingIds)
        {
            if (string.IsNullOrWhiteSpace(configuredValue))
                return Task.FromResult(new EvaluationResult(false, "Configured Value is empty"));

            var configuredValues = configuredValue.Split(',').Select(p => p.Trim()).ToList();
            return Task.FromResult(new EvaluationResult(configuredValues.Any(value => value.ToLowerInvariant() == contextValue.ToLowerInvariant())));
        }
    }
}
