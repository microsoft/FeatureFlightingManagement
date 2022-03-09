using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    public class MergedInOperatorOptimizer : MergedOperatorOptimizer, IFlightOptimizationRule
    {
        public override string RuleName => nameof(MergedInOperatorOptimizer);

        protected override Operator DuplicateOperator => Operator.In;

        protected override Operator OptimizedOperator => Operator.In;

        protected override string EventName => "FeatureFlagOptmized:InOperatorsMerged";

        public MergedInOperatorOptimizer(ILogger logger) : base(logger) { }
    }
}
