using System.Linq;
using System.Threading.Tasks;
using Microsoft.PS.FlightingService.Common;
using Microsoft.PS.FlightingService.Domain.FeatureFilters;
using static Microsoft.PS.FlightingService.Common.Constants;

namespace Microsoft.PS.FlightingService.Domain.Evaluators
{
    public abstract class BaseOperatorEvaluator
    {
        public abstract Operator Operator {get;}
        public abstract string[] SupportedFilters { get; }
        
        public virtual async Task<EvaluationResult> Evaluate(string configuredValue, string contextValue, string filterType, LoggerTrackingIds trackingIds)
        {
            if (SupportedFilters.Any(filter =>
                filter.ToLowerInvariant() == Flighting.ALL ||
                filter.ToLowerInvariant() == filterType.ToLowerInvariant()))
            {
                return await Process(configuredValue, contextValue, filterType, trackingIds);
            }
            return new EvaluationResult(false, $"Operator of type {nameof(Operator)} is not supported for filter {filterType}");
        }

        protected abstract Task<EvaluationResult> Process(string configuredValue, string contextValue, string filterType, LoggerTrackingIds trackingIds);
    }
}
