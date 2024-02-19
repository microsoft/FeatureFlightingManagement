using AppInsights.EnterpriseTelemetry;
using CQRS.Mediatr.Lite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.API.Controllers;
using Microsoft.FeatureFlighting.Core.Commands;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.API.Tests.ControllerTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("FeatureFlagsEvaluationController")]
    [TestClass]
    public class FeatureFlagsEvaluationControllerTest
    {
        public Mock<IConfiguration> _mockConfiguration;
        public Mock<ICommandBus> _mockCommandBus;
        public Mock<ILogger> _mockogger;
        public Mock<IQueryService> _mockQueryService;
        public Mock<IFeatureFlagEvaluator> _mockFeatureFlagEvaluator;
        public Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        public FeatureFlagsEvaluationController featureFlagsEvaluationController;

        public FeatureFlagsEvaluationControllerTest()
        {
            _mockQueryService = new Mock<IQueryService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockCommandBus = new Mock<ICommandBus>();
            _mockogger = new Mock<ILogger>();
            _mockFeatureFlagEvaluator = new Mock<IFeatureFlagEvaluator>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            Mock<HttpContext> _mockHttpContext = new Mock<HttpContext>();
            Mock<HttpRequest> _mockHttpRequest = new Mock<HttpRequest>();
            //Mock<HttpHeaders> _mockHttpHeaders = new Mock<HttpHeaders>();
            Mock<IHeaderDictionary> _mockHeaderDictionary = new Mock<IHeaderDictionary>();
            _mockHttpContext.Setup(hc=>hc.Request).Returns(new Mock<HttpRequest>().Object);
            _mockHttpContext.Setup(hc=>hc.Request.Headers).Returns(_mockHeaderDictionary.Object);
            _mockHttpContextAccessor.Setup(h=>h.HttpContext).Returns(_mockHttpContext.Object);


            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["x-application"] = "test-Tenant";
            httpContext.Request.Headers["x-environment"] = "preprop";

            var testConfig = new Mock<IConfigurationSection>();
            testConfig.Setup(s => s.Value).Returns("preprop,prod");

            _mockConfiguration.Setup(c => c.GetSection("Env:Supported")).Returns(testConfig.Object);

            Command<IdCommandResult> Command = new UnsubscribeAlertsCommand("testFeature", "tesTenant", "preprop", "123", "1234", "test source");
            _mockCommandBus.Setup(c => c.Send(It.IsAny<Command<IdCommandResult>>()));
            featureFlagsEvaluationController = new FeatureFlagsEvaluationController(_mockFeatureFlagEvaluator.Object, _mockQueryService.Object, _mockHttpContextAccessor.Object, _mockConfiguration.Object, _mockogger.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };
        }

        [TestMethod]
        public async Task EvaluateFeatureFlag_Backward_Success_when_featureNames_is_filled_with_whitespaces()
        {
           var result= await featureFlagsEvaluationController.EvaluateFeatureFlag_Backward("FeatureFlite", "preprod", "");
            var evaluateFeatureFlagResult = result as OkObjectResult;
            Assert.AreEqual(evaluateFeatureFlagResult.StatusCode, StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task EvaluateFeatureFlag_Backward_Success_when_featureNames_is_null()
        {
            var result = await featureFlagsEvaluationController.EvaluateFeatureFlag_Backward("FeatureFlite", "preprod", null);
            var evaluateFeatureFlagResult = result as OkObjectResult;
            Assert.AreEqual(evaluateFeatureFlagResult.StatusCode, StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task EvaluateFeatureFlag_Backward_Success_when_featureNames_has_data()
        {
            var result = await featureFlagsEvaluationController.EvaluateFeatureFlag_Backward("FeatureFlite", "preprod", "add,update,delete");
            var evaluateFeatureFlagResult = result as OkObjectResult;
            Assert.AreEqual(evaluateFeatureFlagResult.StatusCode, StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task EvaluateFeatureFlag_Success_when_featureNames_is_filled_with_whitespaces()
        {
            var result = await featureFlagsEvaluationController.EvaluateFeatureFlag("");
            var evaluateFeatureFlagResult = result as OkObjectResult;
            Assert.AreEqual(evaluateFeatureFlagResult.StatusCode, StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task EvaluateFeatureFlag_Success_when_featureNames_is_null()
        {
            var result = await featureFlagsEvaluationController.EvaluateFeatureFlag(null);
            var evaluateFeatureFlagResult = result as OkObjectResult;
            Assert.AreEqual(evaluateFeatureFlagResult.StatusCode, StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task EvaluateFeatureFlag_Success_when_featureNames_has_data()
        {
            var result = await featureFlagsEvaluationController.EvaluateFeatureFlag("add,update,delete");
            var evaluateFeatureFlagResult = result as OkObjectResult;
            Assert.AreEqual(evaluateFeatureFlagResult.StatusCode, StatusCodes.Status200OK);
        }
    }
}
