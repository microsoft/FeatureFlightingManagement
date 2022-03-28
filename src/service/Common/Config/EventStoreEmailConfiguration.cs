namespace Microsoft.FeatureFlighting.Common.Config
{
    public class EventStoreEmailConfiguration
    {
        public string PublisherId { get; set; }
        public string PublisherName { get; set; }
        public string SenderAddress { get; set; }
        public string NotificationChannel { get; set; }
        public string EmailSubjectPrefix { get; set; }

        public string FeatureFlightCreatedEmailSubject { get; set; }
        public string FeatureFlightCreatedEmailTemplate { get; set; }

        public string FeatureFlightUpdatedEmailSubject { get; set; }
        public string FeatureFlightUpdatedEmailTemplate { get; set; }

        public string FeatureFlightEnabledEmailSubject { get; set; }
        public string FeatureFlightEnabledEmailTemplate { get; set; }

        public string FeatureFlightDisabledEmailSubject { get; set; }
        public string FeatureFlightDisabledEmailTemplate { get; set; }

        public string FeatureFlightDeletedEmailSubject { get; set; }
        public string FeatureFlightDeletedEmailTemplate { get; set; }

        public string FeatureFlightAlertsEnabledEmailSubject { get; set; }
        public string FeatureFlightAlertsEnabledTemplate { get; set; }
        

        public void SetDefaultEmailTemplates()
        {
            FeatureFlightCreatedEmailSubject = string.IsNullOrWhiteSpace(FeatureFlightCreatedEmailSubject) ? "A new feature flight \"<<FeatureName>>\" has been created" : FeatureFlightCreatedEmailSubject;
            FeatureFlightCreatedEmailTemplate = string.IsNullOrWhiteSpace(FeatureFlightCreatedEmailTemplate) ? "email-feature-flight-created" : FeatureFlightCreatedEmailTemplate;

            FeatureFlightUpdatedEmailSubject = string.IsNullOrWhiteSpace(FeatureFlightUpdatedEmailSubject) ? "Feature flight \"<<FeatureName>>\" has been updated" : FeatureFlightUpdatedEmailSubject;
            FeatureFlightUpdatedEmailTemplate = string.IsNullOrWhiteSpace(FeatureFlightUpdatedEmailTemplate) ? "email-feature-flight-updated" : FeatureFlightUpdatedEmailTemplate;

            FeatureFlightEnabledEmailSubject = string.IsNullOrWhiteSpace(FeatureFlightEnabledEmailSubject) ? "Feature flight \"<<FeatureName>>\" has been enabled" : FeatureFlightEnabledEmailSubject;
            FeatureFlightEnabledEmailTemplate = string.IsNullOrWhiteSpace(FeatureFlightEnabledEmailTemplate) ? "email-feature-flight-enabled" : FeatureFlightEnabledEmailTemplate;

            FeatureFlightDisabledEmailSubject = string.IsNullOrWhiteSpace(FeatureFlightDisabledEmailSubject) ? "Feature flight \"<<FeatureName>>\" has been disabled" : FeatureFlightDisabledEmailSubject;
            FeatureFlightDisabledEmailTemplate = string.IsNullOrWhiteSpace(FeatureFlightDisabledEmailTemplate) ? "email-feature-flight-disabled" : FeatureFlightDisabledEmailTemplate;

            FeatureFlightDeletedEmailSubject = string.IsNullOrWhiteSpace(FeatureFlightDeletedEmailSubject) ? "Feature flight \"<<FeatureName>>\" has been deleted" : FeatureFlightDeletedEmailSubject;
            FeatureFlightDeletedEmailTemplate = string.IsNullOrWhiteSpace(FeatureFlightDeletedEmailTemplate) ? "email-feature-flight-deleted" : FeatureFlightDeletedEmailTemplate;

            FeatureFlightAlertsEnabledEmailSubject = string.IsNullOrWhiteSpace(FeatureFlightAlertsEnabledEmailSubject) ? "Alerts for Feature flight \"<<FeatureName>>\" has been enabled" : FeatureFlightAlertsEnabledEmailSubject;
            FeatureFlightAlertsEnabledTemplate = string.IsNullOrWhiteSpace(FeatureFlightAlertsEnabledTemplate) ? "email-feature-flight-alerts-enabled" : FeatureFlightAlertsEnabledTemplate;
        }
    }
}
