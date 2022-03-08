using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Webhook;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.WebhookHandlers
{
    internal class FeatureFlightAlertsEnabledWebhookHandler : BaseFeatureFlightWebhookEventHandler<FeatureFlightAlertsEnabled>
    {
        protected override string NotificationSubject => _emailConfiguration.FeatureFlightAlertsEnabledEmailSubject;

        protected override string NotificationContent => _emailConfiguration.FeatureFlightAlertsEnabledTemplate;

        public FeatureFlightAlertsEnabledWebhookHandler(ITenantConfigurationProvider tenantConfigurationProvider, IWebhookTriggerManager webhookTriggerManager, IConfiguration configuration, ILogger logger)
            :base(tenantConfigurationProvider, webhookTriggerManager, configuration, logger)
        { }
    }
}
