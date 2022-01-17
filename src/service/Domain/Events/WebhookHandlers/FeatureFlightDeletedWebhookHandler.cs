using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Webhook;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.WebhookHandlers
{
    internal class FeatureFlightDeletedWebhookHandler : BaseFeatureFlightWebhookEventHandler<FeatureFlightDeleted>
    {
        protected override string NotificationSubject => _emailConfiguration.FeatureFlightDeletedEmailSubject;

        protected override string NotificationContent => _emailConfiguration.FeatureFlightDeletedEmailTemplate;

        public FeatureFlightDeletedWebhookHandler(ITenantConfigurationProvider tenantConfigurationProvider, IWebhookTriggerManager webhookTriggerManager, EventStoreEmailConfiguration emailConfiguration, ILogger logger)
            :base(tenantConfigurationProvider, webhookTriggerManager, emailConfiguration, logger)
        { }
    }
}
