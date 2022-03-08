using System;

namespace Microsoft.FeatureFlighting.Common.Model.AzureAppConfig
{
    public class AzureFlagInsights
    {   
        public int TotalEvaluations { get; set; }
        public int WeeklyEvaluations { get; set; }
        public double AverageEvaluationLatency { get; set; }
        public DateTime? LastEvaluatedOn { get; set; }
        public string LastEvaluatedBy { get; set; }
        public DateTime? MetricsLastUpdatedOn { get; set; }
        public bool ShowWarning { get; set; }
        public string WarningStatement { get; set; }
    }
}
