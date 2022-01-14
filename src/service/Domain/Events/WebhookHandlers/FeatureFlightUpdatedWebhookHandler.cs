using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Webhook;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.WebhookHandlers
{
    internal class FeatureFlightUpdatedWebhookHandler : BaseFeatureFlightWebhookEventHandler<FeatureFlightUpdated>
    {
        protected override string NotificationSubject => _emailConfiguration.FeatureFlagUpdatedEmailSubject;

        protected override string NotificationContent => _emailConfiguration.FeatureFlagUpdatedEmailTemplate;

        public FeatureFlightUpdatedWebhookHandler(ITenantConfigurationProvider tenantConfigurationProvider, IWebhookTriggerManager webhookTriggerManager, EventStoreEmailConfiguration emailConfiguration, ILogger logger)
            :base(tenantConfigurationProvider, webhookTriggerManager, emailConfiguration, logger)
        { }
    }
}
