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

namespace Microsoft.FeatureFlighting.Core.Tests.FilterTests
{
    [TestCategory("GenericFilter")]
    [TestClass]
    public class GenericFilterTests : InitializeFilterTests
    {
        private Mock<IHttpContextAccessor> httpContextAccessorMockWithoutGeneric;
        private Mock<IHttpContextAccessor> httpContextAccessorMockInDefinedGeneric;
        private Mock<IHttpContextAccessor> httpContextAccessorMockNotInDefinedGeneric;
        private FeatureFilterEvaluationContext featureContextOperatorIn;
        private FeatureFilterEvaluationContext featureContextOperatorNotIn;
        private FeatureFilterEvaluationContext featureContextOperatorEquals;
        private FeatureFilterEvaluationContext featureContextOperatorNotEquals;
        private FeatureFilterEvaluationContext featureContextOperatorWithoutActiveStage;
        private Mock<ILogger> loggerMock;
        private Mock<IConfiguration> configMock;
        private readonly string Generics = "1";

        [TestInitialize]
        public void TestStartup()
        {
            successfullMockEvaluatorStrategy = SetupMockOperatorEvaluatorStrategy(true);
            failureMockEvaluatorStrategy = SetupMockOperatorEvaluatorStrategy(false);

            httpContextAccessorMockWithoutGeneric = SetupHttpContextAccessorMock(httpContextAccessorMockWithoutGeneric, false, null);
            httpContextAccessorMockInDefinedGeneric = SetupHttpContextAccessorMock(httpContextAccessorMockInDefinedGeneric, true, "1");
            httpContextAccessorMockNotInDefinedGeneric = SetupHttpContextAccessorMock(httpContextAccessorMockNotInDefinedGeneric, true, "3");

            featureContextOperatorIn = SetFilterContext(featureContextOperatorIn, Operator.In,"true");
            featureContextOperatorNotIn = SetFilterContext(featureContextOperatorNotIn, Operator.NotIn, "true");
            featureContextOperatorEquals = SetFilterContext(featureContextOperatorEquals, Operator.Equals, "true");
            featureContextOperatorNotEquals = SetFilterContext(featureContextOperatorNotEquals, Operator.NotEquals, "true");
            featureContextOperatorWithoutActiveStage = SetFilterContext(featureContextOperatorNotEquals, Operator.Equals, "false");
            configMock = SetConfigMock(configMock);
            loggerMock = SetLoggerMock(loggerMock);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_Equals_Operator()
        {
            GenericFilter GenericFilter = new GenericFilter(configMock.Object, httpContextAccessorMockInDefinedGeneric.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            var featureFlagStatus = await GenericFilter.EvaluateAsync(featureContextOperatorEquals);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_Equals_Operator()
        {
            GenericFilter GenericFilter = new GenericFilter(configMock.Object, httpContextAccessorMockNotInDefinedGeneric.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            var featureFlagStatus = await GenericFilter.EvaluateAsync(featureContextOperatorEquals);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_NotEquals_Operator()
        {
            GenericFilter GenericFilter = new GenericFilter(configMock.Object, httpContextAccessorMockNotInDefinedGeneric.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            var featureFlagStatus = await GenericFilter.EvaluateAsync(featureContextOperatorNotEquals);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_NotEquals_Operator()
        {
            GenericFilter GenericFilter = new GenericFilter(configMock.Object, httpContextAccessorMockInDefinedGeneric.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            var featureFlagStatus = await GenericFilter.EvaluateAsync(featureContextOperatorNotEquals);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_In_Operator()
        {
            GenericFilter GenericFilter = new GenericFilter(configMock.Object, httpContextAccessorMockInDefinedGeneric.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            var featureFlagStatus = await GenericFilter.EvaluateAsync(featureContextOperatorIn);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_In_Operator()
        {
            GenericFilter GenericFilter = new GenericFilter(configMock.Object, httpContextAccessorMockNotInDefinedGeneric.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            var featureFlagStatus = await GenericFilter.EvaluateAsync(featureContextOperatorIn);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_NotIn_Operator()
        {
            GenericFilter GenericFilter = new GenericFilter(configMock.Object, httpContextAccessorMockNotInDefinedGeneric.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            var featureFlagStatus = await GenericFilter.EvaluateAsync(featureContextOperatorNotIn);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_NotIn_Operator()
        {
            GenericFilter GenericFilter = new GenericFilter(configMock.Object, httpContextAccessorMockInDefinedGeneric.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            var featureFlagStatus = await GenericFilter.EvaluateAsync(featureContextOperatorNotIn);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_HttpContext_Has_No_Generic()
        {
            GenericFilter GenericFilter = new GenericFilter(configMock.Object, httpContextAccessorMockWithoutGeneric.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            var featureFlagStatus = await GenericFilter.EvaluateAsync(featureContextOperatorIn);
            Assert.AreEqual(false, featureFlagStatus);
        }
        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_StageNotActive()
        {
            GenericFilter GenericFilter = new GenericFilter(configMock.Object, httpContextAccessorMockInDefinedGeneric.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            var featureFlagStatus = await GenericFilter.EvaluateAsync(featureContextOperatorWithoutActiveStage);
            Assert.AreEqual(false, featureFlagStatus);
        }
        public Mock<IHttpContextAccessor> SetupHttpContextAccessorMock(Mock<IHttpContextAccessor> httpContextAccessorMock, bool hasGeneric, string Generic)
        {
            httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            Dictionary<string, string> contextParams = new Dictionary<string, string>();
            if (hasGeneric)
                contextParams.Add("Generic", Generic);

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

        private FeatureFilterEvaluationContext SetFilterContext(FeatureFilterEvaluationContext context, Operator filterOperator,string IsActive)
        {
            Dictionary<string, string> filterSettings = new Dictionary<string, string>
            {
                { "IsActive", IsActive},
                { "StageId", "1" },
                { "Value", Generics },
                {"FlightContextKey","Generic" }
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
