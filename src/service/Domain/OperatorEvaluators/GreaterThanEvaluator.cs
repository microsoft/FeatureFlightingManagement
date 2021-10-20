using System;
using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using static Microsoft.FeatureFlighting.Common.Constants;

namespace Microsoft.FeatureFlighting.Core.Evaluators
{
    public class GreaterThanEvaluator : BaseOperatorEvaluator
    {
        public override Operator Operator => Operator.GreaterThan;
        public override string[] SupportedFilters => new string[] { Flighting.ALL };

        protected override Task<EvaluationResult> Process(string configuredValue, string contextValue, string filterType, LoggerTrackingIds trackingIds)
        {
            if (filterType.ToLowerInvariant() == FilterKeys.Date.ToLowerInvariant())
                return Task.FromResult(EvaluateDate(configuredValue, contextValue));

            if (int.TryParse(configuredValue, out int configuredNumber) && int.TryParse(contextValue, out int contextNumber))
                return Task.FromResult(EvaluateNumber(configuredNumber, contextNumber));

            return Task.FromResult(new EvaluationResult(string.Compare(contextValue, configuredValue) > 0));
        }

        private EvaluationResult EvaluateDate(string configuredValue, string contextValue)
        {
            DateTime date = new(1970, 1, 1, 0, 0, 0, 0);
            DateTime configuredDate = date.AddMilliseconds(Convert.ToDouble(configuredValue)).ToLocalTime();
            DateTime contextDate = date.AddMilliseconds(Convert.ToDouble(contextValue)).ToLocalTime();
            return new EvaluationResult(contextDate > configuredDate);

        }

        private EvaluationResult EvaluateNumber(double configuredValue, double contextValue)
        {
            return new EvaluationResult(contextValue > configuredValue);
        }
    }
}
