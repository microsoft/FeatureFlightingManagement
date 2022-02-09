using System;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;

namespace Microsoft.FeatureFlighting.Core.Domain.Events
{
    /// <summary>
    /// Event when the evalution metrics of a feature flight is updated
    /// </summary>
    internal class FeatureFlightMetricsUpdated : BaseFeatureFlightEvent
    {
        public override string DisplayName => nameof(FeatureFlightMetricsUpdated);

        public DateTime MetricsCalculationStartTime { get; set; }
        public DateTime MetricsCalculationEndTime { get; set; }

        public int WeeklyEvaluationCount { get; set; }
        public int TotalEvaluations { get; set; }
        public double AverageLatency { get; set; }
        public double P95Latency { get; set; }
        public double P90Latency { get; set; }
        public string MetricsUpdatedBy { get; set; }

        public FeatureFlightMetricsUpdated(FeatureFlightAggregateRoot flight, LoggerTrackingIds trackingIds, string source)
            :base(flight, trackingIds, source)
        {
            MetricsCalculationStartTime = flight.EvaluationMetrics.StartedOn;
            MetricsCalculationEndTime = flight.EvaluationMetrics.CompletedOn;
            WeeklyEvaluationCount = flight.EvaluationMetrics.EvaluationCount;
            TotalEvaluations = flight.EvaluationMetrics.TotalEvaluations;
            AverageLatency = flight.EvaluationMetrics.Performance.AverageLatency;
            P95Latency = flight.EvaluationMetrics.Performance.Percentile95Latency;
            P90Latency = flight.EvaluationMetrics.Performance.Percentile90Latency;
            MetricsUpdatedBy = flight.Audit.LastModifiedBy;
        }

        public override Dictionary<string, string> GetProperties()
        {
            Dictionary<string, string> properties = base.GetProperties();
            properties.AddOrUpdate(nameof(MetricsCalculationStartTime), MetricsCalculationStartTime.ToString());
            properties.AddOrUpdate(nameof(MetricsCalculationEndTime), MetricsCalculationEndTime.ToString());
            properties.AddOrUpdate(nameof(WeeklyEvaluationCount), WeeklyEvaluationCount.ToString());
            properties.AddOrUpdate(nameof(TotalEvaluations), TotalEvaluations.ToString());
            properties.AddOrUpdate(nameof(AverageLatency), AverageLatency.ToString());
            properties.AddOrUpdate(nameof(P95Latency), P95Latency.ToString());
            properties.AddOrUpdate(nameof(P90Latency), P90Latency.ToString());
            properties.AddOrUpdate(nameof(MetricsUpdatedBy), MetricsUpdatedBy);
            return properties;
        }
    }
}
