﻿using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Webhook;
using Microsoft.FeatureFlighting.Core.Domain.Events;

namespace Microsoft.FeatureFlighting.Core.Events.WebhookHandlers
{
    internal class FeatureFlightUpdatedWebhookHandler : BaseFeatureFlightWebhookEventHandler<FeatureFlightUpdated>
    {
        protected override string NotificationSubject => _emailConfiguration.FeatureFlightUpdatedEmailSubject;

        protected override string NotificationContent => _emailConfiguration.FeatureFlightUpdatedEmailTemplate;

        public FeatureFlightUpdatedWebhookHandler(ITenantConfigurationProvider tenantConfigurationProvider, IWebhookTriggerManager webhookTriggerManager, IConfiguration emailConfiguration, ILogger logger)
            :base(tenantConfigurationProvider, webhookTriggerManager, emailConfiguration, logger)
        { }
    }
}
