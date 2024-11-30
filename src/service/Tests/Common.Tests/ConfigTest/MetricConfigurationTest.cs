using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Common.Tests.ConfigTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("MetricConfiguration")]
    [TestClass]
    public class MetricConfigurationTest
    {
        [TestMethod]
        public void MergeWithDefault_ShouldCorrectlyMergeDefaultConfiguration()
        {
            // Arrange
            var defaultConfig = MetricConfiguration.GetDefault();
            var config = new MetricConfiguration
            {
                Enabled = true,
                AppInsightsName = null,
                TrackingEventName = null
            };

            // Act
            config.MergeWithDefault(defaultConfig);

            // Assert
            Assert.AreEqual(defaultConfig.AppInsightsName, config.AppInsightsName);
            Assert.AreEqual(defaultConfig.TrackingEventName, config.TrackingEventName);
        }
    }
}
