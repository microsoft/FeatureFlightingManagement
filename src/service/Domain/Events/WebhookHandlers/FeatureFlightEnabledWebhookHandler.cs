using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Webhook;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.WebhookHandlers
{
    internal class FeatureFlightEnabledWebhookHandler : BaseFeatureFlightWebhookEventHandler<FeatureFlightEnabled>
    {
        protected override string NotificationSubject => _emailConfiguration.FeatureFlagEnabledEmailSubject;

        protected override string NotificationContent => _emailConfiguration.FeatureFlagEnabledEmailTemplate;
        
        public FeatureFlightEnabledWebhookHandler(ITenantConfigurationProvider tenantConfigurationProvider, IWebhookTriggerManager webhookTriggerManager, EventStoreEmailConfiguration emailConfiguration, ILogger logger)
            :base(tenantConfigurationProvider, webhookTriggerManager, emailConfiguration, logger)
        { }
    }
}
