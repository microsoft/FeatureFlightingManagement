using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    internal class MemberOfSecurityGroupOptimizer : MergedOperatorOptimizer
    {
        public override string RuleName => nameof(MemberOfSecurityGroupOptimizer);

        protected override Operator DuplicateOperator => Operator.MemberOfSecurityGroup;

        protected override Operator OptimizedOperator => Operator.MemberOfSecurityGroup;

        protected override string EventName => "FeatureFlagOptmized:MemberOfSecurityGroupOperatorMerged";

        public MemberOfSecurityGroupOptimizer(ILogger logger) : base(logger) { }
    }
}
