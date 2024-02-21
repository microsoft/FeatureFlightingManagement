using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Common.Tests.ConfigTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("FlightOptimizationConfiguration")]
    [TestClass]
    public class FlightOptimizationConfigurationTest
    {
        [TestMethod]
        public void MergeWithDefault_ShouldMergeCorrectly()
        {
            var config = new FlightOptimizationConfiguration { OptimizationRules = "rule1" };
            var defaultConfig = new FlightOptimizationConfiguration { OptimizationRules = "rule2" };

            config.MergeWithDefault(defaultConfig);

            Assert.AreEqual("rule1", config.OptimizationRules);
        }

        [TestMethod]
        public void MergeWithDefault_ShouldUseDefaultValues_WhenValuesAreNull()
        {
            var config = new FlightOptimizationConfiguration { OptimizationRules = null };
            var defaultConfig = new FlightOptimizationConfiguration { OptimizationRules = "rule2" };

            config.MergeWithDefault(defaultConfig);

            Assert.AreEqual("rule2", config.OptimizationRules);
        }

        [TestMethod]
        public void GetDefault_ShouldReturnDefaultConfiguration()
        {
            var defaultConfig = FlightOptimizationConfiguration.GetDefault();

            Assert.IsFalse(defaultConfig.EnableOptimization);
            Assert.AreEqual(string.Empty, defaultConfig.OptimizationRules);
        }
    }
}
