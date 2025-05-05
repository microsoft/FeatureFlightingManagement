﻿using AppInsights.EnterpriseTelemetry;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;
using Microsoft.FeatureFlighting.Core.Domain;
using Microsoft.FeatureFlighting.Core.Domain.Events;
using Microsoft.FeatureFlighting.Core.Domain.ValueObjects;
using Microsoft.FeatureFlighting.Core.Events.TelemetryHandlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.PS.FlightingService.Core.Tests.EventsTest.TelemetryHandlersTest
{
    internal class FeatureFlightUpdatedTelemetryHandlerTest
    {
        private Mock<ILogger> _mockLogger;
        private FeatureFlightUpdatedTelemetryHandler _handler;

        [TestInitialize]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger>();
            _handler = new FeatureFlightUpdatedTelemetryHandler(_mockLogger.Object);
        }

        [TestMethod]
        public async Task Handle_ShouldLogEvent()
        {
            var testEvent = GetTestFeatureFlightEvent();

            await _handler.Handle(testEvent);

            _mockLogger.Verify(l => l.Log(It.IsAny<EventContext>()), Times.Once);
        }

        [TestMethod]
        public async Task Handle_ShouldNotThrowException_WhenLoggingFails()
        {
            try
            {
                var testEvent = GetTestFeatureFlightEvent();
                _mockLogger.Setup(l => l.Log(It.IsAny<EventContext>())).Throws(new System.Exception());
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(ex.Message);
            }
        }

        private FeatureFlightUpdated GetTestFeatureFlightEvent()
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
            FeatureFlightDto featureFlightDto = new FeatureFlightDto() { };

            return new FeatureFlightUpdated(featureFlightAggregateRoot, featureFlightDto, "test type", loggerTrackingIds, "test source");
        }
    }
}
