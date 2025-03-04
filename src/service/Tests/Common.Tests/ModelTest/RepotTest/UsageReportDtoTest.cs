using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Common.Tests.ModelTest.RepotTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory(" UsageReportDto")]
    [TestClass]
    public class UsageReportDtoTest
    {
        [TestMethod]
        public void CreateFlightSelectorBody_ShouldCreateFlightSelectorBody()
        {
            // Arrange
            var usageReportDto = new UsageReportDto("testTenant", "testEnvironment", "testUser")
            {
                UnusedFeatures = new List<ThresholdExceededReportDto> { new ThresholdExceededReportDto { FeatureName = "feature1" } },
                LongInactiveFeatures = new List<ThresholdExceededReportDto> { new ThresholdExceededReportDto { FeatureName = "feature2" } },
                LongActiveFeatures = new List<ThresholdExceededReportDto> { new ThresholdExceededReportDto { FeatureName = "feature3" } },
                LongLaunchedFeatures = new List<ThresholdExceededReportDto> { new ThresholdExceededReportDto { FeatureName = "feature4" } }
            };

            // Act
            usageReportDto.CreateFlightSelectorBody();

            // Assert
            Assert.IsNotNull(usageReportDto.FlightSelectorBody);
        }

        [TestMethod]
        public void UpdatePendingAction_ShouldUpdatePendingAction()
        {
            // Arrange
            var usageReportDto = new UsageReportDto("testTenant", "testEnvironment", "testUser")
            {
                UnusedFeatures = new List<ThresholdExceededReportDto> { new ThresholdExceededReportDto { FeatureName = "feature1" } },
                LongInactiveFeatures = new List<ThresholdExceededReportDto> { new ThresholdExceededReportDto { FeatureName = "feature2" } },
                LongActiveFeatures = new List<ThresholdExceededReportDto> { new ThresholdExceededReportDto { FeatureName = "feature3" } }
            };

            // Act
            usageReportDto.UpdatePendingAction();

            // Assert
            Assert.IsTrue(usageReportDto.PendingAction);
            Assert.AreEqual("Yes (See Below)", usageReportDto.PendingActionDescription);
        }

        [TestMethod]
        public void ActiveFeaturesCount_ShouldReturnCorrectCount()
        {
            // Arrange
            var usageReportDto = new UsageReportDto("testTenant", "testEnvironment", "testUser")
            {
                ActiveFeatures = new List<string> { "feature1", "feature2", "feature3" }
            };

            // Assert
            Assert.AreEqual(3, usageReportDto.ActiveFeaturesCount);
        }

        [TestMethod]
        public void NewAddedFeaturesCount_ShouldReturnCorrectCount()
        {
            // Arrange
            var usageReportDto = new UsageReportDto("testTenant", "testEnvironment", "testUser")
            {
                NewlyAddedFeatures = new List<string> { "feature1", "feature2", "feature3" }
            };

            // Assert
            Assert.AreEqual(3, usageReportDto.NewAddedFeaturesCount);
        }
    }
}
