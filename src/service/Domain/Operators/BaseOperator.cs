using System.Linq;
using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using static Microsoft.FeatureFlighting.Common.Constants;

namespace Microsoft.FeatureFlighting.Core.Operators
{
    public abstract class BaseOperator
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
