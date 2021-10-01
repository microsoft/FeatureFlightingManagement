using System;
using System.Threading.Tasks;
using Microsoft.PS.FlightingService.Common;
using Microsoft.PS.FlightingService.Domain.FeatureFilters;
using static Microsoft.PS.FlightingService.Common.Constants;

namespace Microsoft.PS.FlightingService.Domain.Evaluators
{
    public class GreaterThanEvaluator : BaseOperatorEvaluator
    {
        public override Operator Operator => Operator.GreaterThan;
        public override string[] SupportedFilters => new string[] { Flighting.ALL };

        protected override Task<EvaluationResult> Process(string configuredValue, string contextValue, string filterType, LoggerTrackingIds trackingIds)
        {
            if (filterType.ToLowerInvariant() == FilterKeys.Date.ToLowerInvariant())
                return Task.FromResult(EvaluateDate(configuredValue, contextValue));

            if (int.TryParse(configuredValue, out int _) && int.TryParse(contextValue, out int _))
                return Task.FromResult(EvaluateNumber(configuredValue, contextValue));

            return Task.FromResult(new EvaluationResult(string.Compare(contextValue, configuredValue) > 0));
        }

        private EvaluationResult EvaluateDate(string configuredValue, string contextValue)
        {
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTime configuredDate = date.AddMilliseconds(Convert.ToDouble(configuredValue)).ToLocalTime();
            DateTime contextDate = date.AddMilliseconds(Convert.ToDouble(contextValue)).ToLocalTime();
           
            
                return new EvaluationResult(contextDate > configuredDate);
            
        }

        private EvaluationResult EvaluateNumber(string configuredValue, string contextValue)
        {
            if (int.TryParse(configuredValue, out int configuredNumber) && int.TryParse(contextValue, out int contextNumber))
            {
                return new EvaluationResult(contextNumber > configuredNumber);
            }
            return new EvaluationResult(false, "Either the context or the configured value is not an integer");
        }
    }
}
