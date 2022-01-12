using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    public class NotInOperatorGroupingOptimizer : OperatorGroupingOptimizer, IFlightOptimizationRule
    {
        public override string RuleName => nameof(NotInOperatorGroupingOptimizer);

        protected override Operator DuplicateOperator => Operator.NotIn;

        protected override Operator OptimizedOperator => Operator.NotIn;

        protected override string EventName => "FeatureFlagOptmized:NotInOperatorsMerged";

        private readonly IConfiguration _configuration;

        public NotInOperatorGroupingOptimizer(ILogger logger, IConfiguration configuration) : base(logger)
        {
            _configuration = configuration;
        }
    }
}
