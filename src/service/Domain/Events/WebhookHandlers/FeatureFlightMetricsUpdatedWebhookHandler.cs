using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Webhook;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.WebhookHandlers
{
    internal class FeatureFlightMetricsUpdatedWebhookHandler : BaseFeatureFlightWebhookEventHandler<FeatureFlightMetricsUpdated>
    {
        protected override string NotificationSubject => null;

        protected override string NotificationContent => null;

        public FeatureFlightMetricsUpdatedWebhookHandler(ITenantConfigurationProvider tenantConfigurationProvider, IWebhookTriggerManager webhookTriggerManager, IConfiguration emailConfiguration, ILogger logger)
            :base(tenantConfigurationProvider, webhookTriggerManager, emailConfiguration, logger)
        { }
    }
}
