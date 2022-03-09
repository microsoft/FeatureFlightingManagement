namespace Microsoft.FeatureFlighting.Common.Config
{
    public class TenantChangeNotificationConfiguration
    {
        public bool IsSubscribed { get; set; }
        public string SubscribedEvents { get; set; }
        public WebhookConfiguration Webhook { get; set; }
        public bool EnableEmailForChangeNotifications { get; set; }
        public string AdditionalNotificationReceivers { get; set; }
        
        public static TenantChangeNotificationConfiguration GetDefault()
        {
            return new TenantChangeNotificationConfiguration
            {
                IsSubscribed = false
            };
        }

        public void MergeOrDefault(TenantChangeNotificationConfiguration defaultConfiguration)
        {
            if (defaultConfiguration == null)
                return;

            if (!IsSubscribed)
                return;

            SubscribedEvents = !string.IsNullOrWhiteSpace(SubscribedEvents) ? SubscribedEvents : defaultConfiguration.SubscribedEvents;
            Webhook ??= defaultConfiguration.Webhook;
        }
    }
}
