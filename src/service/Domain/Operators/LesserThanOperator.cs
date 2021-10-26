using System;
using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using static Microsoft.FeatureFlighting.Common.Constants;

namespace Microsoft.FeatureFlighting.Core.Operators
{
    /// <summary>
    /// Lesser than operator. Context value must be smaller than configured value.
    /// </summary>
    public class LesserThanOperator : BaseOperator
    {
        public override Operator Operator => Operator.LessThan;
        public override string[] SupportedFilters => new string[] { Flighting.ALL };

        protected override Task<EvaluationResult> Process(string configuredValue, string contextValue, string filterType, LoggerTrackingIds trackingIds)
        {
            if (filterType.ToLowerInvariant() == FilterKeys.Date.ToLowerInvariant())
                return Task.FromResult(EvaluateDate(configuredValue, contextValue));

            if (int.TryParse(configuredValue, out int _) && int.TryParse(contextValue, out int _))
                return Task.FromResult(EvaluateNumber(configuredValue, contextValue, filterType));

            return Task.FromResult(new EvaluationResult(string.Compare(contextValue, configuredValue) < 0, Operator, filterType));
        }

        private EvaluationResult EvaluateDate(string configuredValue, string contextValue)
        {
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTime configuredDate = date.AddMilliseconds(Convert.ToDouble(configuredValue)).ToLocalTime();
            DateTime contextDate = date.AddMilliseconds(Convert.ToDouble(contextValue)).ToLocalTime();

            return new EvaluationResult(contextDate < configuredDate, Operator, FilterKeys.Date);
            
        }

        private EvaluationResult EvaluateNumber(string configuredValue, string contextValue, string filterType)
        {
            if (int.TryParse(configuredValue, out int configuredNumber) && int.TryParse(contextValue, out int contextNumber))
            {
                return new EvaluationResult(contextNumber < configuredNumber, Operator, filterType);
            }
            return new EvaluationResult(false, "Either the context or the configured value is not an integer", Operator, filterType);
        }
    }
}
