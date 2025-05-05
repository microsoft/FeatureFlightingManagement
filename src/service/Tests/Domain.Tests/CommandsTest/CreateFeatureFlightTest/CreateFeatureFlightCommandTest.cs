using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;
using Microsoft.FeatureFlighting.Core.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.PS.FlightingService.Core.Tests.CommandsTest.CreateFeatureFlightTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("CreateFeatureFlightCommand")]
    [TestClass]
    public class CreateFeatureFlightCommandTest
    {
        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenAzureFeatureFlagIsNull()
        {
            // Arrange
            var command = new CreateFeatureFlightCommand(null, "testCorrelationId", "testTransactionId", "testSource");

            // Act
            bool isValid = command.Validate(out string validationErrorMessage);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual("New Feature flag value cannot be null", validationErrorMessage);
        }

        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenAzureFeatureFlagIsInvalid()
        {
            // Arrange
            var azureFeatureFlag = new AzureFeatureFlag
            {
                Label = "testLabel"
            };
            var command = new CreateFeatureFlightCommand(azureFeatureFlag, "testCorrelationId", "testTransactionId", "testSource");

            // Act
            bool isValid = command.Validate(out string validationErrorMessage);

            // Assert
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void Validate_ShouldReturnTrue_WhenAzureFeatureFlagIsValid()
        {
            // Arrange
            var azureFeatureFlag = new AzureFeatureFlag
            {
                Label = "testLabel",
                Tenant= "testTenant",
                Environment= "testEnvironment",
                Conditions= new AzureFilterCollection() { Client_Filters=new AzureFilter[] { } },
            };
            var command = new CreateFeatureFlightCommand(azureFeatureFlag, "testCorrelationId", "testTransactionId", "testSource");

            // Act
            bool isValid = command.Validate(out string validationErrorMessage);

            // Assert
            Assert.IsTrue(isValid);
        }
    }
}
