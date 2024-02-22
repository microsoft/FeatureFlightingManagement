using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Common.Tests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("FlagUtilities")]
    [TestClass]
    public class FlagUtilitiesTest
    {
        [TestMethod]
        public void GetFeatureFlagId_ShouldReturnCorrectId()
        {
            // Arrange
            string appName = "testApp";
            string envName = "testEnv";
            string name = "testName";
            string expectedId = string.Format("{0}_{1}_{2}", appName.ToLowerInvariant(), envName.ToLowerInvariant(), name); // Replace with actual format string

            // Act
            string id = FlagUtilities.GetFeatureFlagId(appName, envName, name);

            // Assert
            Assert.AreEqual(expectedId, id);
        }

        [TestMethod]
        public void GetFeatureFlagName_ShouldReturnCorrectName()
        {
            // Arrange
            string appName = "testApp";
            string envName = "testEnv";
            string name = "testName";
            string id = string.Format("{0}:{1}:{2}", appName.ToLowerInvariant(), envName.ToLowerInvariant(), name); // Replace with actual format string

            // Act
            string actualName = FlagUtilities.GetFeatureFlagName(appName, envName, id);

            // Assert
            Assert.AreEqual(name, actualName);
        }
    }
}
