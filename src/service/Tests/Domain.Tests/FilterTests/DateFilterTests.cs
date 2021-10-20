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
    [TestCategory("DateFilter")]
    [TestClass]
    public class DateFilterTests : InitializeFilterTests
    {
        private Mock<IHttpContextAccessor> httpContextAccessorMock;
        private Mock<IHttpContextAccessor> httpContextAccessorMockWithoutcontext;
        private FeatureFilterEvaluationContext featureContextOperatorLessThanSuccess;
        private FeatureFilterEvaluationContext featureContextOperatorGreaterThanFail;
        private FeatureFilterEvaluationContext featureContextOperatorLessThanFail;
        private FeatureFilterEvaluationContext featureContextOperatorGreaterThanSuccess;
        private Mock<ILogger> loggerMock;
        private Mock<IConfiguration> configMock;

        [TestInitialize]
        public void TestStartup()
        {
            successfullMockEvaluatorStrategy = SetupMockOperatorEvaluatorStrategy(true);
            failureMockEvaluatorStrategy = SetupMockOperatorEvaluatorStrategy(false);

            httpContextAccessorMock = SetupHttpContextAccessorMock(httpContextAccessorMock,true);
            httpContextAccessorMockWithoutcontext = SetupHttpContextAccessorMock(httpContextAccessorMock, false);
            featureContextOperatorLessThanSuccess = SetFilterContext(featureContextOperatorLessThanSuccess, Operator.LessThan, true);
            featureContextOperatorLessThanFail = SetFilterContext(featureContextOperatorLessThanFail, Operator.LessThan, false);
            featureContextOperatorGreaterThanSuccess = SetFilterContext(featureContextOperatorGreaterThanSuccess, Operator.GreaterThan, false);
            featureContextOperatorGreaterThanFail = SetFilterContext(featureContextOperatorGreaterThanFail, Operator.GreaterThan, true);
            configMock = SetConfigMock(configMock);
            loggerMock = SetLoggerMock(loggerMock);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_LessThan_Operator()
        {
            DateFilter dateFilter = new DateFilter(configMock.Object, httpContextAccessorMock.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            var featureFlagStatus = await dateFilter.EvaluateAsync(featureContextOperatorLessThanSuccess);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_LessThan_Operator()
        {
            DateFilter dateFilter = new DateFilter(configMock.Object, httpContextAccessorMock.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            var featureFlagStatus = await dateFilter.EvaluateAsync(featureContextOperatorLessThanFail);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_LessThan_Operator_WithFaultyInput()
        {
            Dictionary<string, string> filterSettings = new Dictionary<string, string>
            {
                { "FlightContextKey", "Date" },
                { "IsActive", "true" },
                { "StageId", "1" },
                { "Value", "testString" },
                { "Operator", nameof(Operator.LessThan) }
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(filterSettings)
                .Build();

            FeatureFilterEvaluationContext context = new FeatureFilterEvaluationContext
            {
                Parameters = configuration
            };

            DateFilter dateFilter = new DateFilter(configMock.Object, httpContextAccessorMock.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            var featureFlagStatus = await dateFilter.EvaluateAsync(context);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_GreaterThan_Operator()
        {
            DateFilter dateFilter = new DateFilter(configMock.Object, httpContextAccessorMock.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            var featureFlagStatus = await dateFilter.EvaluateAsync(featureContextOperatorGreaterThanSuccess);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_GreaterThan_Operator()
        {
            DateFilter dateFilter = new DateFilter(configMock.Object, httpContextAccessorMock.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            var featureFlagStatus = await dateFilter.EvaluateAsync(featureContextOperatorGreaterThanFail);
            Assert.AreEqual(false, featureFlagStatus);
        }
        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_GreaterThan_Operator_Without_context()
        {
            DateFilter dateFilter = new DateFilter(configMock.Object, httpContextAccessorMockWithoutcontext.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            var featureFlagStatus = await dateFilter.EvaluateAsync(featureContextOperatorGreaterThanSuccess);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_GreaterThan_Operator_Without_context()
        {
            DateFilter dateFilter = new DateFilter(configMock.Object, httpContextAccessorMockWithoutcontext.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            var featureFlagStatus = await dateFilter.EvaluateAsync(featureContextOperatorGreaterThanFail);
            Assert.AreEqual(false, featureFlagStatus);
        }
        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_GreaterThan_Operator_WithFaultyInput()
        {
            Dictionary<string, string> filterSettings = new Dictionary<string, string>
            {
                { "FlightContextKey", "Date" },
                { "IsActive", "true" },
                { "StageId", "1" },
                { "Value", "testString" },
                { "Operator", nameof(Operator.GreaterThan) }
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(filterSettings)
                .Build();

            FeatureFilterEvaluationContext context = new FeatureFilterEvaluationContext
            {
                Parameters = configuration
            };

            DateFilter dateFilter = new DateFilter(configMock.Object, httpContextAccessorMock.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            var featureFlagStatus = await dateFilter.EvaluateAsync(context);
            Assert.AreEqual(false, featureFlagStatus);
        }

        private Mock<IHttpContextAccessor> SetupHttpContextAccessorMock(Mock<IHttpContextAccessor> httpContextAccessorMock,bool hasDate)
        {
            httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            Dictionary<string, string> contextParams = new Dictionary<string, string> { };
            if(hasDate)
            {
                contextParams.Add("Date", "01/01/2060");
            }
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

        private FeatureFilterEvaluationContext SetFilterContext(FeatureFilterEvaluationContext context, Operator filterOperator, bool isAlwaysGreaterDate)
        {
            Dictionary<string, string> filterSettings = new Dictionary<string, string>
            {
                { "FlightContextKey", "Date" },
                { "IsActive", "true" },
                { "StageId", "1" }
            };
            if (isAlwaysGreaterDate)
                filterSettings.Add("Value", "01/01/2080");
            else
                filterSettings.Add("Value", "01/01/2000");

            switch (filterOperator)
            {
                case Operator.LessThan:
                    filterSettings.Add("Operator", nameof(Operator.LessThan));
                    break;
                case Operator.GreaterThan:
                    filterSettings.Add("Operator", nameof(Operator.GreaterThan));
                    break;
                default:
                    filterSettings.Add("Operator", nameof(Operator.LessThan));
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
