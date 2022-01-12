using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    public class NotEqualOperatorGroupingOptimizer : OperatorGroupingOptimizer, IFlightOptimizationRule
    {
        public override string RuleName => nameof(NotEqualOperatorGroupingOptimizer);

        protected override Operator DuplicateOperator => Operator.NotEquals;

        protected override Operator OptimizedOperator => Operator.NotIn;

        protected override string EventName => "FeatureFlagOptmized:NotEqualOperatorsMerged";

        private readonly IConfiguration _configuration;

        public NotEqualOperatorGroupingOptimizer(ILogger logger, IConfiguration configuration): base(logger)
        {
            _configuration = configuration;
        }
    }
}
