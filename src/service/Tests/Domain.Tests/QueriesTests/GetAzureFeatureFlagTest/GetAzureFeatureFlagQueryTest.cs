using Microsoft.FeatureFlighting.Core.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.PS.FlightingService.Core.Tests.OptimizerTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("DuplicateFilterValuesOptimizationRule")]
    [TestClass]
    public class GetAzureFeatureFlagQueryTest
    {
        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenFeatureNameIsEmpty()
        {
            var query = new GetAzureFeatureFlagQuery("", "tenant", "environment", "correlationId", "transactionId");
            var result = query.Validate(out string errorMessage);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenTenantNameIsEmpty()
        {
            var query = new GetAzureFeatureFlagQuery("feature", "", "environment", "correlationId", "transactionId");
            var result = query.Validate(out string errorMessage);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenEnvironmentNameIsEmpty()
        {
            var query = new GetAzureFeatureFlagQuery("feature", "tenant", "", "correlationId", "transactionId");
            var result = query.Validate(out string errorMessage);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Validate_ShouldReturnTrue_WhenAllFieldsAreValid()
        {
            var query = new GetAzureFeatureFlagQuery("feature", "tenant", "environment", "correlationId", "transactionId");
            var result = query.Validate(out string errorMessage);
            Assert.IsTrue(result);
        }
    }
}
