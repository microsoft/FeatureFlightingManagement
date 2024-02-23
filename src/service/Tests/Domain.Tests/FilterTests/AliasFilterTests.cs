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
    [TestCategory("AliasFilter")]
    [TestClass]
    public class AliasFilterTests: InitializeFilterTests
    {
        private Mock<IHttpContextAccessor> httpContextAccessorMockWithAliasMemberOfGroup;
        private Mock<IHttpContextAccessor> httpContextAccessorMockWithAliasNotMemberOfGroup;
        private Mock<IHttpContextAccessor> httpContextAccessorMockWithoutAlias;

        private FeatureFilterEvaluationContext featureContextOperatorMemberOfCustomGroup;
        private FeatureFilterEvaluationContext featureContextOperatorNotMemberOfCustomGroup;
        private FeatureFilterEvaluationContext featureContextOperatorEquals;
        private FeatureFilterEvaluationContext featureContextOperatorNotEquals;
        
        private Mock<ILogger> loggerMock;
        private Mock<IConfiguration> configMock;
        private readonly string customGroup = "testUser1";

        [TestInitialize]
        public void TestStartup()
        {
            successfullMockEvaluatorStrategy = SetupMockOperatorEvaluatorStrategy(true);
            failureMockEvaluatorStrategy = SetupMockOperatorEvaluatorStrategy(false);

            httpContextAccessorMockWithAliasMemberOfGroup = SetupHttpContextAccessorMockAlias(httpContextAccessorMockWithAliasMemberOfGroup, true, "testUser1");
            httpContextAccessorMockWithAliasNotMemberOfGroup = SetupHttpContextAccessorMockAlias(httpContextAccessorMockWithAliasNotMemberOfGroup, true, "testUser3");
            httpContextAccessorMockWithoutAlias = SetupHttpContextAccessorMockAlias(httpContextAccessorMockWithoutAlias, false, null);

            featureContextOperatorMemberOfCustomGroup = SetFilterContext(featureContextOperatorMemberOfCustomGroup, Operator.MemberOfCustomGroup);
            featureContextOperatorNotMemberOfCustomGroup = SetFilterContext(featureContextOperatorNotMemberOfCustomGroup, Operator.NotMemberOfCustomGroup);
            featureContextOperatorEquals = SetFilterContext(featureContextOperatorEquals, Operator.Equals);
            featureContextOperatorNotEquals = SetFilterContext(featureContextOperatorNotEquals, Operator.NotEquals);
            configMock = SetConfigMock(configMock);
            loggerMock = SetLoggerMock(loggerMock);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_Equals_Operator()
        {
            AliasFilter aliasFilter = new AliasFilter(configMock.Object, httpContextAccessorMockWithAliasMemberOfGroup.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorEquals.Settings = aliasFilter.BindParameters(featureContextOperatorEquals.Parameters);
            var featureFlagStatus = await aliasFilter.EvaluateAsync(featureContextOperatorEquals);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_Equals_Operator()
        {
            AliasFilter aliasFilter = new AliasFilter(configMock.Object, httpContextAccessorMockWithAliasNotMemberOfGroup.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorEquals.Settings = aliasFilter.BindParameters(featureContextOperatorEquals.Parameters);
            var featureFlagStatus = await aliasFilter.EvaluateAsync(featureContextOperatorEquals);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_NotEquals_Operator()
        {
            AliasFilter aliasFilter = new AliasFilter(configMock.Object, httpContextAccessorMockWithAliasNotMemberOfGroup.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorNotEquals.Settings = aliasFilter.BindParameters(featureContextOperatorNotEquals.Parameters);
            var featureFlagStatus = await aliasFilter.EvaluateAsync(featureContextOperatorNotEquals);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_NotEquals_Operator()
        {
            AliasFilter aliasFilter = new AliasFilter(configMock.Object, httpContextAccessorMockWithAliasMemberOfGroup.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorNotEquals.Settings = aliasFilter.BindParameters(featureContextOperatorNotEquals.Parameters);
            var featureFlagStatus = await aliasFilter.EvaluateAsync(featureContextOperatorNotEquals);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_Member_Of_Custom_Group_Operator()
        {
            AliasFilter aliasFilter = new AliasFilter(configMock.Object, httpContextAccessorMockWithAliasMemberOfGroup.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorMemberOfCustomGroup.Settings = aliasFilter.BindParameters(featureContextOperatorMemberOfCustomGroup.Parameters);
            var featureFlagStatus = await aliasFilter.EvaluateAsync(featureContextOperatorMemberOfCustomGroup);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_Member_Of_Custom_Group_Operator()
        {
            AliasFilter aliasFilter = new AliasFilter(configMock.Object, httpContextAccessorMockWithAliasNotMemberOfGroup.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorMemberOfCustomGroup.Settings = aliasFilter.BindParameters(featureContextOperatorMemberOfCustomGroup.Parameters);
            var featureFlagStatus = await aliasFilter.EvaluateAsync(featureContextOperatorMemberOfCustomGroup);
            Assert.AreEqual(false, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_True_If_Succeeds_Not_Member_Of_Custom_Group_Operator()
        {
            AliasFilter aliasFilter = new AliasFilter(configMock.Object, httpContextAccessorMockWithAliasNotMemberOfGroup.Object, loggerMock.Object, successfullMockEvaluatorStrategy.Object);
            featureContextOperatorNotMemberOfCustomGroup.Settings = aliasFilter.BindParameters(featureContextOperatorNotMemberOfCustomGroup.Parameters);
            var featureFlagStatus = await aliasFilter.EvaluateAsync(featureContextOperatorNotMemberOfCustomGroup);
            Assert.AreEqual(true, featureFlagStatus);
        }

        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_Fails_Not_Member_Of_Custom_Group_Operator()
        {
            AliasFilter aliasFilter = new AliasFilter(configMock.Object, httpContextAccessorMockWithAliasMemberOfGroup.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorNotMemberOfCustomGroup.Settings = aliasFilter.BindParameters(featureContextOperatorNotMemberOfCustomGroup.Parameters);
            var featureFlagStatus = await aliasFilter.EvaluateAsync(featureContextOperatorNotMemberOfCustomGroup);
            Assert.AreEqual(false, featureFlagStatus);
        }



        [TestMethod]
        public async Task Feature_Filter_Must_Evaluate_To_False_If_HttpContext_Has_No_User()
        {
            AliasFilter aliasFilter = new AliasFilter(configMock.Object, httpContextAccessorMockWithoutAlias.Object, loggerMock.Object, failureMockEvaluatorStrategy.Object);
            featureContextOperatorNotMemberOfCustomGroup.Settings = aliasFilter.BindParameters(featureContextOperatorNotMemberOfCustomGroup.Parameters);
            var featureFlagStatus = await aliasFilter.EvaluateAsync(featureContextOperatorNotMemberOfCustomGroup);
            Assert.AreEqual(false, featureFlagStatus);
        }

        private Mock<IHttpContextAccessor> SetupHttpContextAccessorMockAlias(Mock<IHttpContextAccessor> httpContextAccessorMock, bool hasAlias, string alias)
        {
            httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            Dictionary<string, string> contextParams = new Dictionary<string, string>();
            if (hasAlias)
            {
                contextParams.Add("alias", alias);
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

        private FeatureFilterEvaluationContext SetFilterContext(FeatureFilterEvaluationContext context, Operator filterOperator)
        {
            Dictionary<string, string> filterSettings = new Dictionary<string, string>();
            filterSettings.Add("IsActive", "true");
            filterSettings.Add("StageId", "1");
            filterSettings.Add("Value", customGroup);

            string oidString = string.Empty;

            switch (filterOperator)
            {
                case Operator.MemberOfCustomGroup:
                    filterSettings.Add("Operator", nameof(Operator.MemberOfCustomGroup));
                    break;
                case Operator.NotMemberOfCustomGroup:
                    filterSettings.Add("Operator", nameof(Operator.NotMemberOfCustomGroup));
                    break;
                case Operator.Equals:
                    filterSettings.Add("Operator", nameof(Operator.Equals));
                    break;
                case Operator.NotEquals:
                    filterSettings.Add("Operator", nameof(Operator.NotEquals));
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

            context = new FeatureFilterEvaluationContext();
            context.Parameters = configuration;
            return context;
        }

        private string GetGroupOidJsonString()
        {
            return "[{\"Name\":\"Group1\",\"ObjectId\":\"Oid1\"},{\"Name\":\"Group2\",\"ObjectId\":\"Oid2\"}]";
        }
    }
}
