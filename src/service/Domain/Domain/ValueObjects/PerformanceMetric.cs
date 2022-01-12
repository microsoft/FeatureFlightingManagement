namespace Microsoft.FeatureFlighting.Core.Domain.ValueObjects
{
    public class PerformanceMetric
    {
        public int Percentile95Latency { get; private set; }
        public int Percentile90Latency { get; private set; }
        public int AverageLatency { get; private set; }

        public PerformanceMetric(int p95, int p90, int averge)
        {
            Percentile95Latency = p95;
            Percentile90Latency = p90;
            AverageLatency = averge;
        }
    }
}
