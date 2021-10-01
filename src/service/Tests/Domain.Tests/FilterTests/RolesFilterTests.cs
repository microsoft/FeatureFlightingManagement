using Moq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.PS.FlightingService.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.PS.FlightingService.Domain.FeatureFilters;

namespace Microsoft.PS.FlightingService.Domain.Tests.FilterTests
{
    [TestCategory("RoleFilter")]
    [TestClass]
    public class RolesFilterTests:InitializeFilterTests
    {
        private Mock<IHttpContextAccessor> httpContextAccessorMockWithoutRoles;
        private Mock<IHttpContextAccessor> httpContextAccessorMockInDefinedRoles;
        private Mock<IHttpContextAccessor> httpContextAccessorMockNotInDefinedRoles;
        private FeatureFilterEvaluationContext featureContextOperatorIn;
        private FeatureFilterEvaluationContext featureContextOperatorNotIn;
        private FeatureFilterEvaluationContext featureContextOperatorEquals;
        private FeatureFilterEvaluationContext featureContextOperatorNotEquals;
        private Mock<ILogger> loggerMock;
        private Mock<IConfiguration> configMock;
        private readonly string roles = "Admin";

        [TestInitialize]
        public void TestStartup()
        {
            successfullMockEvaluatorStrategy = SetupMockOperatorEvaluatorStrategy(true);
            failureMockEvaluatorStrategy = SetupMockOperatorEvaluatorStrategy(false);

            httpContextAccessorMockWithoutRoles = SetupHttpContextAccessorMock(httpContextAccessorMockWithoutRoles, false, null);
            httpContextAccessorMockInDefinedRoles = SetupHttpContextAccessorMock(httpContextAccessorMockInDefinedRoles, true, "Admin");
            httpContextAccessorMockNotInDefinedRoles = SetupHttpContextAccessorMock(httpContextAccessorMockNotInDefinedRoles, true, "notadMin");

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
            RoleFilter RolesFilter = new RoleFilter(configMock.Object, httpContextAccessorMockInDefinedRoles.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            var featureFlagStatus = await RolesFilter.EvaluateAsync(featureContextOperatorEquals);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_Equals_Operator()
        {
            RoleFilter RolesFilter = new RoleFilter(configMock.Object, httpContextAccessorMockNotInDefinedRoles.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            var featureFlagStatus = await RolesFilter.EvaluateAsync(featureContextOperatorEquals);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_NotEquals_Operator()
        {
            RoleFilter RolesFilter = new RoleFilter(configMock.Object, httpContextAccessorMockNotInDefinedRoles.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            var featureFlagStatus = await RolesFilter.EvaluateAsync(featureContextOperatorNotEquals);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_NotEquals_Operator()
        {
            RoleFilter RolesFilter = new RoleFilter(configMock.Object, httpContextAccessorMockInDefinedRoles.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            var featureFlagStatus = await RolesFilter.EvaluateAsync(featureContextOperatorNotEquals);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_In_Operator()
        {
            RoleFilter RolesFilter = new RoleFilter(configMock.Object, httpContextAccessorMockInDefinedRoles.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            var featureFlagStatus = await RolesFilter.EvaluateAsync(featureContextOperatorIn);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_In_Operator()
        {
            RoleFilter RolesFilter = new RoleFilter(configMock.Object, httpContextAccessorMockNotInDefinedRoles.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            var featureFlagStatus = await RolesFilter.EvaluateAsync(featureContextOperatorIn);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_NotIn_Operator()
        {
            RoleFilter RolesFilter = new RoleFilter(configMock.Object, httpContextAccessorMockNotInDefinedRoles.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            var featureFlagStatus = await RolesFilter.EvaluateAsync(featureContextOperatorNotIn);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_NotIn_Operator()
        {
            RoleFilter RolesFilter = new RoleFilter(configMock.Object, httpContextAccessorMockInDefinedRoles.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            var featureFlagStatus = await RolesFilter.EvaluateAsync(featureContextOperatorNotIn);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_HttpContext_Has_No_Roles()
        {
            RoleFilter RolesFilter = new RoleFilter(configMock.Object, httpContextAccessorMockWithoutRoles.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            var featureFlagStatus = await RolesFilter.EvaluateAsync(featureContextOperatorIn);
            Assert.AreEqual(false, featureFlagStatus);
        }

        private Mock<IHttpContextAccessor> SetupHttpContextAccessorMock(Mock<IHttpContextAccessor> httpContextAccessorMock, bool hasRole, string role)
        {
            httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            Dictionary<string, string> contextParams = new Dictionary<string, string>();
            if (hasRole)
                contextParams.Add("role", role);

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
                { "Value", roles }
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

            context = new FeatureFilterEvaluationContext();
            context.Parameters = configuration;
            return context;
        }
    }
}
