using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    public class MergedNotEqualOperatorOptimizer : MergedOperatorOptimizer, IFlightOptimizationRule
    {
        public override string RuleName => nameof(MergedNotEqualOperatorOptimizer);

        protected override Operator DuplicateOperator => Operator.NotEquals;

        protected override Operator OptimizedOperator => Operator.NotIn;

        protected override string EventName => "FeatureFlagOptmized:NotEqualOperatorsMerged";

        private readonly IConfiguration _configuration;

        public MergedNotEqualOperatorOptimizer(ILogger logger, IConfiguration configuration): base(logger)
        {
            _configuration = configuration;
        }
    }
}
