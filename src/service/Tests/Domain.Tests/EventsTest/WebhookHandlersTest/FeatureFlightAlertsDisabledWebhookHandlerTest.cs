using AppInsights.EnterpriseTelemetry;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;
using Microsoft.FeatureFlighting.Common.Model.ChangeNotification;
using Microsoft.FeatureFlighting.Common.Webhook;
using Microsoft.FeatureFlighting.Core.Domain;
using Microsoft.FeatureFlighting.Core.Domain.Events;
using Microsoft.FeatureFlighting.Core.Domain.ValueObjects;
using Microsoft.FeatureFlighting.Core.Events.WebhookHandlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.PS.FlightingService.Core.Tests.EventsTest.WebhookHandlersTest
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FeatureFlightAlertsDisabledWebhookHandlerTest
    {
        private Mock<ITenantConfigurationProvider> _mockTenantConfigurationProvider;
        private Mock<IWebhookTriggerManager> _mockWebhookTriggerManager;
        private IConfiguration _mockConfiguration;
        private Mock<ILogger> _mockLogger;
        private FeatureFlightAlertsDisabledWebhookHandler _handler;

        [TestInitialize]
        public void SetUp()
        {
            _mockTenantConfigurationProvider = new Mock<ITenantConfigurationProvider>();
            _mockWebhookTriggerManager = new Mock<IWebhookTriggerManager>();
            _mockConfiguration = new ConfigurationBuilder().AddJsonFile(@"appsettings.test.json").Build();
            _mockLogger = new Mock<ILogger>();
            _handler = new FeatureFlightAlertsDisabledWebhookHandler(_mockTenantConfigurationProvider.Object, _mockWebhookTriggerManager.Object, _mockConfiguration, _mockLogger.Object);
        }

        [TestMethod]
        public async Task Handle_ShouldTriggerWebhook()
        {
            var testEvent = GetFeatureFlightAlertsDisabled();
            var tenantConfiguration = new TenantConfiguration
            {
                ChangeNotificationSubscription = new TenantChangeNotificationConfiguration
                {
                    IsSubscribed = true,
                    SubscribedEvents = "*",
                    Webhook = new WebhookConfiguration()
                }
            };
            _mockTenantConfigurationProvider.Setup(t => t.Get(It.IsAny<string>())).ReturnsAsync(tenantConfiguration);

            await _handler.Handle(testEvent);

            _mockWebhookTriggerManager.Verify(w => w.Trigger(It.IsAny<WebhookConfiguration>(), It.IsAny<FeatureFlightChangeEventNotification>(), It.IsAny<LoggerTrackingIds>()), Times.Once);
        }

        [TestMethod]
        public void Handle_ShouldLogAndThrowException_WhenTriggeringWebhookFails()
        {
            var testEvent = GetFeatureFlightAlertsDisabled();
            var tenantConfiguration = new TenantConfiguration
            {
                ChangeNotificationSubscription = new TenantChangeNotificationConfiguration
                {
                    IsSubscribed = true,
                    SubscribedEvents = "*",
                    Webhook = new WebhookConfiguration()
                }
            };
            _mockTenantConfigurationProvider.Setup(t => t.Get(It.IsAny<string>())).ReturnsAsync(tenantConfiguration);
            _mockWebhookTriggerManager.Setup(w => w.Trigger(It.IsAny<WebhookConfiguration>(), It.IsAny<FeatureFlightChangeEventNotification>(), It.IsAny<LoggerTrackingIds>())).ThrowsAsync(new System.Exception());

            Assert.ThrowsExceptionAsync<System.Exception>(async () => await _handler.Handle(testEvent));

            _mockLogger.Verify(l => l.Log(It.IsAny<ExceptionContext>()), Times.Once);
        }

        private FeatureFlightAlertsDisabled GetFeatureFlightAlertsDisabled()
        {
            Feature feature = new Feature("test", "test");
            Status status = new Status(true, true, new List<string> { "test", "test" });
            Tenant tenant = new Tenant("test", "test");
            Settings settings = new Settings(new FlightOptimizationConfiguration());
            Condition condition = new Condition(true, new AzureFilterCollection());
            Audit audit = new Audit("test", DateTime.UtcNow, "test", DateTime.UtcNow, "test");
            FeatureFlighting.Core.Domain.ValueObjects.Version version = new FeatureFlighting.Core.Domain.ValueObjects.Version();
            LoggerTrackingIds loggerTrackingIds = new LoggerTrackingIds();
            FeatureFlightAggregateRoot featureFlightAggregateRoot = new FeatureFlightAggregateRoot(feature, status, tenant, settings, condition, audit, version);

            return new FeatureFlightAlertsDisabled(featureFlightAggregateRoot, loggerTrackingIds, "test source");
        }
    }
}
