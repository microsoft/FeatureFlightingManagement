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
    [TestCategory("RoleGroupFilter")]
    [TestClass]
    public class RoleGroupFilterTests:InitializeFilterTests
    {
        private Mock<IHttpContextAccessor> httpContextAccessorMockWithoutRoleGroup;
        private Mock<IHttpContextAccessor> httpContextAccessorMockInDefinedRoleGroup;
        private Mock<IHttpContextAccessor> httpContextAccessorMockNotInDefinedRoleGroup;
        private FeatureFilterEvaluationContext featureContextOperatorIn;
        private FeatureFilterEvaluationContext featureContextOperatorNotIn;
        private FeatureFilterEvaluationContext featureContextOperatorEquals;
        private FeatureFilterEvaluationContext featureContextOperatorNotEquals;
        private Mock<ILogger> loggerMock;
        private Mock<IConfiguration> configMock;
        private readonly string roleGroups = "1";

        [TestInitialize]
        public void TestStartup()
        {
            successfullMockEvaluatorStrategy = SetupMockOperatorEvaluatorStrategy(true);
            failureMockEvaluatorStrategy = SetupMockOperatorEvaluatorStrategy(false);

            httpContextAccessorMockWithoutRoleGroup = SetupHttpContextAccessorMock(httpContextAccessorMockWithoutRoleGroup, false, null);
            httpContextAccessorMockInDefinedRoleGroup = SetupHttpContextAccessorMock(httpContextAccessorMockInDefinedRoleGroup, true, "1");
            httpContextAccessorMockNotInDefinedRoleGroup = SetupHttpContextAccessorMock(httpContextAccessorMockNotInDefinedRoleGroup, true, "3");

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
            RoleGroupFilter roleGroupFilter = new RoleGroupFilter(configMock.Object, httpContextAccessorMockInDefinedRoleGroup.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorEquals.Settings = roleGroupFilter.BindParameters(featureContextOperatorEquals.Parameters);
            var featureFlagStatus = await roleGroupFilter.EvaluateAsync(featureContextOperatorEquals);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_Equals_Operator()
        {
            RoleGroupFilter roleGroupFilter = new RoleGroupFilter(configMock.Object, httpContextAccessorMockNotInDefinedRoleGroup.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorEquals.Settings = roleGroupFilter.BindParameters(featureContextOperatorEquals.Parameters);
            var featureFlagStatus = await roleGroupFilter.EvaluateAsync(featureContextOperatorEquals);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_NotEquals_Operator()
        {
            RoleGroupFilter roleGroupFilter = new RoleGroupFilter(configMock.Object, httpContextAccessorMockNotInDefinedRoleGroup.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorNotEquals.Settings = roleGroupFilter.BindParameters(featureContextOperatorNotEquals.Parameters);
            var featureFlagStatus = await roleGroupFilter.EvaluateAsync(featureContextOperatorNotEquals);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_NotEquals_Operator()
        {
            RoleGroupFilter roleGroupFilter = new RoleGroupFilter(configMock.Object, httpContextAccessorMockInDefinedRoleGroup.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorNotEquals.Settings = roleGroupFilter.BindParameters(featureContextOperatorNotEquals.Parameters);
            var featureFlagStatus = await roleGroupFilter.EvaluateAsync(featureContextOperatorNotEquals);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_In_Operator()
        {
            RoleGroupFilter roleGroupFilter = new RoleGroupFilter(configMock.Object, httpContextAccessorMockInDefinedRoleGroup.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorIn.Settings = roleGroupFilter.BindParameters(featureContextOperatorIn.Parameters);
            var featureFlagStatus = await roleGroupFilter.EvaluateAsync(featureContextOperatorIn);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_In_Operator()
        {
            RoleGroupFilter roleGroupFilter = new RoleGroupFilter(configMock.Object, httpContextAccessorMockNotInDefinedRoleGroup.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorIn.Settings = roleGroupFilter.BindParameters(featureContextOperatorIn.Parameters);
            var featureFlagStatus = await roleGroupFilter.EvaluateAsync(featureContextOperatorIn);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_NotIn_Operator()
        {
            RoleGroupFilter roleGroupFilter = new RoleGroupFilter(configMock.Object, httpContextAccessorMockNotInDefinedRoleGroup.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorNotIn.Settings = roleGroupFilter.BindParameters(featureContextOperatorNotIn.Parameters);
            var featureFlagStatus = await roleGroupFilter.EvaluateAsync(featureContextOperatorNotIn);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_NotIn_Operator()
        {
            RoleGroupFilter roleGroupFilter = new RoleGroupFilter(configMock.Object, httpContextAccessorMockInDefinedRoleGroup.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorNotIn.Settings = roleGroupFilter.BindParameters(featureContextOperatorNotIn.Parameters);
            var featureFlagStatus = await roleGroupFilter.EvaluateAsync(featureContextOperatorNotIn);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_HttpContext_Has_No_RoleGroup()
        {
            RoleGroupFilter roleGroupFilter = new RoleGroupFilter(configMock.Object, httpContextAccessorMockWithoutRoleGroup.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorIn.Settings = roleGroupFilter.BindParameters(featureContextOperatorIn.Parameters);
            var featureFlagStatus = await roleGroupFilter.EvaluateAsync(featureContextOperatorIn);
            Assert.AreEqual(false, featureFlagStatus);
        }

        public Mock<IHttpContextAccessor> SetupHttpContextAccessorMock(Mock<IHttpContextAccessor> httpContextAccessorMock, bool hasRoleGroup, string roleGroup)
        {
            httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            Dictionary<string, string> contextParams = new Dictionary<string, string>();
            if (hasRoleGroup)
                contextParams.Add("rolegroup", roleGroup);

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
                { "Value", roleGroups }
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
