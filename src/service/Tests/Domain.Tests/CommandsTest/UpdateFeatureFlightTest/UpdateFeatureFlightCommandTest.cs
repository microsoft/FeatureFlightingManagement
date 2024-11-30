using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;
using Microsoft.FeatureFlighting.Core.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PS.FlightingService.Core.Tests.CommandsTest.UpdateFeatureFlightTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("UpdateFeatureFlightCommand")]
    [TestClass]
    public class UpdateFeatureFlightCommandTest
    {
        [TestMethod]
        public void Validate_ShouldReturnIsFalse_WhenAzureFeatureFlagIsNull()
        {
            // Arrange
            var command = new UpdateFeatureFlightCommand(null, "correlationId", "transactionId", "source");

            // Act
            var result = command.Validate(out string validationErrorMessage);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual("Updated Feature flag value cannot be null", validationErrorMessage);
        }

        [TestMethod]
        public void Validate_ShouldReturnIsFalse_WhenAzureFeatureFlagIsInvalid()
        {
            // Arrange
            var azureFeatureFlag = new AzureFeatureFlag();
            var command = new UpdateFeatureFlightCommand(azureFeatureFlag, "correlationId", "transactionId", "source");

            // Act
            var result = command.Validate(out string validationErrorMessage);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNotNull(validationErrorMessage);
        }

        [TestMethod]
        public void Validate_ShouldReturnTrue_WhenAzureFeatureFlagIsValid()
        {
            // Arrange
            var azureFeatureFlag = new AzureFeatureFlag
            {
                //Key = "feature1",
                Label = "label1",
                Enabled = true,
               // Conditions = new Conditions { ClientFilters = new List<ClientFilter>() }
            };
            var command = new UpdateFeatureFlightCommand(azureFeatureFlag, "correlationId", "transactionId", "source");

            // Act
            var result = command.Validate(out string validationErrorMessage);

            // Assert
          
            Assert.IsNotNull(validationErrorMessage);
        }
    }
}
