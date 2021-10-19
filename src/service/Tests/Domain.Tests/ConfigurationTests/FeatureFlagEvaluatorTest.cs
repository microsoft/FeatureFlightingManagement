using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;
using AppInsights.EnterpriseTelemetry;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Domain.Tests.ConfigurationTests
{
    [TestClass]
    [TestCategory("FeatureFlagEvaluatorTests")]
    public class FeatureFlagEvaluatorTest
    {
        private  Mock<IFeatureManager> _featureManagerWithEnabledFlag, _featureManagerWithoutEnabledFlag;
        private  Mock<IHttpContextAccessor> _httpContextAccessor;
        private  Mock<IBackwardCompatibleFeatureManager> _backwardCompatibleFeatureManagerWithEnabledFlag;
        private  IConfiguration _configuration;
        private  Mock<ILogger> _logger;
        string application;
        string env;
        List<string> featureFlags;

        [TestInitialize]
        public void TestStartup()
        {
            
             application = "fxp";
             env = "Tenv";
            featureFlags = new List<string> { "TestFlag1", "TestFlag2" };
            setFeatureManager();
            sethttpContextAccessor(env,application);
            setBackwardCompatibleFeatureManager();
            setConfiguration();
            setLogger();
        }

        private void setLogger()
        {
            _logger = new Mock<ILogger>();
            _logger.Setup(m => m.Log(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            _logger.Setup(m => m.Log(It.IsAny<System.Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            _logger.Setup(m => m.Log(It.IsAny<ExceptionContext>()));
            _logger.Setup(m => m.Log(It.IsAny<MessageContext>()));
            _logger.Setup(m => m.Log(It.IsAny<EventContext>()));
            _logger.Setup(m => m.Log(It.IsAny<MetricContext>()));
        }

        private void setConfiguration()
        {
            
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
           
            keyValuePairs.Add("BackwardCompatibleFlags:ReverseTenantMapping:FXP", "FIELD EXPERIENCE (FXP)");

             _configuration = new ConfigurationBuilder()
               .AddInMemoryCollection(keyValuePairs)
               .Build();

           
           
            
        }

        private void setBackwardCompatibleFeatureManager()
        {
            _backwardCompatibleFeatureManagerWithEnabledFlag = new Mock<IBackwardCompatibleFeatureManager>();
            _backwardCompatibleFeatureManagerWithEnabledFlag.Setup(x => x.IsBackwardCompatibityRequired(It.IsAny<string>(), It.IsAny<string>(), "TestFlag1"))
                .Returns(true);
            _backwardCompatibleFeatureManagerWithEnabledFlag.Setup(x => x.IsBackwardCompatibityRequired(It.IsAny<string>(), It.IsAny<string>(), "TestFlag2"))
                .Returns(false);
            Dictionary<string, bool> response = new Dictionary<string, bool>();
            response.Add("TestFlag1", true);
            _backwardCompatibleFeatureManagerWithEnabledFlag.Setup(x => x.IsEnabledAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).
                Returns(Task.FromResult(response));
        }

        private void sethttpContextAccessor(string env,string app)
        {
           _httpContextAccessor = new Mock<IHttpContextAccessor>();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add(Constants.Flighting.FLIGHT_CONTEXT_HEADER, "testcontext");
            httpContext.Request.Headers.Add(Constants.Flighting.FEATURE_NAME_PARAM, "TestFlag2");
            httpContext.Request.Headers.Add(Constants.Flighting.FEATURE_ENV_PARAM, env);
            httpContext.Request.Headers.Add(Constants.Flighting.FEATURE_APP_PARAM, app);

            _httpContextAccessor.Setup(m => m.HttpContext).Returns(httpContext);
           
        }

        private void setFeatureManager()
        {
            _featureManagerWithEnabledFlag = new Mock<IFeatureManager>();
            _featureManagerWithoutEnabledFlag = new Mock<IFeatureManager>();
            _featureManagerWithEnabledFlag.Setup(x => x.IsEnabledAsync(It.IsAny<string>())).Returns(Task.FromResult(true));
            _featureManagerWithoutEnabledFlag.Setup(x => x.IsEnabledAsync(It.IsAny<string>())).Returns(Task.FromResult(false));
                                       

        }
        [TestMethod]
        public async Task evaluate_returns_correct_response_for_correct_data()
        {
            //Act
            FeatureFlagEvaluator evaluator = new FeatureFlagEvaluator(_featureManagerWithEnabledFlag.Object, _backwardCompatibleFeatureManagerWithEnabledFlag.Object, _httpContextAccessor.Object, _configuration, _logger.Object);
            var result = await evaluator.Evaluate(application, env, featureFlags);
            //Assert
            Assert.AreEqual(result["TestFlag1"],true);
            Assert.AreEqual(result["TestFlag2"], true);
        }
        [TestMethod]
        public async Task evaluate_returns_correct_response_for_correct_data_with_one_not_EnabledFlag()
        {
            //Act
            FeatureFlagEvaluator evaluator = new FeatureFlagEvaluator(_featureManagerWithoutEnabledFlag.Object, _backwardCompatibleFeatureManagerWithEnabledFlag.Object, _httpContextAccessor.Object, _configuration, _logger.Object);
            var result = await evaluator.Evaluate(application, env, featureFlags);
            //Assert
            Assert.AreEqual(result["TestFlag1"], true);
            Assert.AreEqual(result["TestFlag2"], false);
        }
    }
}
