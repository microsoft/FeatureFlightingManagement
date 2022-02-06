using System;
using CQRS.Mediatr.Lite.SDK.Domain;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Model;

namespace Microsoft.FeatureFlighting.Core.Domain.ValueObjects
{
    internal class Report: ValueObject
    {
        public ReportSettings Settings { get; private set; }

        public string DisplayStatement { get; private set; }

        public int ActivePeriod { get; private set; }
        public int InactivePeriod { get; private set; }
        public int UnusedPeriod { get; private set; }

        public bool HasActivePeriodCrossed { get; private set; }
        public bool HasInactivePeriodCrossed { get; private set; }
        public bool HasUnusedPeriodCrossed { get; private set; }
        public bool IsNew { get; private set; }

        public bool TriggerAlert => Settings.EnableReportGeneration 
            && Settings.EnableAlerts 
            && (HasActivePeriodCrossed || HasInactivePeriodCrossed || HasUnusedPeriodCrossed);

        public string GeneratedBy { get; private set; }
        public DateTime GeneratedOn { get; private set; }

        public Report(TenantConfiguration tenantConfiguration, FeatureUsageReportDto usageReport)
        {
            Settings = new(tenantConfiguration, usageReport?.EnableAlerts ?? true);
            if (usageReport == null)
                return;

            DisplayStatement = usageReport.UsageStatement;

            ActivePeriod = usageReport.ActivePeriod;
            InactivePeriod = usageReport.InactivePeriod;
            UnusedPeriod = usageReport.UnusedPeriod;

            HasActivePeriodCrossed = usageReport.HasActivationPeriodExceeded;
            HasInactivePeriodCrossed = usageReport.HasDisabledPeriodExceeded;
            HasUnusedPeriodCrossed = usageReport.HasUnsedPeriodExceeded;
            IsNew = usageReport.IsNew;
            
            GeneratedBy = usageReport.GeneratedBy;
            GeneratedOn = usageReport.GeneratedOn;
        }

        public void UpdateStatus(bool isNew, int activePeriod, int inactivePeriod, int unusedPeriod, string generatedBy, DateTime generatedOn)
        {
            IsNew = isNew;
            ActivePeriod = activePeriod;
            InactivePeriod = inactivePeriod;
            UnusedPeriod = unusedPeriod;

            HasActivePeriodCrossed = !IsNew && ActivePeriod > Settings.MaximumActivationPeriod;
            HasInactivePeriodCrossed = !IsNew && InactivePeriod > Settings.MaximumInactivePeriod;
            HasUnusedPeriodCrossed = !IsNew && unusedPeriod > Settings.MaximumUnusedPeriod;

            CreateStatement();

            GeneratedBy = generatedBy;
            GeneratedOn = generatedOn;
        }

        private void CreateStatement()
        {
            if (IsNew)
            {
                DisplayStatement = "The flight has been newly added";
                return;
            }

            if (HasUnusedPeriodCrossed)
            {
                DisplayStatement = $"The flight has been unused for more than {Settings.MaximumUnusedPeriod} days. Consider deleting the feature flight if the feature is not in use anymore.";
                return;
            }

            if (HasInactivePeriodCrossed)
            {
                DisplayStatement = $"The flight has been disabled for more than {Settings.MaximumInactivePeriod} days. Consider deleting the feature flight if the feature has been released to a global audience.";
                return;
            }

            if (HasActivePeriodCrossed)
            {
                DisplayStatement = $"The feature is being flighted for more than {Settings.MaximumInactivePeriod} days. Consider releasing or aborting the feature.";
                return;
            }

            DisplayStatement = "Feature flight in active usage";
        }
    }
}
