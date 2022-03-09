//using Moq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using System.Collections.Generic;
//using Microsoft.FeatureManagement;
//using AppInsights.EnterpriseTelemetry;
//using Microsoft.FeatureFlighting.Common;
//using Microsoft.FeatureFlighting.Common.Config;
//using AppInsights.EnterpriseTelemetry.Context;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace Microsoft.FeatureFlighting.Core.Tests.ConfigurationTests
//{
//    [TestClass]
//    [TestCategory("FeatureFlagEvaluatorTests")]
//    public class FeatureFlagEvaluatorTest
//    {
//        private Mock<IFeatureManager> _featureManagerWithEnabledFlag, _featureManagerWithoutEnabledFlag;
//        private Mock<IHttpContextAccessor> _httpContextAccessor;
//        private ITenantConfigurationProvider _tenantConfigurationProvider;
//        private Mock<ILogger> _logger;
//        string application;
//        string env;
//        List<string> featureFlags;

//        [TestInitialize]
//        public void TestStartup()
//        {
//            application = "fxp";
//            env = "Tenv";
//            featureFlags = new List<string> { "TestFlag1", "TestFlag2" };
//            SetFeatureManager();
//            SethttpContextAccessor(env, application);
//            SetConfiguration();
//            SetLogger();
//        }

//        private void SetLogger()
//        {
//            _logger = new Mock<ILogger>();
//            _logger.Setup(m => m.Log(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
//            _logger.Setup(m => m.Log(It.IsAny<System.Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
//            _logger.Setup(m => m.Log(It.IsAny<ExceptionContext>()));
//            _logger.Setup(m => m.Log(It.IsAny<MessageContext>()));
//            _logger.Setup(m => m.Log(It.IsAny<EventContext>()));
//            _logger.Setup(m => m.Log(It.IsAny<MetricContext>()));
//        }

//        private void SetConfiguration()
//        {
//            TenantConfiguration defaultTenantConfiguration = TenantConfiguration.GetDefault();
//            Mock<ITenantConfigurationProvider> mockProvider = new();
//            mockProvider.Setup(provider => provider.Get(It.IsAny<string>()))
//                .Returns(Task.FromResult(defaultTenantConfiguration));
//            _tenantConfigurationProvider = mockProvider.Object;
//        }

//        private void SethttpContextAccessor(string env, string app)
//        {
//            _httpContextAccessor = new Mock<IHttpContextAccessor>();
//            var httpContext = new DefaultHttpContext();
//            httpContext.Request.Headers.Add(Constants.Flighting.FLIGHT_CONTEXT_HEADER, "testcontext");
//            httpContext.Request.Headers.Add(Constants.Flighting.FEATURE_NAME_PARAM, "TestFlag2");
//            httpContext.Request.Headers.Add(Constants.Flighting.FEATURE_ENV_PARAM, env);
//            httpContext.Request.Headers.Add(Constants.Flighting.FEATURE_APP_PARAM, app);
//            _httpContextAccessor.Setup(m => m.HttpContext).Returns(httpContext);
//        }

//        private void SetFeatureManager()
//        {
//            _featureManagerWithEnabledFlag = new Mock<IFeatureManager>();
//            _featureManagerWithoutEnabledFlag = new Mock<IFeatureManager>();
//            _featureManagerWithEnabledFlag.Setup(x => x.IsEnabledAsync(It.IsAny<string>())).Returns(Task.FromResult(true));
//            _featureManagerWithoutEnabledFlag.Setup(x => x.IsEnabledAsync(It.IsAny<string>())).Returns(Task.FromResult(false));
//        }

//        [TestMethod]
//        public async Task Evaluate_returns_correct_response_for_correct_data()
//        {
//            //Act
//            FeatureFlagEvaluator evaluator = new(_featureManagerWithEnabledFlag.Object, _httpContextAccessor.Object, _tenantConfigurationProvider, _logger.Object);
//            var result = await evaluator.Evaluate(application, env, featureFlags);
//            //Assert
//            Assert.IsTrue(result["TestFlag1"]);
//            Assert.IsTrue(result["TestFlag2"]);
//        }

//        [TestMethod]
//        public async Task Evaluate_returns_correct_response_for_correct_data_with_one_not_EnabledFlag()
//        {
//            //Act
//            FeatureFlagEvaluator evaluator = new(_featureManagerWithoutEnabledFlag.Object, _httpContextAccessor.Object, _tenantConfigurationProvider, _logger.Object);
//            var result = await evaluator.Evaluate(application, env, featureFlags);
//            //Assert
//            Assert.IsFalse(result["TestFlag1"]);
//            Assert.IsFalse(result["TestFlag2"]);
//        }
//    }
//}
