using Microsoft.FeatureFlighting.Core.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.PS.FlightingService.Core.Tests.Commands.ActivateStage
{
    [ExcludeFromCodeCoverage]
    [TestCategory("ActivateStageCommand")]
    [TestClass]
    public class ActivateStageCommandTest
    {
        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenFeatureNameIsNull()
        {
            // Arrange
            var command = new ActivateStageCommand(null, "testTenant", "testEnvironment", "testStage", "testCorrelationId", "testTransactionId", "testSource");

            // Act
            bool isValid = command.Validate(out string validationErrorMessage);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual("Feature name cannot be null or empty | ", validationErrorMessage);
        }

        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenTenantIsNull()
        {
            // Arrange
            var command = new ActivateStageCommand("testFeature", null, "testEnvironment", "testStage", "testCorrelationId", "testTransactionId", "testSource");

            // Act
            bool isValid = command.Validate(out string validationErrorMessage);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual("Tenant cannot be null or empty | ", validationErrorMessage);
        }

        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenEnvironmentIsNull()
        {
            // Arrange
            var command = new ActivateStageCommand("testFeature", "testTenant", null, "testStage", "testCorrelationId", "testTransactionId", "testSource");

            // Act
            bool isValid = command.Validate(out string validationErrorMessage);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual("Environment cannot be null or empty", validationErrorMessage);
        }

        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenStageNameIsNull()
        {
            // Arrange
            var command = new ActivateStageCommand("testFeature", "testTenant", "testEnvironment", null, "testCorrelationId", "testTransactionId", "testSource");

            // Act
            bool isValid = command.Validate(out string validationErrorMessage);

            // Assert
            Assert.IsFalse(isValid);
            Assert.AreEqual("Stage Name to be activated cannot be null or empty", validationErrorMessage);
        }

        [TestMethod]
        public void Validate_ShouldReturnTrue_WhenAllPropertiesAreValid()
        {
            // Arrange
            var command = new ActivateStageCommand("testFeature", "testTenant", "testEnvironment", "testStage", "testCorrelationId", "testTransactionId", "testSource");

            // Act
            bool isValid = command.Validate(out string validationErrorMessage);

            // Assert
            Assert.IsTrue(isValid);
           
        }
    }
}
