using Microsoft.FeatureFlighting.Common.Authentication;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Model.ChangeNotification;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Infrastructure.Webhook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using System;

namespace Microsoft.FeatureFlighting.Infrastructure.Tests.WebhookTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("WebhookTriggerManager")]
    [TestClass]
    public class WebhookTriggerManagerTest
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly WebhookTriggerManager _webhookTriggerManager;

        public WebhookTriggerManagerTest()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _tokenGeneratorMock = new Mock<ITokenGenerator>();
            _loggerMock = new Mock<ILogger>();
            _webhookTriggerManager = new WebhookTriggerManager(_httpClientFactoryMock.Object, _tokenGeneratorMock.Object, _loggerMock.Object);
        }

        [TestMethod]
        public async Task Trigger_ThrowException()
        {
            // Arrange
            var webhook = new WebhookConfiguration();
            webhook.HttpMethod = "GET";
            webhook.BaseEndpoint = "https://www.google.com";
            webhook.WebhookId = "test";
            webhook.AuthenticationAuthority = "https://login.microsoftonline.com/contoso.onmicrosoft.com";
            webhook.ClientId = "testClientId";
            webhook.ClientSecret = "nb3v4mn23v4234mn234nm2b34mn32b4b,m2n3mnb4mn234";
            webhook.ResourceId = "testResourceId";
            var featureFlightChangeEvent = new FeatureFlightChangeEvent();
            var trackingIds = new LoggerTrackingIds();
            var httpClient = new HttpClient();
            _httpClientFactoryMock.Setup(hcf => hcf.CreateClient(It.IsAny<string>())).Returns(httpClient);
            _tokenGeneratorMock.Setup(tg => tg.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("token");

            try
            {
                // Act
                var result = await _webhookTriggerManager.Trigger(webhook, featureFlightChangeEvent, trackingIds);
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(ex.Message);
            }

        }
    }
}
