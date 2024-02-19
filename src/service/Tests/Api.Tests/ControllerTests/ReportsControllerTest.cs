using AppInsights.EnterpriseTelemetry;
using CQRS.Mediatr.Lite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.API.Controllers;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Core.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.API.Tests.ControllerTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("ReportsController")]
    [TestClass]
    public class ReportsControllerTest
    {
        public Mock<IConfiguration> _mockConfiguration;
        public Mock<ICommandBus> _mockCommandBus;
        public Mock<ILogger> _mockogger;
        public ReportsController reportsController;
        public ReportsControllerTest() {
            
            _mockConfiguration = new Mock<IConfiguration>();
            _mockCommandBus = new Mock<ICommandBus>();
            _mockogger = new Mock<ILogger>();


            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["x-application"] = "test-Tenant";
            httpContext.Request.Headers["x-environment"] = "preprop";

            var testConfig = new Mock<IConfigurationSection>();
            testConfig.Setup(s => s.Value).Returns("preprop,prod");

            _mockConfiguration.Setup(c => c.GetSection("Env:Supported")).Returns(testConfig.Object);

            Command<IdCommandResult> Command = new UnsubscribeAlertsCommand("testFeature", "tesTenant", "preprop", "123", "1234", "test source");
            _mockCommandBus.Setup(c => c.Send(It.IsAny<Command<IdCommandResult>>()));
            reportsController = new ReportsController( _mockCommandBus.Object, _mockConfiguration.Object, _mockogger.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

        }

        [TestMethod]
        public async Task GenerateReport_Success() {
            _mockCommandBus.Setup(c => c.Send(It.IsAny<GenerateReportCommand>())).Returns(Task.FromResult(GetReportCommandResult()));
            var result= await reportsController.GenerateReport();
            var reportControllerResult = result as OkObjectResult;

            Assert.AreEqual(StatusCodes.Status200OK, reportControllerResult.StatusCode);
        } 

        private ReportCommandResult GetReportCommandResult()
        {
            return new ReportCommandResult(new UsageReportDto("tenant", "preprod", "adminuser") { ReportCreatedOn = DateTime.UtcNow })
            {
                ExecutedAt = DateTime.UtcNow,
                TimeTaken = new TimeSpan(100)
            };
        }
    }
}
