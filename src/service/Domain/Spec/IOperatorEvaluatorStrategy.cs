using System.Collections.Generic;
using Microsoft.FeatureFlighting.Core.Evaluators;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Core.Spec
{
    public interface IOperatorEvaluatorStrategy
    {
        BaseOperatorEvaluator Get(Operator op);
        Dictionary<string, List<string>> GetFilterOperatorMapping();
    }
}
