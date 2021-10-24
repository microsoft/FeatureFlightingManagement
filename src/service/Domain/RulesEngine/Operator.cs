using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Core.Operators;
using static Microsoft.FeatureFlighting.Common.Constants;

namespace Microsoft.FeatureFlighting.Core.RulesEngine
{
    public static class Operator
    {
        private static IOperatorEvaluatorStrategy _operatorEvaluatorStrategy;
        
        public static void Initialize(IOperatorEvaluatorStrategy operatorEvaluatorStrategy)
        {
            _operatorEvaluatorStrategy = operatorEvaluatorStrategy;
        }

        public static bool In(string value, string contextValue)
        {
            BaseOperator op = _operatorEvaluatorStrategy.Get(FeatureFilters.Operator.In);
            EvaluationResult result = op.Evaluate(value, contextValue, FilterKeys.RuleEngine, new Common.LoggerTrackingIds())
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            return result.Result;
        }

        public static bool NotIn(string value, string contextValue)
        {
            BaseOperator op = _operatorEvaluatorStrategy.Get(FeatureFilters.Operator.NotIn);
            EvaluationResult result = op.Evaluate(value, contextValue, FilterKeys.RuleEngine, new Common.LoggerTrackingIds())
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            return result.Result;
        }

        public static bool IsMember(string value, string contextValue)
        {
            BaseOperator op = _operatorEvaluatorStrategy.Get(FeatureFilters.Operator.MemberOfSecurityGroup);
            EvaluationResult result = op.Evaluate(value, contextValue, FilterKeys.RuleEngine, new Common.LoggerTrackingIds())
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            return result.Result;
        }

        public static bool IsNotMember(string value, string contextValue)
        {
            BaseOperator op = _operatorEvaluatorStrategy.Get(FeatureFilters.Operator.NotMemberOfSecurityGroup);
            EvaluationResult result = op.Evaluate(value, contextValue, FilterKeys.RuleEngine, new Common.LoggerTrackingIds())
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            return result.Result;
        }
    }
}
