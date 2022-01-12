using System;
using Microsoft.FeatureFlighting.Common.Model;

namespace Microsoft.FeatureFlighting.Core.Domain.ValueObjects
{
    public class Metric
    {
        public DateTime? LastEvaluatedOn { get; private set; }
        public string LastEvaluatedBy { get; private set; }
        public int EvaluationCount { get; private set; }
        public PerformanceMetric Performance { get; private set; }
        public DateTime StartedOn { get; private set; }
        public DateTime CompletedOn { get; private set; }

        public Metric(EvaluationMetricsDto evaluationMetrics)
        {
            LastEvaluatedOn = evaluationMetrics.LastEvaluatedOn;
            LastEvaluatedBy = evaluationMetrics.LastEvaluatedBy;
            EvaluationCount = evaluationMetrics.EvaluationCount;
            Performance = new(evaluationMetrics.P95Latency, evaluationMetrics.P90Latency, evaluationMetrics.AverageLatency);
            StartedOn = evaluationMetrics.From;
            CompletedOn = evaluationMetrics.To;
        }
    }
}
