using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Common.Tests.ConfigTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("IntelligentAlertConfiguration")]
    [TestClass]
    public class IntelligentAlertConfigurationTest
    {
        [TestMethod]
        public void MergeWithDefault_ShouldCorrectlyMergeDefaultConfiguration()
        {
            // Arrange
            var defaultConfig = IntelligentAlertConfiguration.GetDefault();
            var config = new IntelligentAlertConfiguration
            {
                AlertEventName = null,
                AlertEmailSubject = null,
                AlertEmailTemplate = null
            };

            // Act
            config.MergeWithDefault(defaultConfig);

            // Assert
            Assert.AreEqual(defaultConfig.AlertEventName, config.AlertEventName);
            Assert.AreEqual(defaultConfig.AlertEmailSubject, config.AlertEmailSubject);
            Assert.AreEqual(defaultConfig.AlertEmailTemplate, config.AlertEmailTemplate);
        }
    }
}

