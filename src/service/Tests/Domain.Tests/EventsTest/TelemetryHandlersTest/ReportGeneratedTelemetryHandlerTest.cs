using AppInsights.EnterpriseTelemetry;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Core.Domain.Events;
using Microsoft.FeatureFlighting.Core.Events.TelemetryHandlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.PS.FlightingService.Core.Tests.EventsTest.TelemetryHandlersTest
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ReportGeneratedTelemetryHandlerTest
    {
        private Mock<ILogger> _mockLogger;
        private ReportGeneratedTelemetryHandler _handler;

        [TestInitialize]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger>();
            _handler = new ReportGeneratedTelemetryHandler(_mockLogger.Object);
        }

        [TestMethod]
        public async Task Handle_ShouldLogEvent()
        {
            var testEvent = GetReportGenerated();

            await _handler.Handle(testEvent);

            _mockLogger.Verify(l => l.Log(It.IsAny<EventContext>()), Times.Once);
        }

        [TestMethod]
        public async Task Handle_ShouldNotThrowException_WhenLoggingFails()
        {
            var testEvent = GetReportGenerated();
            _mockLogger.Setup(l => l.Log(It.IsAny<EventContext>())).Throws(new System.Exception());

        }

        private ReportGenerated GetReportGenerated()
        {
            UsageReportDto usageReportDto = new UsageReportDto("test tenant", "env", "test user");

            return new ReportGenerated(usageReportDto,"test1","test2");
        }
    }
}
