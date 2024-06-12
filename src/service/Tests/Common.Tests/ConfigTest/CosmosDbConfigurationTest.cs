using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Common.Tests.ConfigTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory(" CosmosDbConfiguration")]
    [TestClass]
    public class CosmosDbConfigurationTest
    {
        [TestMethod]
        public void MergeWithDefault_ShouldMergeCorrectly()
        {
            var config = new CosmosDbConfiguration { Endpoint = "endpoint1", PrimaryKey = "key1", DatabaseId = "db1", ContainerId = "container1" };
            var defaultConfig = new CosmosDbConfiguration { Endpoint = "endpoint2", PrimaryKey = "key2", DatabaseId = "db2", ContainerId = "container2" };

            config.MergeWithDefault(defaultConfig);

            Assert.AreEqual("endpoint1", config.Endpoint);
            Assert.AreEqual("key1", config.PrimaryKey);
            Assert.AreEqual("db1", config.DatabaseId);
            Assert.AreEqual("container1", config.ContainerId);
        }

        [TestMethod]
        public void MergeWithDefault_ShouldUseDefaultValues_WhenValuesAreNull()
        {
            var config = new CosmosDbConfiguration { Endpoint = null, PrimaryKey = null, DatabaseId = null, ContainerId = null };
            var defaultConfig = new CosmosDbConfiguration { Endpoint = "endpoint2", PrimaryKey = "key2", DatabaseId = "db2", ContainerId = "container2" };

            config.MergeWithDefault(defaultConfig);

            Assert.AreEqual("endpoint2", config.Endpoint);
            Assert.AreEqual("key2", config.PrimaryKey);
            Assert.AreEqual("db2", config.DatabaseId);
            Assert.AreEqual("container2", config.ContainerId);
        }

    }
}
