using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Core.Evaluators
{
    public class OperatorEvaluatorStrategy: IOperatorEvaluatorStrategy
    {
        private readonly IEnumerable<BaseOperatorEvaluator> _evaluators;
        private static Dictionary<string, List<string>> _filterOperatorMapping = null;
        
        public OperatorEvaluatorStrategy(IEnumerable<BaseOperatorEvaluator> evaluators)
        {
            _evaluators = evaluators;
        }

        public BaseOperatorEvaluator Get(Operator op)
        {
            return _evaluators.FirstOrDefault(evaluator => evaluator.Operator == op);
        }

        public Dictionary<string, List<string>> GetFilterOperatorMapping()
        {
            if (_filterOperatorMapping != null)
                return _filterOperatorMapping;

            var filterTypes = Enum.GetNames(typeof(Filters)).Distinct();
            var map = new Dictionary<string, List<string>>();

            foreach(var filterType in filterTypes)
            {
                var supportedOperators =
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
