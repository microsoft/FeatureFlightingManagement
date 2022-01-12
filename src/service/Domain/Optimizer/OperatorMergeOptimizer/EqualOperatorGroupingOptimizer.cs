using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    public class EqualOperatorGroupingOptimizer : OperatorGroupingOptimizer, IFlightOptimizationRule
    {
        public override string RuleName => nameof(InOperatorGroupingOptimizer);
        protected override Operator DuplicateOperator => Operator.Equals;

        protected override Operator OptimizedOperator => Operator.In;

        protected override string EventName => "FeatureFlagOptmized:EqualOperatorsMerged";

        private readonly IConfiguration _configuration;

        public EqualOperatorGroupingOptimizer(ILogger logger, IConfiguration configuration): base(logger)
        {
            _configuration = configuration;
        }
    }
}
