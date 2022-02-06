using System;
using System.Linq;
using CQRS.Mediatr.Lite;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common.Model;

namespace Microsoft.FeatureFlighting.Core.Domain.Events
{
    /// <summary>
    /// Event when a report is generated
    /// </summary>
    public class ReportGenerated : Event
    {
        public override string DisplayName => nameof(ReportGenerated);

        public override string Id { get; set; }
        public UsageReportDto Report { get; set; }

        public ReportGenerated(UsageReportDto report, string correlationId, string transactionId)
        {
            Id = Guid.NewGuid().ToString();
            Report = report;
            CorrelationId = correlationId;
            TransactionId = transactionId;
        }

        public Dictionary<string, string> CreateProperties()
        {
            return new Dictionary<string, string>
            {
                { "Tenant", Report.Tenant },
                { "Environment", Report.Environment },
                { "TotalEvaluations", Report.TotalEvaluations.ToString() },
                { "UnusedPeriodThreshold", Report.UnusedPeriodThreshold.ToString() },
                { "InactivePeriodThreshold", Report.InactivePeriodThreshold.ToString() },
                { "ActivePeriodThreshold", Report.ActivePeriodThreshold.ToString() },
                { "ActiveFeatures", Report.ActiveFeatures != null ? string.Join(',', Report.ActiveFeatures) : "N/A" },
                { "ActiveFeaturesCount", Report.ActiveFeaturesCount.ToString() },
                { "NewlyAddedFeatures", Report.NewlyAddedFeatures != null ? string.Join(',', Report.NewlyAddedFeatures) : "N/A" },
                { "NewlyAddedFeaturesCount", Report.NewAddedFeaturesCount.ToString() },
                { "UnusedFeatures", Report.UnusedFeatures != null ? string.Join(',', Report.UnusedFeatures.Select(feature => feature.FeatureId) ) : "N/A" },
                { "LongInactiveFeatures", Report.LongInactiveFeatures != null ? string.Join(',', Report.LongInactiveFeatures.Select(feature => feature.FeatureId) ): "N/A" },
                { "LongActiveFeatures", Report.LongActiveFeatures != null ? string.Join(',', Report.LongActiveFeatures.Select(feature => feature.FeatureId) ) : "N/A" },
                { "ActionNeeded", Report.PendingAction.ToString() },
                { "ReportRequestedBy", Report.ReportRequestedBy },
                { "ReportRequestedOn", Report.ReportCreatedOn.ToString() }
            };
        }
    }
}
