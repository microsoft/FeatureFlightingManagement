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
    [TestCategory("UserUpnFilter")]
    [TestClass]
    public class UserUpnFilterTests: InitializeFilterTests
    {
        private Mock<IHttpContextAccessor> httpContextAccessorMockWithoutUserUpn;
        private Mock<IHttpContextAccessor> httpContextAccessorMockInDefinedUserUpn;
        private Mock<IHttpContextAccessor> httpContextAccessorMockNotInDefinedUserUpn;
        private Mock<IHttpContextAccessor> httpContextAccessorMockWithUpnMemberOfGroup;
        private Mock<IHttpContextAccessor> httpContextAccessorMockWithUpnNotMemberOfGroup;
        private FeatureFilterEvaluationContext featureContextOperatorIn;
        private FeatureFilterEvaluationContext featureContextOperatorNotIn;
        private FeatureFilterEvaluationContext featureContextOperatorEquals;
        private FeatureFilterEvaluationContext featureContextOperatorNotEquals;
        private FeatureFilterEvaluationContext featureContextOperatorMemberOfSecurityGroup;
        private FeatureFilterEvaluationContext featureContextOperatorNotMemberOfSecurityGroup;
        private Mock<ILogger> loggerMock;
        private Mock<IConfiguration> configMock;
        private readonly string emailId = "test123@microsoft.com";

        [TestInitialize]
        public void TestStartup()
        {
            successfullMockEvaluatorStrategy = SetupMockOperatorEvaluatorStrategy(true);
            failureMockEvaluatorStrategy = SetupMockOperatorEvaluatorStrategy(false);

            httpContextAccessorMockWithoutUserUpn = SetupHttpContextAccessorMock(httpContextAccessorMockWithoutUserUpn, false, null);
            httpContextAccessorMockInDefinedUserUpn = SetupHttpContextAccessorMock(httpContextAccessorMockInDefinedUserUpn, true, "test123@microsoft.com");
            httpContextAccessorMockNotInDefinedUserUpn = SetupHttpContextAccessorMock(httpContextAccessorMockNotInDefinedUserUpn, true, "notTest123@microsoft.com");
            httpContextAccessorMockWithUpnMemberOfGroup = SetupHttpContextAccessorMock(httpContextAccessorMockWithUpnMemberOfGroup, true, "test123@microsoft.com");
            httpContextAccessorMockWithUpnNotMemberOfGroup = SetupHttpContextAccessorMock(httpContextAccessorMockWithUpnNotMemberOfGroup, true, "test1234@microsoft.com");

            featureContextOperatorIn = SetFilterContext(featureContextOperatorIn, Operator.In);
            featureContextOperatorNotIn = SetFilterContext(featureContextOperatorNotIn, Operator.NotIn);
            featureContextOperatorEquals = SetFilterContext(featureContextOperatorEquals, Operator.Equals);
            featureContextOperatorNotEquals = SetFilterContext(featureContextOperatorNotEquals, Operator.NotEquals);
            featureContextOperatorMemberOfSecurityGroup = SetFilterContext(featureContextOperatorMemberOfSecurityGroup, Operator.MemberOfSecurityGroup);
            featureContextOperatorNotMemberOfSecurityGroup = SetFilterContext(featureContextOperatorNotMemberOfSecurityGroup, Operator.NotMemberOfSecurityGroup);

            configMock = SetConfigMock(configMock);
            loggerMock = SetLoggerMock(loggerMock);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_Equals_Operator()
        {
            UserUpnFilter UserUpnFilter = new UserUpnFilter(configMock.Object, httpContextAccessorMockInDefinedUserUpn.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorEquals.Settings = UserUpnFilter.BindParameters(featureContextOperatorEquals.Parameters);
            var featureFlagStatus = await UserUpnFilter.EvaluateAsync(featureContextOperatorEquals);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_Equals_Operator()
        {
            UserUpnFilter UserUpnFilter = new UserUpnFilter(configMock.Object, httpContextAccessorMockNotInDefinedUserUpn.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorEquals.Settings = UserUpnFilter.BindParameters(featureContextOperatorEquals.Parameters);
            var featureFlagStatus = await UserUpnFilter.EvaluateAsync(featureContextOperatorEquals);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_NotEquals_Operator()
        {
            UserUpnFilter UserUpnFilter = new UserUpnFilter(configMock.Object, httpContextAccessorMockNotInDefinedUserUpn.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorNotEquals.Settings = UserUpnFilter.BindParameters(featureContextOperatorNotEquals.Parameters);
            var featureFlagStatus = await UserUpnFilter.EvaluateAsync(featureContextOperatorNotEquals);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_NotEquals_Operator()
        {
            UserUpnFilter UserUpnFilter = new UserUpnFilter(configMock.Object, httpContextAccessorMockInDefinedUserUpn.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorNotEquals.Settings = UserUpnFilter.BindParameters(featureContextOperatorNotEquals.Parameters);
            var featureFlagStatus = await UserUpnFilter.EvaluateAsync(featureContextOperatorNotEquals);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_In_Operator()
        {
            UserUpnFilter UserUpnFilter = new UserUpnFilter(configMock.Object, httpContextAccessorMockInDefinedUserUpn.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorIn.Settings = UserUpnFilter.BindParameters(featureContextOperatorIn.Parameters);
            var featureFlagStatus = await UserUpnFilter.EvaluateAsync(featureContextOperatorIn);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_In_Operator()
        {
            UserUpnFilter UserUpnFilter = new UserUpnFilter(configMock.Object, httpContextAccessorMockNotInDefinedUserUpn.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorIn.Settings = UserUpnFilter.BindParameters(featureContextOperatorIn.Parameters);
            var featureFlagStatus = await UserUpnFilter.EvaluateAsync(featureContextOperatorIn);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_NotIn_Operator()
        {
            UserUpnFilter UserUpnFilter = new UserUpnFilter(configMock.Object, httpContextAccessorMockNotInDefinedUserUpn.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorNotIn.Settings = UserUpnFilter.BindParameters(featureContextOperatorNotIn.Parameters);
            var featureFlagStatus = await UserUpnFilter.EvaluateAsync(featureContextOperatorNotIn);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_NotIn_Operator()
        {
            UserUpnFilter UserUpnFilter = new UserUpnFilter(configMock.Object, httpContextAccessorMockInDefinedUserUpn.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorNotIn.Settings = UserUpnFilter.BindParameters(featureContextOperatorNotIn.Parameters);
            var featureFlagStatus = await UserUpnFilter.EvaluateAsync(featureContextOperatorNotIn);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_HttpContext_Has_No_UserUpn()
        {
            UserUpnFilter UserUpnFilter = new UserUpnFilter(configMock.Object, httpContextAccessorMockWithoutUserUpn.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorIn.Settings = UserUpnFilter.BindParameters(featureContextOperatorIn.Parameters);
            var featureFlagStatus = await UserUpnFilter.EvaluateAsync(featureContextOperatorIn);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_Member_Of_Saved_Group_Operator()
        {
            UserUpnFilter userupnFilter = new UserUpnFilter(configMock.Object, httpContextAccessorMockWithUpnMemberOfGroup.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorMemberOfSecurityGroup.Settings = userupnFilter.BindParameters(featureContextOperatorMemberOfSecurityGroup.Parameters);
            var featureFlagStatus = await userupnFilter.EvaluateAsync(featureContextOperatorMemberOfSecurityGroup);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_Member_Of_Saved_Group_Operator()
        {
            UserUpnFilter userupnFilter = new UserUpnFilter(configMock.Object, httpContextAccessorMockWithUpnNotMemberOfGroup.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorMemberOfSecurityGroup.Settings = userupnFilter.BindParameters(featureContextOperatorMemberOfSecurityGroup.Parameters);
            var featureFlagStatus = await userupnFilter.EvaluateAsync(featureContextOperatorMemberOfSecurityGroup);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_Not_Member_Of_Saved_Group_Operator()
        {
            UserUpnFilter userupnFilter = new UserUpnFilter(configMock.Object, httpContextAccessorMockWithUpnMemberOfGroup.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorNotMemberOfSecurityGroup.Settings = userupnFilter.BindParameters(featureContextOperatorNotMemberOfSecurityGroup.Parameters);
            var featureFlagStatus = await userupnFilter.EvaluateAsync(featureContextOperatorNotMemberOfSecurityGroup);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_Not_Member_Of_Saved_Group_Operator()
        {
            UserUpnFilter userupnFilter = new UserUpnFilter(configMock.Object, httpContextAccessorMockWithUpnNotMemberOfGroup.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorNotMemberOfSecurityGroup.Settings = userupnFilter.BindParameters(featureContextOperatorNotMemberOfSecurityGroup.Parameters);
            var featureFlagStatus = await userupnFilter.EvaluateAsync(featureContextOperatorNotMemberOfSecurityGroup);
            Assert.AreEqual(false, featureFlagStatus);
        }

        private Mock<IHttpContextAccessor> SetupHttpContextAccessorMock(Mock<IHttpContextAccessor> httpContextAccessorMock, bool hasUserUpn, string upn)
        {
            httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            Dictionary<string, string> contextParams = new Dictionary<string, string>();
            if (hasUserUpn)
                contextParams.Add("upn", upn);

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
                { "Value", emailId }
            };
            string oidString;
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
                case Operator.MemberOfSecurityGroup:
                    filterSettings.Add("Operator", nameof(Operator.MemberOfSecurityGroup));
                    filterSettings.Remove("Value");
                    oidString = GetGroupOidJsonString();
                    filterSettings.Add("Value", oidString);
                    break;
                case Operator.NotMemberOfSecurityGroup:
                    filterSettings.Add("Operator", nameof(Operator.NotMemberOfSecurityGroup));
                    filterSettings.Remove("Value");
                    oidString = GetGroupOidJsonString();
                    filterSettings.Add("Value", oidString);
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

        private string GetGroupOidJsonString()
        {
            return "[{\"Name\":\"Group1\",\"ObjectId\":\"Oid1\"},{\"Name\":\"Group2\",\"ObjectId\":\"Oid2\"}]";
        }
    }
}
