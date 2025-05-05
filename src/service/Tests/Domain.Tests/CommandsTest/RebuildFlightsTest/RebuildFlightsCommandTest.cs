using Microsoft.FeatureFlighting.Core.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.PS.FlightingService.Core.Tests.CommandsTest.RebuildFlightsTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory(" RebuildFlightsCommand")]
    [TestClass]
    public class RebuildFlightsCommandTest
    {
        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenTenantIsEmpty()
        {
            // Arrange
            var command = new RebuildFlightsCommand(new List<string> { "feature1" }, "", "environment", "reason", "correlationId", "transactionId", "source");

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
            var command = new RebuildFlightsCommand(new List<string> { "feature1" }, "tenant", "", "reason", "correlationId", "transactionId", "source");

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
            var command = new RebuildFlightsCommand(new List<string> { "feature1" }, "tenant", "environment", "reason", "correlationId", "transactionId", "source");

            // Act
            var result = command.Validate(out string validationErrorMessage);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(string.Empty, validationErrorMessage);
        }
    }
}
