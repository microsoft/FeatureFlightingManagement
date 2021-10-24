using System.Collections.Generic;
using Microsoft.FeatureFlighting.Core.Operators;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Core.Spec
{
    public interface IOperatorEvaluatorStrategy
    {
        BaseOperator Get(Operator op);
        Dictionary<string, List<string>> GetFilterOperatorMapping();
    }
}
