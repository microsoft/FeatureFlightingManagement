using Microsoft.FeatureFlighting.Core.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.PS.FlightingService.Core.Tests.CommandsTest.EnableFeatureFlightTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("EnableFeatureFlightCommand")]
    [TestClass]
    public class EnableFeatureFlightCommandTest
    {
        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenFeatureNameIsEmpty()
        {
            // Arrange
            var command = new EnableFeatureFlightCommand("", "tenant", "environment", "correlationId", "transactionId", "source");

            // Act
            var result = command.Validate(out string validationErrorMessage);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual("Feature name cannot be null or empty | ", validationErrorMessage);
        }

        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenTenantIsEmpty()
        {
            // Arrange
            var command = new EnableFeatureFlightCommand("featureName", "", "environment", "correlationId", "transactionId", "source");

            // Act
            var result = command.Validate(out string validationErrorMessage);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual("Tenant cannot be null or empty | ", validationErrorMessage);
        }

        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenEnvironmentIsEmpty()
        {
            // Arrange
            var command = new EnableFeatureFlightCommand("featureName", "tenant", "", "correlationId", "transactionId", "source");

            // Act
            var result = command.Validate(out string validationErrorMessage);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual("Environment cannot be null or empty", validationErrorMessage);
        }

        [TestMethod]
        public void Validate_ShouldReturnTrue_WhenAllFieldsAreValid()
        {
            // Arrange
            var command = new EnableFeatureFlightCommand("featureName", "tenant", "environment", "correlationId", "transactionId", "source");

            // Act
            var result = command.Validate(out string validationErrorMessage);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(string.Empty, validationErrorMessage);
        }
    }
}
