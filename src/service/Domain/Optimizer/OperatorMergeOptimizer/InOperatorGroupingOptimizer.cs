using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    public class InOperatorGroupingOptimizer : OperatorGroupingOptimizer, IFlightOptimizationRule
    {
        public override string RuleName => nameof(InOperatorGroupingOptimizer);

        protected override Operator DuplicateOperator => Operator.In;

        protected override Operator OptimizedOperator => Operator.In;

        protected override string EventName => "FeatureFlagOptmized:InOperatorsMerged";

        private readonly IConfiguration _configuration;

        public InOperatorGroupingOptimizer(ILogger logger, IConfiguration configuration) : base(logger)
        {
            _configuration = configuration;
        }
    }
}
