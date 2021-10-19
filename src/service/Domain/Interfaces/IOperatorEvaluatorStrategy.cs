using System.Collections.Generic;
using Microsoft.FeatureFlighting.Domain.Evaluators;
using Microsoft.FeatureFlighting.Domain.FeatureFilters;

namespace Microsoft.FeatureFlighting.Domain.Interfaces
{
    public interface IOperatorEvaluatorStrategy
    {
        BaseOperatorEvaluator Get(Operator op);
        Dictionary<string, List<string>> GetFilterOperatorMapping();
    }
}
