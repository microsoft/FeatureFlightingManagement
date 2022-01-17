using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Webhook;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.WebhookHandlers
{
    internal class FeatureFlightDisabledWebhookHandler : BaseFeatureFlightWebhookEventHandler<FeatureFlightDisabled>
    {
        protected override string NotificationSubject => _emailConfiguration.FeatureFlightDisabledEmailSubject;

        protected override string NotificationContent => _emailConfiguration.FeatureFlightDisabledEmailTemplate;
        
        public FeatureFlightDisabledWebhookHandler(ITenantConfigurationProvider tenantConfigurationProvider, IWebhookTriggerManager webhookTriggerManager, EventStoreEmailConfiguration emailConfiguration, ILogger logger)
            :base(tenantConfigurationProvider, webhookTriggerManager, emailConfiguration, logger)
        { }
    }
}
