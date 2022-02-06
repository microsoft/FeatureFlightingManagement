using System.Linq;
using System.Text;
using Newtonsoft.Json;
using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Webhook;
using Microsoft.FeatureFlighting.Core.Domain.Events;
using Microsoft.FeatureFlighting.Common.Model.ChangeNotification;

namespace Microsoft.FeatureFlighting.Core.Events.WebhookHandlers
{
    /// <summary>
    /// Handles <see cref="ReportGenerated"/>
    /// </summary>
    internal class ReportGeneratedWebhookHandler : EventHandler<ReportGenerated>
    {
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly IWebhookTriggerManager _webhookTriggerManager;
        protected readonly EventStoreEmailConfiguration _emailConfiguration;

        public ReportGeneratedWebhookHandler(ITenantConfigurationProvider tenantConfigurationProvider, IWebhookTriggerManager webhookTriggerManager, IConfiguration configuration)
        {
            _tenantConfigurationProvider = tenantConfigurationProvider;
            _webhookTriggerManager = webhookTriggerManager;
            _emailConfiguration = configuration.GetSection("EventStore:Email").Get<EventStoreEmailConfiguration>();
            _emailConfiguration.SetDefaultEmailTemplates();
        }

        /// <summary>
        /// Invokes the configured webhook with the weekly digest notification
        /// </summary>
        /// <param name="event" cref="ReportGenerated">Event</param>
        protected override async Task<VoidResult> ProcessRequest(ReportGenerated @event)
        {
            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(@event.Report.Tenant);
            if (!IsSubscribed(tenantConfiguration, @event))
                return new VoidResult();

            FeatureFlightChangeEventNotification notification = CreateWeeklyDigestNotification(@event, tenantConfiguration);
            WebhookConfiguration webhook = tenantConfiguration.ChangeNotificationSubscription.Webhook;
            await _webhookTriggerManager.Trigger(webhook, notification, new Common.LoggerTrackingIds(@event.CorrelationId, @event.TransactionId));
            return new VoidResult();
        }

        private FeatureFlightChangeEventNotification CreateWeeklyDigestNotification(ReportGenerated @event, TenantConfiguration tenantConfiguration)
        {
            UsageReportDto report = @event.Report;
            FeatureFlightChangeEventNotification notification = new()
            {
                EventName = tenantConfiguration.IntelligentAlerts.AlertEventName,
                EventSubject = new StringBuilder().Append(tenantConfiguration.Name).Append(":").Append(report.Environment).ToString(),
                EventType = "Notification",
                Payload = string.Empty,
                Properties = @event.CreateProperties(),
                Publisher = new(_emailConfiguration.PublisherId, _emailConfiguration.PublisherName),
                Notification = new NotificationCollection
                {
                    EmailNotification = new()
                    {
                        Channel = _emailConfiguration.NotificationChannel,
                        Enabled = true,
                        Notifications = new List<Notification>
                        {
                            new Notification
                            {
                                SenderAddress = _emailConfiguration.SenderAddress,
                                Subject = new StringBuilder().Append(_emailConfiguration.EmailSubjectPrefix).Append(tenantConfiguration.IntelligentAlerts.AlertEmailSubject).ToString(),
                                Content = tenantConfiguration.IntelligentAlerts.AlertEmailTemplate,
                                ReceiverAddresses = new List<string> { tenantConfiguration.Contact },
                                AlternateReceiverAddreses = !string.IsNullOrWhiteSpace(tenantConfiguration.ChangeNotificationSubscription.AdditionalNotificationReceivers) ?
                                    tenantConfiguration.ChangeNotificationSubscription.AdditionalNotificationReceivers.Split(',').ToList() : null,
                                Properties = new Dictionary<string, string>
                                {
                                    { "AdvancedRenderingParameters", JsonConvert.SerializeObject(report) }
                                }
                            }
                        }
                    }
                }
            };
            return notification;
        }

        private bool IsSubscribed(TenantConfiguration tenantConfiguration, ReportGenerated @event)
        {
            if (tenantConfiguration.ChangeNotificationSubscription == null || !tenantConfiguration.ChangeNotificationSubscription.IsSubscribed)
                return false;

            if (tenantConfiguration.IntelligentAlerts == null || !tenantConfiguration.IntelligentAlerts.Enabled)
                return false;

            List<string> subscribedEvents = !string.IsNullOrWhiteSpace(tenantConfiguration.ChangeNotificationSubscription.SubscribedEvents)
                ? tenantConfiguration.ChangeNotificationSubscription.SubscribedEvents.Split(",").ToList()
                : new List<string>();

            if (subscribedEvents == null || !subscribedEvents.Any())
                return false;

            if (subscribedEvents[0].ToLowerInvariant() == "*".ToLowerInvariant())
                return true;

            return subscribedEvents.Any(subscribedEvent => subscribedEvent.ToLowerInvariant() == @event.DisplayName.ToLowerInvariant());
        }
    }
}
