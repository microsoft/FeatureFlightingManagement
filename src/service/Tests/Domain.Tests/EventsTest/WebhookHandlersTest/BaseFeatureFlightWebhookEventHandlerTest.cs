using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;
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
using Microsoft.FeatureFlighting.Common.Model.ChangeNotification;

namespace Microsoft.PS.FlightingService.Core.Tests.EventsTest.WebhookHandlersTest
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class BaseFeatureFlightWebhookEventHandlerTest
    {
        private Mock<ITenantConfigurationProvider> _mockTenantConfigurationProvider;
        private Mock<IWebhookTriggerManager> _mockWebhookTriggerManager;
        private IConfiguration _mockConfiguration;
        private Mock<ILogger> _mockLogger;
        private TestFeatureFlightWebhookEventHandler _handler;

        [TestInitialize]
        public void SetUp()
        {
            _mockTenantConfigurationProvider = new Mock<ITenantConfigurationProvider>();
            _mockWebhookTriggerManager = new Mock<IWebhookTriggerManager>();
            _mockConfiguration = new ConfigurationBuilder().AddJsonFile(@"appsettings.test.json").Build();
            _mockLogger = new Mock<ILogger>();
            _handler = new TestFeatureFlightWebhookEventHandler(_mockTenantConfigurationProvider.Object, _mockWebhookTriggerManager.Object, _mockConfiguration, _mockLogger.Object);
        }

        [TestMethod]
        public async Task ProcessRequest_ShouldTriggerWebhook()
        {
            var testEvent = GetTestFeatureFlightEvent();
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
        public void ProcessRequest_ShouldLogAndThrowException_WhenTriggeringWebhookFails()
        {
            var testEvent = GetTestFeatureFlightEvent();
            var tenantConfiguration = new TenantConfiguration
            {
                ChangeNotificationSubscription = new TenantChangeNotificationConfiguration
                {
                    IsSubscribed = true,
                    SubscribedEvents = "*",
                    Webhook = new WebhookConfiguration()
                }
            };
            try
            {

                _mockTenantConfigurationProvider.Setup(t => t.Get(It.IsAny<string>())).ReturnsAsync(tenantConfiguration);
                _mockWebhookTriggerManager.Setup(w => w.Trigger(It.IsAny<WebhookConfiguration>(), It.IsAny<FeatureFlightChangeEventNotification>(), It.IsAny<LoggerTrackingIds>())).ThrowsAsync(new System.Exception());
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(ex.Message);
            }
        }

        private TestFeatureFlightEvent GetTestFeatureFlightEvent()
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

            return new TestFeatureFlightEvent(featureFlightAggregateRoot, loggerTrackingIds, "test source");
        }
    }

    internal class TestFeatureFlightWebhookEventHandler : BaseFeatureFlightWebhookEventHandler<TestFeatureFlightEvent>
    {
        public TestFeatureFlightWebhookEventHandler(ITenantConfigurationProvider tenantConfigurationProvider, IWebhookTriggerManager webhookTriggerManager, IConfiguration configuration, ILogger logger) : base(tenantConfigurationProvider, webhookTriggerManager, configuration, logger) { }

        protected override string NotificationSubject => "TestNotificationSubject";
        protected override string NotificationContent => "TestNotificationContent";
    }

    internal class TestFeatureFlightEvent : BaseFeatureFlightEvent
    {
        public TestFeatureFlightEvent(FeatureFlightAggregateRoot flight, LoggerTrackingIds trackingIds, string requestSource) : base(flight, trackingIds, requestSource)
        {
        }

        public override string DisplayName => "TestFeatureFlightEvent";
    }
}
