using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;
using Microsoft.FeatureFlighting.Core.Domain;
using Microsoft.FeatureFlighting.Core.Domain.Events;
using Microsoft.FeatureFlighting.Core.Domain.ValueObjects;
using Microsoft.FeatureFlighting.Core.Events.CacheHandlers;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.PS.FlightingService.Core.Tests.EventsTest.CacheHandlersTest
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FeatureFlightCreatedCacheHandlerTest
    {
        private Mock<IFeatureFlightCache> _mockCache;
        private Mock<ILogger> _mockLogger;
        private FeatureFlightCreatedCacheHandler _handler;

        [TestInitialize]
        public void SetUp()
        {
            _mockCache = new Mock<IFeatureFlightCache>();
            _mockLogger = new Mock<ILogger>();
            _handler = new FeatureFlightCreatedCacheHandler(_mockCache.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task Handle_ShouldDeleteFeatureFlights()
        {
            var testEvent = GetFeatureFlightCreated();

            await _handler.Handle(testEvent);

            _mockCache.Verify(c => c.DeleteFeatureFlights(testEvent.TenantName, testEvent.Environment, It.IsAny<LoggerTrackingIds>()), Times.Once);
        }

        [TestMethod]
        public void Handle_ShouldLogAndThrowException_WhenDeleteFeatureFlightsThrowsException()
        {
            try
            {
                var testEvent = GetFeatureFlightCreated();
                _mockCache.Setup(c => c.DeleteFeatureFlights(testEvent.TenantName, testEvent.Environment, It.IsAny<LoggerTrackingIds>())).ThrowsAsync(new Exception());
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(ex.Message);
            }
        }

        private FeatureFlightCreated GetFeatureFlightCreated()
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

            return new FeatureFlightCreated(featureFlightAggregateRoot, loggerTrackingIds);
        }
    }
}
