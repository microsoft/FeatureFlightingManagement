using Microsoft.FeatureFlighting.Core.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.PS.FlightingService.Core.Tests.CommandsTest.UpdateMetricsTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory(" UpdateMetricsCommand")]
    [TestClass]
    public class UpdateMetricsCommandTest
    {
        [TestMethod]
        public void Validate_ShouldReturnIsIsFalse_WhenFeatureNameIsEmpty()
        {
            // Arrange
            var command = new UpdateMetricsCommand("", "tenant", "environment", 1, "correlationId", "transactionId", "source");

            // Act
            var result = command.Validate(out string validationErrorMessage);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual("Feature name cannot be null or empty | ", validationErrorMessage);
        }

        [TestMethod]
        public void Validate_ShouldReturnIsIsFalse_WhenTenantIsEmpty()
        {
            // Arrange
            var command = new UpdateMetricsCommand("featureName", "", "environment", 1, "correlationId", "transactionId", "source");

            // Act
            var result = command.Validate(out string validationErrorMessage);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual("Tenant cannot be null or empty | ", validationErrorMessage);
        }

        [TestMethod]
        public void Validate_ShouldReturnIsIsFalse_WhenEnvironmentIsEmpty()
        {
            // Arrange
            var command = new UpdateMetricsCommand("featureName", "tenant", "", 1, "correlationId", "transactionId", "source");

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
            var command = new UpdateMetricsCommand("featureName", "tenant", "environment", 1, "correlationId", "transactionId", "source");

            // Act
            var result = command.Validate(out string validationErrorMessage);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(string.Empty, validationErrorMessage);
        }

        [TestMethod]
        public void AdjustTimespan_ShouldSetTimespanToDefault_WhenLastMetricsGeneratedOnIsNull()
        {
            // Arrange
            var command = new UpdateMetricsCommand("featureName", "tenant", "environment", 0, "correlationId", "transactionId", "source");
            int defaultTimespan = 7;

            // Act
            command.AdjustTimespan(DateTime.UtcNow.AddDays(10), defaultTimespan);

            // Assert
            Assert.AreEqual(defaultTimespan, command.TimespanInDays);
        }

        [TestMethod]
        public void AdjustTimespan_ShouldSetTimespanToDaysSinceMetricsGenerated_WhenLastMetricsGeneratedOnIsNotNull()
        {
            // Arrange
            var command = new UpdateMetricsCommand("featureName", "tenant", "environment", 0, "correlationId", "transactionId", "source");
            int defaultTimespan = 7;
            DateTime lastMetricsGeneratedOn = DateTime.UtcNow.AddDays(-5);

            // Act
            command.AdjustTimespan(lastMetricsGeneratedOn, defaultTimespan);

            // Assert
            Assert.AreEqual(5, command.TimespanInDays);
        }
    }
}
