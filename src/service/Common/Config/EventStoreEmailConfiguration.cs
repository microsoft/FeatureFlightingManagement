namespace Microsoft.FeatureFlighting.Common.Config
{
    public class EventStoreEmailConfiguration
    {
        public string PublisherId { get; set; }
        public string PublisherName { get; set; }
        public string SenderAddress { get; set; }
        public string NotificationChannel { get; set; }
        public string EmailSubjectPrefix { get; set; }

        public string FeatureFlagCreatedEmailSubject { get; set; }
        public string FeatureFlagCreatedEmailTemplate { get; set; }

        public string FeatureFlagUpdatedEmailSubject { get; set; }
        public string FeatureFlagUpdatedEmailTemplate { get; set; }

        public string FeatureFlagEnabledEmailSubject { get; set; }
        public string FeatureFlagEnabledEmailTemplate { get; set; }

        public string FeatureFlagDisabledEmailSubject { get; set; }
        public string FeatureFlagDisabledEmailTemplate { get; set; }
        
        public string InactiveFeatureFlagEmailTemplate { get; set; }

        public void SetDefaultEmailTemplates()
        {
            FeatureFlagCreatedEmailSubject = string.IsNullOrWhiteSpace(FeatureFlagCreatedEmailSubject) ? "A new feature flag \"<<FeatureName>>\" has been created" : FeatureFlagCreatedEmailSubject;
            FeatureFlagCreatedEmailTemplate = string.IsNullOrWhiteSpace(FeatureFlagCreatedEmailTemplate) ? "email-feature-flag-created" : FeatureFlagCreatedEmailTemplate;

            FeatureFlagUpdatedEmailSubject = string.IsNullOrWhiteSpace(FeatureFlagUpdatedEmailSubject) ? "Feature flag \"<<FeatureName>>\" has been created" : FeatureFlagUpdatedEmailSubject;
            FeatureFlagUpdatedEmailTemplate = string.IsNullOrWhiteSpace(FeatureFlagUpdatedEmailTemplate) ? "email-feature-flag-updated" : FeatureFlagUpdatedEmailTemplate;

            FeatureFlagEnabledEmailSubject = string.IsNullOrWhiteSpace(FeatureFlagEnabledEmailSubject) ? "Feature flag \"<<FeatureName>>\" has been enabled" : FeatureFlagEnabledEmailSubject;
            FeatureFlagEnabledEmailTemplate = string.IsNullOrWhiteSpace(FeatureFlagEnabledEmailTemplate) ? "email-feature-flag-enabled" : FeatureFlagEnabledEmailTemplate;

            FeatureFlagDisabledEmailSubject = string.IsNullOrWhiteSpace(FeatureFlagDisabledEmailSubject) ? "Feature flag \"<<FeatureName>>\" has been disabled" : FeatureFlagDisabledEmailSubject;
            FeatureFlagDisabledEmailTemplate = string.IsNullOrWhiteSpace(FeatureFlagDisabledEmailTemplate) ? "email-feature-flag-disabled" : FeatureFlagDisabledEmailTemplate;
        }
    }
}
