using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    internal class MergedEqualOperatorOptimizer : MergedOperatorOptimizer
    {
        public override string RuleName => nameof(MergedEqualOperatorOptimizer);
        protected override Operator DuplicateOperator => Operator.Equals;

        protected override Operator OptimizedOperator => Operator.In;

        protected override string EventName => "FeatureFlagOptmized:EqualOperatorsMerged";
        

        public MergedEqualOperatorOptimizer(ILogger logger): base(logger) { }
    }
}
