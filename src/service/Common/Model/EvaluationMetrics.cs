using System;

namespace Microsoft.FeatureFlighting.Common.Model
{
    /// <summary>
    /// Evaluation metrics of a feature flight
    /// </summary>
    public class EvaluationMetricsDto
    {
        public DateTime? LastEvaluatedOn { get; set; }
        public int EvaluationCount { get; set; }
        public int TotalEvaluations { get; set; }
        public string LastEvaluatedBy { get; set; }
        public double P95Latency { get; set; }
        public double P90Latency { get; set; }
        public double AverageLatency { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public static EvaluationMetricsDto GetDefault()
        {
            return new EvaluationMetricsDto
            {
                EvaluationCount = 0,
                TotalEvaluations = 0,
                AverageLatency = 0.0,
                P95Latency = 0.0,
                P90Latency = 0.0,
            };
        }
    }
}
