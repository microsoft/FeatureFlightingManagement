using Microsoft.FeatureFlighting.Core.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.PS.FlightingService.Core.Tests.CommandsTest.GenerateReportTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("GenerateReportCommand")]
    [TestClass]
    public class GenerateReportCommandTest
    {
        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenTenantIsEmpty()
        {
            // Arrange
            var command = new GenerateReportCommand("", "environment", false, "correlationId", "transactionId");

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
            var command = new GenerateReportCommand("tenant", "", false, "correlationId", "transactionId");

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
            var command = new GenerateReportCommand("tenant", "environment", false, "correlationId", "transactionId");

            // Act
            var result = command.Validate(out string validationErrorMessage);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(string.Empty, validationErrorMessage);
        }
    }
}
