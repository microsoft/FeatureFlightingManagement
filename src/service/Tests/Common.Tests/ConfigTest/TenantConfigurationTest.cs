using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Common.Tests.ConfigTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory(" TenantConfiguration")]
    [TestClass]
    public class TenantConfigurationTest
    {
        [TestMethod]
        public void MergeWithDefault_ShouldCorrectlyMergeDefaultConfiguration()
        {
            // Arrange
            var defaultConfig = TenantConfiguration.GetDefault();
            var config = new TenantConfiguration
            {
                Authorization = null,
                Cache = null,
                FlightsDatabase = null,
                Optimization = null,
                ChangeNotificationSubscription = null,
                Evaluation = null,
                IntelligentAlerts = null,
                Metrics = null
            };

            // Act
            config.MergeWithDefault(defaultConfig);

            // Assert
            Assert.AreEqual(defaultConfig.Authorization, config.Authorization);
            Assert.AreEqual(defaultConfig.Cache, config.Cache);
            Assert.AreEqual(defaultConfig.FlightsDatabase, config.FlightsDatabase);
            Assert.AreEqual(defaultConfig.Optimization, config.Optimization);
            Assert.AreEqual(defaultConfig.ChangeNotificationSubscription, config.ChangeNotificationSubscription);
            Assert.AreEqual(defaultConfig.Evaluation, config.Evaluation);
            Assert.AreEqual(defaultConfig.IntelligentAlerts, config.IntelligentAlerts);
            Assert.AreEqual(defaultConfig.Metrics, config.Metrics);
        }
    }
}
