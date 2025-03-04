using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Common.Tests.Model.AzureAppConfig
{
    [ExcludeFromCodeCoverage]
    [TestCategory(" AzureFilter")]
    [TestClass]
    public class AzureFilterTest
    {
        [TestMethod]
        public void IsValid_ShouldReturnFalse_WhenNameIsNull()
        {
            // Arrange
            var azureFilter = new AzureFilter
            {
                Name = null,
                Parameters = new AzureFilterParameters { StageId = "1", IsActive = "true" }
            };

            // Act
            bool isValid = azureFilter.IsValid(out string validationErrorMessage);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual("Filter name and filer parameters must be present", validationErrorMessage);
        }

        [TestMethod]
        public void IsValid_ShouldReturnFalse_WhenStageIdIsNotANumber()
        {
            // Arrange
            var azureFilter = new AzureFilter
            {
                Name = "testName",
                Parameters = new AzureFilterParameters { StageId = "notANumber", IsActive = "true" }
            };

            // Act
            bool isValid = azureFilter.IsValid(out string validationErrorMessage);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual("Stage ID must be a number", validationErrorMessage);
        }

        [TestMethod]
        public void IsValid_ShouldReturnFalse_WhenIsActiveIsNotBoolean()
        {
            // Arrange
            var azureFilter = new AzureFilter
            {
                Name = "testName",
                Parameters = new AzureFilterParameters { StageId = "1", IsActive = "notABoolean" }
            };

            // Act
            bool isValid = azureFilter.IsValid(out string validationErrorMessage);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual("IsActive must be boolean", validationErrorMessage);
        }

        [TestMethod]
        public void IsValid_ShouldReturnTrue_WhenAllPropertiesAreValid()
        {
            // Arrange
            var azureFilter = new AzureFilter
            {
                Name = "testName",
                Parameters = new AzureFilterParameters { StageId = "1", IsActive = "true" }
            };

            // Act
            bool isValid = azureFilter.IsValid(out string validationErrorMessage);

            // Assert
            Assert.IsTrue(isValid);
            Assert.IsNull(validationErrorMessage);
        }

        [TestMethod]
        public void IsActive_ShouldReturnTrue_WhenIsActiveIsTrue()
        {
            // Arrange
            var azureFilter = new AzureFilter
            {
                Name = "testName",
                Parameters = new AzureFilterParameters { StageId = "1", IsActive = "true" }
            };

            // Act
            bool isActive = azureFilter.IsActive();

            // Assert
            Assert.IsTrue(isActive);
        }

        [TestMethod]
        public void IsActive_ShouldReturnFalse_WhenIsActiveIsFalse()
        {
            // Arrange
            var azureFilter = new AzureFilter
            {
                Name = "testName",
                Parameters = new AzureFilterParameters { StageId = "1", IsActive = "false" }
            };

            // Act
            bool isActive = azureFilter.IsActive();

            // Assert
            Assert.IsFalse(isActive);
        }
    }
}
