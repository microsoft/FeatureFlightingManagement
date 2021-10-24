using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Core.Operators
{
    public class OperatorEvaluatorStrategy: IOperatorEvaluatorStrategy
    {
        private readonly IEnumerable<BaseOperator> _evaluators;
        private static Dictionary<string, List<string>> _filterOperatorMapping = null;
        
        public OperatorEvaluatorStrategy(IEnumerable<BaseOperator> evaluators)
        {
            _evaluators = evaluators;
        }

        public BaseOperator Get(Operator op)
        {
            return _evaluators.FirstOrDefault(evaluator => evaluator.Operator == op);
        }

        public Dictionary<string, List<string>> GetFilterOperatorMapping()
        {
            if (_filterOperatorMapping != null)
                return _filterOperatorMapping;

            var filterTypes = Enum.GetNames(typeof(Filters)).Distinct();
            Dictionary<string, List<string>> map = new();

            foreach(string filterType in filterTypes)
            {
                IEnumerable<Operator> supportedOperators =
                    _evaluators
                    .Where(evaluator => evaluator.SupportedFilters.Any(supportedFilter => supportedFilter.ToLowerInvariant() == Constants.Flighting.ALL.ToLowerInvariant() || supportedFilter.ToLowerInvariant() == filterType.ToLowerInvariant()))
                    .Select(evaluator => evaluator.Operator);

                if (supportedOperators != null && supportedOperators.Any())
                    map.Add(filterType, supportedOperators.Select(op => Enum.GetName(typeof(Operator), op)).ToList());
            }
            _filterOperatorMapping = map;
            return _filterOperatorMapping;
        }
    }
}
