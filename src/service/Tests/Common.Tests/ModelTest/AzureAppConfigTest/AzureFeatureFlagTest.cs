using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Common.Tests.Model.AzureAppConfig
{
    [ExcludeFromCodeCoverage]
    [TestCategory("AzureFeatureFlag")]
    [TestClass]
    public class AzureFeatureFlagTest
    {
        [TestMethod]
        public void IsValid_ShouldReturnFalse_WhenTenantIsNull()
        {
            // Arrange
            var azureFeatureFlag = new AzureFeatureFlag
            {
                Tenant = null,
                Environment = "testEnvironment"
            };

            // Act
            bool isValid = azureFeatureFlag.IsValid(out string validationErrorMessage);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual("Tenant cannot be null", validationErrorMessage);
        }

        [TestMethod]
        public void IsValid_ShouldReturnFalse_WhenEnvironmentIsNull()
        {
            // Arrange
            var azureFeatureFlag = new AzureFeatureFlag
            {
                Tenant = "testTenant",
                Environment = null
            };

            // Act
            bool isValid = azureFeatureFlag.IsValid(out string validationErrorMessage);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual("Environment cannot be null", validationErrorMessage);
        }

        [TestMethod]
        public void IsValid_ShouldReturnTrue_WhenTenantAndEnvironmentAreNotNull()
        {
            // Arrange
            var azureFeatureFlag = new AzureFeatureFlag
            {
                Tenant = "testTenant",
                Environment = "testEnvironment"
            };

            // Act
            bool isValid = azureFeatureFlag.IsValid(out string validationErrorMessage);

            // Assert
            Assert.IsTrue(isValid);
            Assert.IsNull(validationErrorMessage);
        }
    }
}
