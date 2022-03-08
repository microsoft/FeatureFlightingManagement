using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    internal class NotMemberOfSecurityGroupOptimizer : MergedOperatorOptimizer
    {
        public override string RuleName => nameof(NotMemberOfSecurityGroupOptimizer);

        protected override Operator DuplicateOperator => Operator.NotMemberOfSecurityGroup;

        protected override Operator OptimizedOperator => Operator.NotMemberOfSecurityGroup;

        protected override string EventName => "FeatureFlagOptmized:NotMemberOfSecurityGroupOperatorMerged";

        public NotMemberOfSecurityGroupOptimizer(ILogger logger) : base(logger) { }
    }
}
