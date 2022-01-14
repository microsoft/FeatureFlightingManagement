using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Webhook;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.WebhookHandlers
{
    internal class FeatureFlightStageUpdatedWebhookHandler : BaseFeatureFlightWebhookEventHandler<FeatureFlightStageActivated>
    {
        protected override string NotificationSubject => null;

        protected override string NotificationContent => null;
        
        public FeatureFlightStageUpdatedWebhookHandler(ITenantConfigurationProvider tenantConfigurationProvider, IWebhookTriggerManager webhookTriggerManager, EventStoreEmailConfiguration emailConfiguration, ILogger logger)
            :base(tenantConfigurationProvider, webhookTriggerManager, emailConfiguration, logger)
        { }
    }
}
