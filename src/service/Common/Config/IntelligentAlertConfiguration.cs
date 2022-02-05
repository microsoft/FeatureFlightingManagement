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
        
        public int MaximumDeactivePeriod { get; set; }
        public bool MaximumDeactivePeriodAlertEnabled { get; set; }

        public int MaximumLastEvaluatedPeriod { get; set; }
        public bool MaximumLastEvaluatedPeriodAlertEnabled { get; set; }

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

                MaximumDeactivePeriod = 30,
                MaximumDeactivePeriodAlertEnabled = true,

                MaximumLastEvaluatedPeriod = 30,
                MaximumLastEvaluatedPeriodAlertEnabled = false
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
