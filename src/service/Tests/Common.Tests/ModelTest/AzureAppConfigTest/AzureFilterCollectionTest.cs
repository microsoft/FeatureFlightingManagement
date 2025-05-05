using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Common.Tests.Model.AzureAppConfig
{
    [ExcludeFromCodeCoverage]
    [TestCategory(" AzureFilterCollection")]
    [TestClass]
    public class AzureFilterCollectionTest
    {
        [TestMethod]
        public void IsValid_ShouldReturnTrue_WhenClientFiltersIsNull()
        {
            // Arrange
            var azureFilterCollection = new AzureFilterCollection
            {
                Client_Filters = null
            };

            // Act
            bool isValid = azureFilterCollection.IsValid(out string validationErrorMessage);

            // Assert
            Assert.IsTrue(isValid);
            Assert.IsNull(validationErrorMessage);
        }

        [TestMethod]
        public void IsValid_ShouldReturnFalse_WhenAnyFilterIsInvalid()
        {
            // Arrange
            var azureFilterCollection = new AzureFilterCollection
            {
                Client_Filters = new AzureFilter[]
                {
                new AzureFilter { Name = "testName", Parameters = new AzureFilterParameters { StageId = "1", IsActive = "true" } },
                new AzureFilter { Name = null, Parameters = new AzureFilterParameters { StageId = "1", IsActive = "true" } }
                }
            };

            // Act
            bool isValid = azureFilterCollection.IsValid(out string validationErrorMessage);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual("Filter name and filer parameters must be present", validationErrorMessage);
        }

        [TestMethod]
        public void IsValid_ShouldReturnTrue_WhenAllFiltersAreValid()
        {
            // Arrange
            var azureFilterCollection = new AzureFilterCollection
            {
                Client_Filters = new AzureFilter[]
                {
                new AzureFilter { Name = "testName", Parameters = new AzureFilterParameters { StageId = "1", IsActive = "true" } },
                new AzureFilter { Name = "testName2", Parameters = new AzureFilterParameters { StageId = "2", IsActive = "false" } }
                }
            };

            // Act
            bool isValid = azureFilterCollection.IsValid(out string validationErrorMessage);

            // Assert
            Assert.IsTrue(isValid);
            Assert.IsNull(validationErrorMessage);
        }
    }
}
