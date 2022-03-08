namespace Microsoft.FeatureFlighting.Core.Domain.ValueObjects
{
    public class PerformanceMetric
    {
        public double Percentile95Latency { get; private set; }
        public double Percentile90Latency { get; private set; }
        public double AverageLatency { get; private set; }

        public PerformanceMetric(double p95, double p90, double averge)
        {
            Percentile95Latency = p95;
            Percentile90Latency = p90;
            AverageLatency = averge;
        }
    }
}
