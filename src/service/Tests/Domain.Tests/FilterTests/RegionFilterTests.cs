using Moq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Core.Tests.FilterTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("RegionFilter")]
    [TestClass]
    public class RegionFilterTests:InitializeFilterTests
    {
        private Mock<IHttpContextAccessor> httpContextAccessorMockWithoutRegion;
        private Mock<IHttpContextAccessor> httpContextAccessorMockInDefinedRegion;
        private Mock<IHttpContextAccessor> httpContextAccessorMockNotInDefinedRegion;
        private FeatureFilterEvaluationContext featureContextOperatorIn;
        private FeatureFilterEvaluationContext featureContextOperatorNotIn;
        private FeatureFilterEvaluationContext featureContextOperatorEquals;
        private FeatureFilterEvaluationContext featureContextOperatorNotEquals;
        private Mock<ILogger> loggerMock;
        private Mock<IConfiguration> configMock;
        private readonly string region = "hyderabad";

        [TestInitialize]
        public void TestStartup()
        {
            successfullMockEvaluatorStrategy = SetupMockOperatorEvaluatorStrategy(true);
            failureMockEvaluatorStrategy = SetupMockOperatorEvaluatorStrategy(false);

            httpContextAccessorMockWithoutRegion = SetupHttpContextAccessorMock(httpContextAccessorMockWithoutRegion, false, null);
            httpContextAccessorMockInDefinedRegion = SetupHttpContextAccessorMock(httpContextAccessorMockInDefinedRegion, true, "hyderabad");
            httpContextAccessorMockNotInDefinedRegion = SetupHttpContextAccessorMock(httpContextAccessorMockNotInDefinedRegion, true, "Cambridge");

            featureContextOperatorIn = SetFilterContext(featureContextOperatorIn, Operator.In);
            featureContextOperatorNotIn = SetFilterContext(featureContextOperatorNotIn, Operator.NotIn);
            featureContextOperatorEquals = SetFilterContext(featureContextOperatorEquals, Operator.Equals);
            featureContextOperatorNotEquals = SetFilterContext(featureContextOperatorNotEquals, Operator.NotEquals);
            configMock = SetConfigMock(configMock);
            loggerMock = SetLoggerMock(loggerMock);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_Equals_Operator()
        {
            RegionFilter RegionFilter = new RegionFilter(configMock.Object, httpContextAccessorMockInDefinedRegion.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorEquals.Settings = RegionFilter.BindParameters(featureContextOperatorEquals.Parameters);
            var featureFlagStatus = await RegionFilter.EvaluateAsync(featureContextOperatorEquals);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_Equals_Operator()
        {
            RegionFilter RegionFilter = new RegionFilter(configMock.Object, httpContextAccessorMockNotInDefinedRegion.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorEquals.Settings = RegionFilter.BindParameters(featureContextOperatorEquals.Parameters);
            var featureFlagStatus = await RegionFilter.EvaluateAsync(featureContextOperatorEquals);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_NotEquals_Operator()
        {
            RegionFilter RegionFilter = new RegionFilter(configMock.Object, httpContextAccessorMockNotInDefinedRegion.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorNotEquals.Settings = RegionFilter.BindParameters(featureContextOperatorNotEquals.Parameters);
            var featureFlagStatus = await RegionFilter.EvaluateAsync(featureContextOperatorNotEquals);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_NotEquals_Operator()
        {
            RegionFilter RegionFilter = new RegionFilter(configMock.Object, httpContextAccessorMockInDefinedRegion.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorNotEquals.Settings = RegionFilter.BindParameters(featureContextOperatorNotEquals.Parameters);
            var featureFlagStatus = await RegionFilter.EvaluateAsync(featureContextOperatorNotEquals);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_In_Operator()
        {
            RegionFilter RegionFilter = new RegionFilter(configMock.Object, httpContextAccessorMockInDefinedRegion.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorIn.Settings = RegionFilter.BindParameters(featureContextOperatorIn.Parameters);
            var featureFlagStatus = await RegionFilter.EvaluateAsync(featureContextOperatorIn);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_In_Operator()
        {
            RegionFilter RegionFilter = new RegionFilter(configMock.Object, httpContextAccessorMockNotInDefinedRegion.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorIn.Settings = RegionFilter.BindParameters(featureContextOperatorIn.Parameters);
            var featureFlagStatus = await RegionFilter.EvaluateAsync(featureContextOperatorIn);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_NotIn_Operator()
        {
            RegionFilter RegionFilter = new RegionFilter(configMock.Object, httpContextAccessorMockNotInDefinedRegion.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorNotIn.Settings = RegionFilter.BindParameters(featureContextOperatorNotIn.Parameters);
            var featureFlagStatus = await RegionFilter.EvaluateAsync(featureContextOperatorNotIn);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_NotIn_Operator()
        {
            RegionFilter RegionFilter = new RegionFilter(configMock.Object, httpContextAccessorMockInDefinedRegion.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorNotIn.Settings = RegionFilter.BindParameters(featureContextOperatorNotIn.Parameters);
            var featureFlagStatus = await RegionFilter.EvaluateAsync(featureContextOperatorNotIn);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_HttpContext_Has_No_Region()
        {
            RegionFilter RegionFilter = new RegionFilter(configMock.Object, httpContextAccessorMockWithoutRegion.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorIn.Settings = RegionFilter.BindParameters(featureContextOperatorIn.Parameters);
            var featureFlagStatus = await RegionFilter.EvaluateAsync(featureContextOperatorIn);
            Assert.AreEqual(false, featureFlagStatus);
        }

        private Mock<IHttpContextAccessor> SetupHttpContextAccessorMock(Mock<IHttpContextAccessor> httpContextAccessorMock, bool hasRegion, string region)
        {
            httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            Dictionary<string, string> contextParams = new Dictionary<string, string>();
            if (hasRegion)
                contextParams.Add("region", region);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.Flighting.FLIGHT_CONTEXT_HEADER] = JsonConvert.SerializeObject(contextParams);
            httpContext.Items[Constants.Flighting.FLIGHT_TRACKER_PARAM] = JsonConvert.SerializeObject(new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            });
            httpContextAccessorMock.Setup(_ => _.HttpContext).Returns(httpContext);

            return httpContextAccessorMock;
        }

        private FeatureFilterEvaluationContext SetFilterContext(FeatureFilterEvaluationContext context, Operator filterOperator)
        {
            Dictionary<string, string> filterSettings = new Dictionary<string, string>
            {
                { "IsActive", "true" },
                { "StageId", "1" },
                { "Value", region }
            };

            switch (filterOperator)
            {
                case Operator.Equals:
                    filterSettings.Add("Operator", nameof(Operator.Equals));
                    break;
                case Operator.NotEquals:
                    filterSettings.Add("Operator", nameof(Operator.NotEquals));
                    break;
                case Operator.In:
                    filterSettings.Add("Operator", nameof(Operator.In));
                    break;
                case Operator.NotIn:
                    filterSettings.Add("Operator", nameof(Operator.NotIn));
                    break;
                default:
                    filterSettings.Add("Operator", nameof(Operator.Equals));
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
