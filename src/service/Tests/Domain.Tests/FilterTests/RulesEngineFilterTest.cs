using AppInsights.EnterpriseTelemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.AppExceptions;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using Microsoft.FeatureFlighting.Core.Operators;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Core.Tests.FilterTests;
using Microsoft.FeatureManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.FeatureFlighting.Common.Constants;

namespace Microsoft.PS.FlightingService.Core.Tests.FilterTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("RoleFilter")]
    [TestClass]
    public class RulesEngineFilterTest:InitializeFilterTests
    {
        private Mock<IHttpContextAccessor> httpContextAccessorMockWithoutRoleGroup;
        private Mock<IHttpContextAccessor> httpContextAccessorMockInDefinedRoleGroup;
        private Mock<IHttpContextAccessor> httpContextAccessorMockNotInDefinedRoleGroup;
        private Mock<IRulesEngineManager> _mockRulesEngineManager;
        private FeatureFilterEvaluationContext featureContextOperatorNotEvaluates;
        private FeatureFilterEvaluationContext featureContextOperatorEvaluates;
        private Mock<ILogger> loggerMock;
        private Mock<IConfiguration> configMock;
        private readonly string ruleengine = "1";
        private Mock<IRulesEngineEvaluator> _mockRulesEngineEvaluator;
        

        [TestInitialize]
        public void TestStartup()
        {
            successfullMockEvaluatorStrategy = SetupMockOperatorEvaluatorStrategy(true);
            failureMockEvaluatorStrategy = SetupMockOperatorEvaluatorStrategy(false);

            httpContextAccessorMockWithoutRoleGroup = SetupHttpContextAccessorMock(httpContextAccessorMockWithoutRoleGroup, false, null);
            httpContextAccessorMockInDefinedRoleGroup = SetupHttpContextAccessorMock(httpContextAccessorMockInDefinedRoleGroup, true, "1");
            httpContextAccessorMockNotInDefinedRoleGroup = SetupHttpContextAccessorMock(httpContextAccessorMockNotInDefinedRoleGroup, true, "3");

            _mockRulesEngineManager=new Mock<IRulesEngineManager> ();
            _mockRulesEngineEvaluator = new Mock<IRulesEngineEvaluator>();

            featureContextOperatorNotEvaluates= SetFilterContext(featureContextOperatorNotEvaluates, Operator.NotEvaluates);
            featureContextOperatorEvaluates= SetFilterContext(featureContextOperatorEvaluates, Operator.Evaluates);
            configMock = SetConfigMock(configMock);
            loggerMock = SetLoggerMock(loggerMock);
        }


        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_Throws_Exception()
        {
            RulesEngineFilter rulesEngineFilter = new RulesEngineFilter(_mockRulesEngineManager.Object, httpContextAccessorMockInDefinedRoleGroup.Object, configMock.Object, loggerMock.Object);
            featureContextOperatorEvaluates.Settings = rulesEngineFilter.BindParameters(featureContextOperatorEvaluates.Parameters);
            try
            {
                var featureFlagStatus = await rulesEngineFilter.EvaluateAsync(featureContextOperatorEvaluates);
            }
            catch (System.Exception ex)
            {
                Assert.IsNotNull(ex.Message);
            }
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_Equals_Operator()
        {
            RulesEngineFilter rulesEngineFilter = new RulesEngineFilter(_mockRulesEngineManager.Object, httpContextAccessorMockInDefinedRoleGroup.Object, configMock.Object, loggerMock.Object);
            featureContextOperatorEvaluates.Settings = rulesEngineFilter.BindParameters(featureContextOperatorEvaluates.Parameters);
            _mockRulesEngineManager.Setup(x => x.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LoggerTrackingIds>())).ReturnsAsync(_mockRulesEngineEvaluator.Object);
            _mockRulesEngineEvaluator.Setup(x => x.Evaluate(It.IsAny<Dictionary<string, object>>(), It.IsAny<LoggerTrackingIds>())).ReturnsAsync(new EvaluationResult(true,"pass"));
           var featureFlagStatus = await rulesEngineFilter.EvaluateAsync(featureContextOperatorEvaluates);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_Equals_Operator()
        {
            RulesEngineFilter rulesEngineFilter = new RulesEngineFilter(_mockRulesEngineManager.Object, httpContextAccessorMockInDefinedRoleGroup.Object, configMock.Object, loggerMock.Object);
            featureContextOperatorNotEvaluates.Settings = rulesEngineFilter.BindParameters(featureContextOperatorNotEvaluates.Parameters);
            _mockRulesEngineManager.Setup(x => x.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<LoggerTrackingIds>())).ReturnsAsync(_mockRulesEngineEvaluator.Object);
            _mockRulesEngineEvaluator.Setup(x => x.Evaluate(It.IsAny<Dictionary<string, object>>(), It.IsAny<LoggerTrackingIds>())).ReturnsAsync(new EvaluationResult(false, "failed"));
            var featureFlagStatus = await rulesEngineFilter.EvaluateAsync(featureContextOperatorNotEvaluates);
            Assert.AreEqual(true, featureFlagStatus);
        }

        public Mock<IHttpContextAccessor> SetupHttpContextAccessorMock(Mock<IHttpContextAccessor> httpContextAccessorMock, bool hasRoleGroup, string ruleengine)
        {
            httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            Dictionary<string, string> contextParams = new Dictionary<string, string>();
            if (hasRoleGroup)
                contextParams.Add("ruleengine", ruleengine);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.Flighting.FLIGHT_CONTEXT_HEADER] = JsonConvert.SerializeObject(contextParams);
            httpContext.Items[Constants.Flighting.FLIGHT_TRACKER_PARAM] = JsonConvert.SerializeObject(new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            });
            httpContext.Items[Flighting.FEATURE_ADD_ENABLED_CONTEXT] = false;
            httpContext.Items[Flighting.FEATURE_ADD_DISABLED_CONTEXT] = false;
            httpContextAccessorMock.Setup(_ => _.HttpContext).Returns(httpContext);

            return httpContextAccessorMock;
        }

        private FeatureFilterEvaluationContext SetFilterContext(FeatureFilterEvaluationContext context, Operator filterOperator)
        {
            Dictionary<string, string> filterSettings = new Dictionary<string, string>
            {
                { "IsActive", "true" },
                { "StageId", "1" },
                { "Value", ruleengine }
            };

            switch (filterOperator)
            {
                case Operator.NotEvaluates:
                    filterSettings.Add("Operator", nameof(Operator.NotEvaluates));
                    break;
                case Operator.Evaluates:
                    filterSettings.Add("Operator", nameof(Operator.Evaluates));
                    break;
                default:
                    filterSettings.Add("Operator", nameof(Operator.NotEvaluates));
                    break;
            }
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(filterSettings)
                .Build();

            context = new FeatureFilterEvaluationContext
            {
                Parameters = configuration
            };
            return context;
        }  
    }
}
