using Microsoft.FeatureFlighting.Core.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.PS.FlightingService.Core.Tests.QueriesTests.GetFeatureNamesTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("GetFeatureNamesQuery")]
    [TestClass]
    public class GetFeatureNamesQueryTest
    {
        [TestMethod]
        public void Constructor_ShouldSetPropertiesCorrectly()
        {
            var tenant = "tenant1";
            var environment = "environment1";
            var correlationId = "correlationId1";
            var transactionId = "transactionId1";

            var query = new GetFeatureNamesQuery(tenant, environment, correlationId, transactionId);

            Assert.AreEqual(tenant, query.Tenant);
            Assert.AreEqual(environment, query.Environment);
            Assert.AreEqual(correlationId, query.CorrelationId);
            Assert.AreEqual(transactionId, query.TransactionId);
        }

        [DataTestMethod]
        [DataRow(null, "environment1")]
        [DataRow("tenant1", null)]
        [DataRow(null, null)]
        public void Validate_ShouldReturnFalse_WhenTenantOrEnvironmentIsNull(string tenant, string environment)
        {
            var query = new GetFeatureNamesQuery(tenant, environment, "correlationId1", "transactionId1");

            var isValid = query.Validate(out string validationErrorMessage);

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void Validate_ShouldReturnTrue_WhenTenantAndEnvironmentAreNotNull()
        {
            var query = new GetFeatureNamesQuery("tenant1", "environment1", "correlationId1", "transactionId1");

            var isValid = query.Validate(out string validationErrorMessage);

            Assert.IsTrue(isValid);
        }
    }
}
