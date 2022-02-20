using System;
using Microsoft.FeatureFlighting.Common.Model;

namespace Microsoft.FeatureFlighting.Core.Domain.ValueObjects
{
    public class Metric
    {
        public DateTime? LastEvaluatedOn { get; private set; }
        public string LastEvaluatedBy { get; private set; }
        public int EvaluationCount { get; private set; }
        public int TotalEvaluations { get; private set; }
        public PerformanceMetric Performance { get; private set; }
        public DateTime StartedOn { get; private set; }
        public DateTime CompletedOn { get; private set; }

        public Metric(EvaluationMetricsDto evaluationMetrics)
        {
            if (evaluationMetrics == null)
                return;

            LastEvaluatedOn = evaluationMetrics.LastEvaluatedOn;
            LastEvaluatedBy = evaluationMetrics.LastEvaluatedBy;
            EvaluationCount = evaluationMetrics.EvaluationCount;
            TotalEvaluations = evaluationMetrics.TotalEvaluations;
            Performance = new(evaluationMetrics.P95Latency, evaluationMetrics.P90Latency, evaluationMetrics.AverageLatency);
            StartedOn = evaluationMetrics.From;
            CompletedOn = evaluationMetrics.To;
        }

        public void Update(EvaluationMetricsDto metrics)
        {   
            EvaluationCount = metrics.EvaluationCount;
            if (LastEvaluatedOn == null)
            {
                LastEvaluatedOn = metrics.LastEvaluatedOn != DateTime.MinValue ? metrics.LastEvaluatedOn : null;
                LastEvaluatedBy = metrics.LastEvaluatedBy;
            }
            else if (metrics.LastEvaluatedOn != null && metrics.LastEvaluatedOn > LastEvaluatedOn)
            {
                LastEvaluatedOn = metrics.LastEvaluatedOn;
                LastEvaluatedBy = metrics.LastEvaluatedBy;
            }

            if (TotalEvaluations == 0 || CompletedOn == DateTime.MinValue || metrics.From >= CompletedOn.AddHours(-1))
            {
                TotalEvaluations += metrics.EvaluationCount;
            }

            if (Performance == null || metrics.P95Latency > 0 || metrics.P90Latency > 0 || metrics.AverageLatency > 0)
            {
                Performance = new(metrics.P95Latency, metrics.P90Latency, metrics.AverageLatency);
            }

            StartedOn = metrics.From;
            CompletedOn = metrics.To;
        }
    }
}
