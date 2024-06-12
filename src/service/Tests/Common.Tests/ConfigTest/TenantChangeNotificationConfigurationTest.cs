using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Common.Tests.ConfigTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory(" TenantChangeNotificationConfiguration")]
    [TestClass]
    public class TenantChangeNotificationConfigurationTest
    {
        [TestMethod]
        public void MergeOrDefault_ShouldMergeDefaultConfiguration_WhenIsSubscribedIsTrue()
        {
            // Arrange
            var defaultConfig = new TenantChangeNotificationConfiguration
            {
                SubscribedEvents = "DefaultEvents",
                Webhook = new WebhookConfiguration()
            };

            var config = new TenantChangeNotificationConfiguration
            {
                IsSubscribed = true,
                SubscribedEvents = null,
                Webhook = null
            };

            // Act
            config.MergeOrDefault(defaultConfig);

            // Assert
            Assert.AreEqual(defaultConfig.SubscribedEvents, config.SubscribedEvents);
            Assert.AreEqual(defaultConfig.Webhook, config.Webhook);
        }

        [TestMethod]
        public void MergeOrDefault_ShouldNotMergeDefaultConfiguration_WhenIsSubscribedIsFalse()
        {
            // Arrange
            var defaultConfig = new TenantChangeNotificationConfiguration
            {
                SubscribedEvents = "DefaultEvents",
                Webhook = new WebhookConfiguration()
            };

            var config = new TenantChangeNotificationConfiguration
            {
                IsSubscribed = false,
                SubscribedEvents = null,
                Webhook = null
            };

            // Act
            config.MergeOrDefault(defaultConfig);

            // Assert
            Assert.IsNull(config.SubscribedEvents);
            Assert.IsNull(config.Webhook);
        }
    }
}
