using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Webhook;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.WebhookHandlers
{
    internal class FeatureFlightRebuiltWebhookHandler: BaseFeatureFlightWebhookEventHandler<FeatureFlightRebuilt>
    {
        protected override string NotificationSubject => null;

        protected override string NotificationContent => null;

        public FeatureFlightRebuiltWebhookHandler(ITenantConfigurationProvider tenantConfigurationProvider, IWebhookTriggerManager webhookTriggerManager, IConfiguration configuration, ILogger logger)
            : base(tenantConfigurationProvider, webhookTriggerManager, configuration, logger)
        { }
    }
}
