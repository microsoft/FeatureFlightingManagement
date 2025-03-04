using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Microsoft.FeatureFlighting.Common.Tests.ConfigTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("AuthorizationConfiguration")]
    [TestClass]
    public class AuthorizationConfigurationTest
    {
        [TestMethod]
        public void GetAdministrators_ShouldReturnCorrectList_WhenAdministratorsIsNotNull()
        {
            var config = new AuthorizationConfiguration { Administrators = "admin1,admin2,admin3" };

            var result = config.GetAdministrators();

            Assert.AreEqual(3, result.Count());
            
        }

        [TestMethod]
        public void GetAdministrators_ShouldReturnEmptyList_WhenAdministratorsIsNull()
        {
            var config = new AuthorizationConfiguration { Administrators = null };

            var result = config.GetAdministrators();

            Assert.AreEqual(result.Count(),0);
        }

        [TestMethod]
        public void MergeWithDefault_ShouldMergeCorrectly()
        {
            var config = new AuthorizationConfiguration { Type = "type1", Administrators = "admin1", SenderAppName = "app1" };
            var defaultConfig = new AuthorizationConfiguration { Type = "type2", Administrators = "admin2", SenderAppName = "app2" };

            config.MergeWithDefault(defaultConfig);

            Assert.AreEqual("type1", config.Type);
            Assert.AreEqual("admin1", config.Administrators);
            Assert.AreEqual("app1", config.SenderAppName);
        }

        [TestMethod]
        public void MergeWithDefault_ShouldUseDefaultValues_WhenValuesAreNull()
        {
            var config = new AuthorizationConfiguration { Type = null, Administrators = null, SenderAppName = null };
            var defaultConfig = new AuthorizationConfiguration { Type = "type2", Administrators = "admin2", SenderAppName = "app2" };

            config.MergeWithDefault(defaultConfig);

            Assert.AreEqual("type2", config.Type);
            Assert.AreEqual("admin2", config.Administrators);
            Assert.AreEqual("app2", config.SenderAppName);
        }

    }
}
