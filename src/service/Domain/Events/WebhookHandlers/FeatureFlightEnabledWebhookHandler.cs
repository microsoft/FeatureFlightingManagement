using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Webhook;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.WebhookHandlers
{
    internal class FeatureFlightEnabledWebhookHandler : BaseFeatureFlightWebhookEventHandler<FeatureFlightEnabled>
    {
        protected override string NotificationSubject => _emailConfiguration.FeatureFlightEnabledEmailSubject;

        protected override string NotificationContent => _emailConfiguration.FeatureFlightEnabledEmailTemplate;
        
        public FeatureFlightEnabledWebhookHandler(ITenantConfigurationProvider tenantConfigurationProvider, IWebhookTriggerManager webhookTriggerManager, IConfiguration emailConfiguration, ILogger logger)
            :base(tenantConfigurationProvider, webhookTriggerManager, emailConfiguration, logger)
        { }
    }
}
