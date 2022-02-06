using System;

namespace Microsoft.FeatureFlighting.Common.Model
{
    public class EvaluationMetricsDto
    {
        public DateTime? LastEvaluatedOn { get; set; }
        public int EvaluationCount { get; set; }
        public int TotalEvaluations { get; set; }
        public string LastEvaluatedBy { get; set; }
        public int P95Latency { get; set; }
        public int P90Latency { get; set; }
        public int AverageLatency { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
