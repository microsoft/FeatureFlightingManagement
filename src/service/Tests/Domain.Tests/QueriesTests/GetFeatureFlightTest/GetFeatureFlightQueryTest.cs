using Microsoft.FeatureFlighting.Core.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.PS.FlightingService.Core.Tests.QueriesTests.GetFeatureFlightTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("GetFeatureFlightQuery")]
    [TestClass]
    public class GetFeatureFlightQueryTest
    {
        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenFeatureNameIsEmpty()
        {
            var query = new GetFeatureFlightQuery("", "tenant", "environment", "correlationId", "transactionId");
            var result = query.Validate(out string errorMessage);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenTenantIsEmpty()
        {
            var query = new GetFeatureFlightQuery("feature", "", "environment", "correlationId", "transactionId");
            var result = query.Validate(out string errorMessage);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenEnvironmentIsEmpty()
        {
            var query = new GetFeatureFlightQuery("feature", "tenant", "", "correlationId", "transactionId");
            var result = query.Validate(out string errorMessage);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Validate_ShouldReturnTrue_WhenAllFieldsAreValid()
        {
            var query = new GetFeatureFlightQuery("feature", "tenant", "environment", "correlationId", "transactionId");
            var result = query.Validate(out string errorMessage);
            Assert.IsTrue(result);
        }
    }
}
