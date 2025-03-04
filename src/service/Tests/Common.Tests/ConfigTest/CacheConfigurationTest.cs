using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Common.Tests.ConfigTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory(" CacheConfiguration")]
    [TestClass]
    public class CacheConfigurationTest
    {
        [DataTestMethod]
        [DataRow("FeatureFlags", "FeatureFlagsCache")]
        [DataRow("FeatureFlagNames", "FeatureFlagNamesCache")]
        [DataRow("Graph", "GraphCache")]
        [DataRow("RulesEngine", "RulesEngineCache")]
        [DataRow("OperatorMapping", "OperatorMappingCache")]
        [DataRow("InvalidOperation", "DefaultCache")]
        public void GetCacheType_ShouldReturnCorrectCacheType(string operation, string expected)
        {
            var config = new CacheConfiguration
            {
                Type = "DefaultCache",
                FeatureFlags = "FeatureFlagsCache",
                FeatureFlagNames = "FeatureFlagNamesCache",
                Graph = "GraphCache",
                RulesEngine = "RulesEngineCache",
                OperatorMapping = "OperatorMappingCache"
            };

            var result = config.GetCacheType(operation);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void MergeWithDefault_ShouldMergeCorrectly()
        {
            var config = new CacheConfiguration { Type = "type1", FeatureFlags = "flags1", Graph = "graph1" };
            var defaultConfig = new CacheConfiguration { Type = "type2", FeatureFlags = "flags2", Graph = "graph2" };

            config.MergeWithDefault(defaultConfig);

            Assert.AreEqual("type1", config.Type);
            Assert.AreEqual("flags1", config.FeatureFlags);
            Assert.AreEqual("graph1", config.Graph);
        }

        [TestMethod]
        public void MergeWithDefault_ShouldUseDefaultValues_WhenValuesAreNull()
        {
            var config = new CacheConfiguration { Type = null, FeatureFlags = null, Graph = null };
            var defaultConfig = new CacheConfiguration { Type = "type2", FeatureFlags = "flags2", Graph = "graph2" };

            config.MergeWithDefault(defaultConfig);

            Assert.AreEqual("type2", config.Type);
            Assert.AreEqual("flags2", config.FeatureFlags);
            Assert.AreEqual("graph2", config.Graph);
        }
    }
}
