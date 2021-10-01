
using Microsoft.Extensions.Configuration;
using AppInsights.EnterpriseTelemetry;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.PS.FlightingService.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.PS.FlightingService.Services.Tests
{
    [TestClass]
    public class CarbonFlightingServiceTests
    {
        private Mock<IAuthorizationService> _authService = new Mock<IAuthorizationService>();
        private Mock<IHttpClientFactory> _httpClientFactory = new Mock<IHttpClientFactory>();
        private Mock<IConfiguration> config = new Mock<IConfiguration>();
        private Mock<ILogger> logger = new Mock<ILogger>();

        [TestInitialize]
        public void TestStartUp()
        {
            setConfiguration();
            setAuthService();
            _httpClientFactory = setHttpClientFactory();
             setLogger();
        }

        private void setConfiguration()
        {
            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(a => a.Value).Returns("true");

            var configurationSectionFlags = new Mock<IConfigurationSection>();
            configurationSectionFlags.Setup(a => a.Value).Returns("enableTestString");

            var configurationSectionCarbonService = new Mock<IConfigurationSection>();
            configurationSectionCarbonService.Setup(a => a.Value).Returns("CarbonFlightingService");

            var configurationSectionCarbonResourceId = new Mock<IConfigurationSection>();
            configurationSectionCarbonResourceId.Setup(a => a.Value).Returns("CarbonFlightingServiceId");

            var configurationSectionAuth = new Mock<IConfigurationSection>();
            configurationSectionAuth.Setup(a => a.Value).Returns("authority");

            var configurationSectionAud = new Mock<IConfigurationSection>();
            configurationSectionAud.Setup(a => a.Value).Returns("aud");

            var configurationSectionAuthenticationSecret = new Mock<IConfigurationSection>();
            configurationSectionAuthenticationSecret.Setup(a => a.Value).Returns("secret");

            var configurationSectionCarbonRelativeUrl = new Mock<IConfigurationSection>();
            configurationSectionCarbonRelativeUrl.Setup(a => a.Value).Returns("FXP/dev/flighting?featureNames=TestFlag1");

            var configurationSectionTenantKey = new Mock<IConfigurationSection>();
            configurationSectionTenantKey.Setup(a => a.Value).Returns("FXP");

            config.Setup(a => a.GetSection("BackwardCompatibleFlags:Enabled")).Returns(configurationSection.Object);
            config.Setup(a => a.GetSection("BackwardCompatibleFlags:FIELD_EXPERIENCE_FXP_:DEV")).Returns(configurationSectionFlags.Object);
            config.Setup(a => a.GetSection("CarbonFlightingService:Name")).Returns(configurationSectionCarbonService.Object);
            config.Setup(a => a.GetSection("Authentication:Authority")).Returns(configurationSectionAuth.Object);
            config.Setup(a => a.GetSection("Authentication:Audience")).Returns(configurationSectionAud.Object);
            config.Setup(a => a.GetSection("AuthenticationSecret")).Returns(configurationSectionAuthenticationSecret.Object);
            config.Setup(a => a.GetSection("CarbonFlightingService:AadResourceId")).Returns(configurationSectionCarbonService.Object);
            config.Setup(a => a.GetSection("CarbonFlightingService:RelativeUrl")).Returns(configurationSectionCarbonRelativeUrl.Object);
            config.Setup(a => a.GetSection("BackwardCompatibleFlags:TenantMapping:FIELD_EXPERIENCE_FXP_")).Returns(configurationSectionTenantKey.Object);
        }
        

        private Mock<IHttpClientFactory> setHttpClientFactory()
        {
            var clientHandlerMock = new Mock<DelegatingHandler>();
            clientHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{'TestFlag1':'true'}"),
                })
                .Verifiable();
            clientHandlerMock.As<IDisposable>().Setup(s => s.Dispose());

            var client = new HttpClient(clientHandlerMock.Object);
            
            client.BaseAddress= new Uri("https://flight/api/v1/");
            var clientFactoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            clientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client).Verifiable();

            return clientFactoryMock;
        }

        private void setAuthService()
        {
            _authService.Setup(x => x.GetAuthenticationToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("TestToken");
        }

        private void setLogger()
        {
            logger.Setup(m => m.Log(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            logger.Setup(m => m.Log(It.IsAny<System.Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            logger.Setup(m => m.Log(It.IsAny<ExceptionContext>()));
            logger.Setup(m => m.Log(It.IsAny<MessageContext>()));
            logger.Setup(m => m.Log(It.IsAny<EventContext>()));
            logger.Setup(m => m.Log(It.IsAny<MetricContext>()));
        }

        [TestMethod]
        public void validate_IsBackwardCompatibityRequired_returns_correct_response()
        {
            //Arrange
            var service = new CarbonFlightingService(_authService.Object, _httpClientFactory.Object, config.Object, logger.Object);
            //Act
            bool result = service.IsBackwardCompatibityRequired("FIELD EXPERIENCE (FXP)", "dev", "enableTestString");
            //Assert
            Assert.AreEqual(result, true);
        }
        [TestMethod]
        public async Task validate_isEnabled_returns_correct_response()
        {
            //Arrange
            var service = new CarbonFlightingService(_authService.Object, _httpClientFactory.Object, config.Object, logger.Object);
            List<string> featureFlags = new List<string> { "enableTestString" };
            //Act
            var result = await service.IsEnabledAsync("FIELD EXPERIENCE (FXP)", "dev", featureFlags, "testFlightContext");
             //Assert
            Assert.AreEqual(result["TestFlag1"], true);
        }
    }
}
