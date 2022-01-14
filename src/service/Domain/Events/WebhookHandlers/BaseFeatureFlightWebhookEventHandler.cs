using System.Linq;
using System.Text;
using Newtonsoft.Json;
using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Webhook;
using Microsoft.FeatureFlighting.Core.Domain.Events;
using Microsoft.FeatureFlighting.Common.Model.ChangeNotification;

namespace Microsoft.FeatureFlighting.Core.Events.WebhookHandlers
{
    internal abstract class BaseFeatureFlightWebhookEventHandler<TEvent> : EventHandler<TEvent> where TEvent : BaseFeatureFlightEvent
    {
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly IWebhookTriggerManager _webhookTriggerManager;
        protected readonly EventStoreEmailConfiguration _emailConfiguration;
        private readonly ILogger _logger;

        protected abstract string NotificationSubject { get; }
        protected abstract string NotificationContent { get; }

        protected virtual bool IsEmailNotificationEnabled => _emailConfiguration != null && !string.IsNullOrWhiteSpace(NotificationSubject) && !string.IsNullOrWhiteSpace(NotificationContent);

        public BaseFeatureFlightWebhookEventHandler(ITenantConfigurationProvider tenantConfigurationProvider, IWebhookTriggerManager webhookTriggerManager, EventStoreEmailConfiguration emailConfiguration, ILogger logger)
        {
            _tenantConfigurationProvider = tenantConfigurationProvider;
            _webhookTriggerManager = webhookTriggerManager;
            _emailConfiguration = emailConfiguration;
            _emailConfiguration.SetDefaultEmailTemplates();
            _logger = logger;
        }

        protected override async Task<VoidResult> ProcessRequest(TEvent @event)
        {
            try
            {
                TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(@event.TenantName);
                if (tenantConfiguration.ChangeNotificationSubscription == null || !tenantConfiguration.ChangeNotificationSubscription.IsSubscribed)
                    return new VoidResult();

                FeatureFlightChangeEventNotification changeNotification = CreateChangeNotification(@event, tenantConfiguration);
                WebhookConfiguration webhook = tenantConfiguration.ChangeNotificationSubscription.Webhook;
                await _webhookTriggerManager.Trigger(webhook, changeNotification, new Common.LoggerTrackingIds(@event.CorrelationId, @event.TransactionId));
                return new VoidResult();
            }
            catch (System.Exception exception)
            {
                System.Exception webhookException = new("There was an error in triggering the webhook, please invoke the webhook manually to avoid any errors. See inner exception for more details", exception);
                ExceptionContext context = new(webhookException, TraceLevel.Critical, @event.CorrelationId, @event.TransactionId, $"{@event.DisplayName}:WebhookHandler:{nameof(ProcessRequest)}", "", @event.FlagId);
                context.AddProperty("FlagId", @event.FlagId);
                _logger.Log(context);
                throw webhookException;
            }
        }

        private FeatureFlightChangeEventNotification CreateChangeNotification(TEvent @event, TenantConfiguration tenantConfiguration)
        {
            TenantChangeNotificationConfiguration changeNotificationConfiguration = tenantConfiguration.ChangeNotificationSubscription;
            FeatureFlightChangeEventNotification changeNotification = new()
            {
                EventName = @event.DisplayName,
                EventSubject = new StringBuilder().Append(@event.TenantName).Append(":").Append(@event.Environment).ToString(),
                EventType = changeNotificationConfiguration.EnableEmailForChangeNotifications ? "Notification" : "Entity",
                Payload = JsonConvert.SerializeObject(@event.Payload),
                Properties = @event.GetProperties(),
                Publisher = new(_emailConfiguration.PublisherId, _emailConfiguration.PublisherName),
                Notification = IsEmailNotificationEnabled && changeNotificationConfiguration.EnableEmailForChangeNotifications ? new NotificationCollection
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
                                Subject = new StringBuilder().Append(_emailConfiguration.EmailSubjectPrefix).Append(" ").Append(NotificationSubject).ToString(),
                                Content = NotificationContent,
                                ReceiverAddresses = new List<string> { tenantConfiguration.Contact },
                                AlternateReceiverAddreses = !string.IsNullOrWhiteSpace(changeNotificationConfiguration.AdditionalNotificationReceivers) ?
                                    changeNotificationConfiguration.AdditionalNotificationReceivers.Split(',').ToList() : null
                            }
                        }
                    }
                } : null
            };
            return changeNotification;
        }
    }
}
