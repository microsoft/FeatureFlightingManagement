using CQRS.Mediatr.Lite;
using Microsoft.FeatureFlighting.Common.Model;

namespace Microsoft.FeatureFlighting.Core.Commands
{
    /// <summary>
    /// Result of <see cref="UpdateMetricsCommand" />
    /// </summary>
    public class MetricsCommandResult : CommandResult
    {
        public EvaluationMetricsDto Metrics { get; set; }
        public MetricsCommandResult(EvaluationMetricsDto metrics) : base(true, "Metrics created")
        {
            Metrics = metrics;
        }

        public MetricsCommandResult() : base(false, "Metrics generation is disabled") { }
    }
}
