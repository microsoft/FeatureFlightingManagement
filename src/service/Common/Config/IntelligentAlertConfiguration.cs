namespace Microsoft.FeatureFlighting.Common.Config
{
    /// <summary>
    /// Configuration for sending intelligent alerts
    /// </summary>
    public class IntelligentAlertConfiguration
    {
        public bool Enabled { get; set; }

        public string AlertEventName { get; set; }
        public string AlertEmailTemplate { get; set; }
        public string AlertEmailSubject { get; set; }

        public int MaximumActivePeriod { get; set; }
        public bool MaximumActivePeriodAlertEnabled { get; set; }
        
        public int MaximumDisabledPeriod { get; set; }
        public bool MaximumDisabledPeriodAlertEnabled { get; set; }

        public int MaximumUnusedPeriod { get; set; }
        public bool MaximumUnusedPeriodAlertEnabled { get; set; }

        public int MaximumLaunchedPeriod { get; set; }
        public bool MaxLaunchedPeriodAlertEnabled { get; set; }

        public static IntelligentAlertConfiguration GetDefault()
        {
            return new IntelligentAlertConfiguration
            {
                Enabled = true,

                AlertEventName = "Feature-Management-Intelligent-Digest-Report-Generated",
                AlertEmailSubject = "Weekly Digest",
                AlertEmailTemplate = "email-feature-management-weekly-digest",

                MaximumActivePeriod = 90,
                MaximumActivePeriodAlertEnabled = true,

                MaximumDisabledPeriod = 30,
                MaximumDisabledPeriodAlertEnabled = true,

                MaximumUnusedPeriod = 30,
                MaximumUnusedPeriodAlertEnabled = false,

                MaximumLaunchedPeriod = 15,
                MaxLaunchedPeriodAlertEnabled = true
            };
        }

        public void MergeWithDefault(IntelligentAlertConfiguration defaultConfiguration)
        {
            AlertEventName ??= defaultConfiguration.AlertEventName;
            AlertEmailSubject ??= defaultConfiguration.AlertEmailSubject;
            AlertEmailTemplate ??= defaultConfiguration.AlertEmailTemplate;
        }
    }
}
