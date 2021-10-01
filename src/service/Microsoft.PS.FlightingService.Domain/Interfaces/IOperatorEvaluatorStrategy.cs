using System.Collections.Generic;
using Microsoft.PS.FlightingService.Domain.Evaluators;
using Microsoft.PS.FlightingService.Domain.FeatureFilters;

namespace Microsoft.PS.FlightingService.Domain.Interfaces
{
    public interface IOperatorEvaluatorStrategy
    {
        BaseOperatorEvaluator Get(Operator op);
        Dictionary<string, List<string>> GetFilterOperatorMapping();
    }
}
