using AppInsights.EnterpriseTelemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.AppExceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.API.Tests.ControllerTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("BaseController")]
    [TestClass]
    public class BaseControllerTest
    {

        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger> _mockLogger;
        private readonly BaseClassExposedToTest _baseController;
        public BaseControllerTest()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger>();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["x-application"] = "TestApp";
            httpContext.Request.Headers["x-environment"] = "preprop";
            httpContext.Request.Headers["x-correlationId"] = "TestCorrelationId";
            httpContext.Request.Headers["x-messageId"] = "TestMessageId";
            httpContext.Request.Headers["x-channel"] = "TestChannel";

            var testConfig = new Mock<IConfigurationSection>();
            testConfig.Setup(s => s.Value).Returns("preprop,prod");

            _mockConfiguration.Setup(c => c.GetSection("Env:Supported")).Returns(testConfig.Object);

            _baseController = new BaseClassExposedToTest(_mockConfiguration.Object, _mockLogger.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };
        }

        [TestMethod]
        public async Task GetHeaders_WhenCalledWithValidHeaders_ShouldReturnHeaders()
        {
            var result = _baseController.GetHeaders();

            Assert.AreEqual("TestApp", result.Item1);
            Assert.AreEqual("preprop", result.Item2);
            Assert.AreEqual("TestCorrelationId", result.Item3);
            Assert.AreEqual("TestMessageId", result.Item4);
            Assert.AreEqual("TestChannel", result.Item5);
        }

        [TestMethod]
        public void GetHeaders_WhenCalledWithMissingHeaders_ShouldThrowDomainException()
        {
            var httpContext = new DefaultHttpContext();

            var _baseClassExposedToTest = new BaseClassExposedToTest(_mockConfiguration.Object, _mockLogger.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            Assert.ThrowsException<DomainException>(() => _baseClassExposedToTest.GetHeaders());
        }

        [TestMethod]
        public void GetHeaders_WhenCalledWithUnsupportedEnvironment_ShouldThrowDomainException()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["x-application"] = "TestApp";
            httpContext.Request.Headers["x-environment"] = "UnsupportedEnv";
            httpContext.Request.Headers["x-correlationId"] = "TestCorrelationId";
            httpContext.Request.Headers["x-messageId"] = "TestMessageId";
            httpContext.Request.Headers["x-channel"] = "TestChannel";

            _mockConfiguration.Setup(c => c.GetSection("Env:Supported").Value).Returns("TestEnv1,TestEnv2");
            var _baseClassExposedToTest = new BaseClassExposedToTest(_mockConfiguration.Object, _mockLogger.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };
            
            Assert.ThrowsException<DomainException>(() => _baseClassExposedToTest.GetHeaders());
        }

    }
}
