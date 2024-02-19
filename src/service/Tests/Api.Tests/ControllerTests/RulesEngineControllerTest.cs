using AppInsights.EnterpriseTelemetry;
using CQRS.Mediatr.Lite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.API.Controllers;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Core.Operators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.API.Tests.ControllerTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("RulesEngineController")]
    [TestClass]
    public class RulesEngineControllerTest
    {
        public Mock<IConfiguration> _mockConfiguration;
        public Mock<ILogger> _mockogger;
        public Mock<IQueryService> _mockQueryService;

        public RulesEngineController rulesEngineController;
        public RulesEngineControllerTest() 
        {
            _mockQueryService = new Mock<IQueryService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockogger = new Mock<ILogger>();


            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["x-application"] = "test-Tenant";
            httpContext.Request.Headers["x-environment"] = "preprop";

            var testConfig = new Mock<IConfigurationSection>();
            testConfig.Setup(s => s.Value).Returns("preprop,prod");

            _mockConfiguration.Setup(c => c.GetSection("Env:Supported")).Returns(testConfig.Object);

            rulesEngineController = new RulesEngineController(_mockQueryService.Object, _mockConfiguration.Object, _mockogger.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };
        }

        [TestMethod]
        public async Task Evaluate_Success()
        {
            var workFlow = GetWorkflow();
            _mockQueryService.Setup(a => a.Query(It.IsAny<Query<EvaluationResult>>())).Returns(Task.FromResult<EvaluationResult>(GetEvaluationResult()));
            var result = await rulesEngineController.Evaluate("delete worflow",workFlow);

            var rulesEngineResult = result as OkObjectResult;

            Assert.AreEqual(rulesEngineResult.StatusCode, StatusCodes.Status200OK);
        }

        private Workflow GetWorkflow()
        {
            return new Workflow
            {
                WorkflowName = "test worflow name"
            };
        }

        private EvaluationResult GetEvaluationResult() 
        {
            return new EvaluationResult(true,"test message")
            {
                IsFaulted = false,
                Message = string.Empty,
                Result = true,
                TimeTaken = 100
            };
        }
    }
}
