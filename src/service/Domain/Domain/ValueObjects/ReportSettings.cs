using CQRS.Mediatr.Lite.SDK.Domain;
using Microsoft.FeatureFlighting.Common.Config;

namespace Microsoft.FeatureFlighting.Core.Domain.ValueObjects
{
    internal class ReportSettings: ValueObject
    {
        public bool EnableReportGeneration { get; private set; }
        public bool Status { get; private set; }

        public bool VerifyActivationPeriod { get; private set; }
        public int MaximumActivationPeriod { get; private set; }
        
        public bool VerifyDisabledPeriod { get; private set; }
        public int MaximumInactivePeriod { get; private set; }
        
        public bool VerifyUnusedPeriod { get; private set; }
        public int MaximumUnusedPeriod { get; private set; }

        public bool VerifyLaunchedPeriod { get; private set; }
        public int MaximumLaunchedPeriod { get; private set; }

        public ReportSettings(TenantConfiguration tenantConfiguration, bool enableAlerts)
        {
            if (tenantConfiguration.IntelligentAlerts == null)
                return;

            EnableReportGeneration = tenantConfiguration.IsReportingEnabled();
            Status = enableAlerts;

            VerifyActivationPeriod = tenantConfiguration.IntelligentAlerts.MaximumActivePeriodAlertEnabled;
            MaximumActivationPeriod = tenantConfiguration.IntelligentAlerts.MaximumActivePeriod;

            VerifyDisabledPeriod = tenantConfiguration.IntelligentAlerts.MaximumDisabledPeriodAlertEnabled;
            MaximumInactivePeriod = tenantConfiguration.IntelligentAlerts.MaximumDisabledPeriod;

            VerifyUnusedPeriod = tenantConfiguration.IntelligentAlerts.MaximumUnusedPeriodAlertEnabled;
            MaximumUnusedPeriod = tenantConfiguration.IntelligentAlerts.MaximumUnusedPeriod;

            VerifyLaunchedPeriod = tenantConfiguration.IntelligentAlerts.MaxLaunchedPeriodAlertEnabled;
            MaximumLaunchedPeriod = tenantConfiguration.IntelligentAlerts.MaximumLaunchedPeriod;
        }

        public void DisableAlerts()
        {
            Status = false;
        }

        public void EnableAlerts()
        {
            Status = true;
        }
    }
}
