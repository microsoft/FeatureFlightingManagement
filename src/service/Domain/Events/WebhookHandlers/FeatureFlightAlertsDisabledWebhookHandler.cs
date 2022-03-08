using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Webhook;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.WebhookHandlers
{
    internal class FeatureFlightAlertsDisabledWebhookHandler : BaseFeatureFlightWebhookEventHandler<FeatureFlightAlertsDisabled>
    {
        protected override string NotificationSubject => null;

        protected override string NotificationContent => null;

        public FeatureFlightAlertsDisabledWebhookHandler(ITenantConfigurationProvider tenantConfigurationProvider, IWebhookTriggerManager webhookTriggerManager, IConfiguration configuration, ILogger logger)
            :base(tenantConfigurationProvider, webhookTriggerManager, configuration, logger)
        { }
    }
}
