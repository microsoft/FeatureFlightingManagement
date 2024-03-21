using Microsoft.FeatureFlighting.Core.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PS.FlightingService.Core.Tests.QueriesTests.GetFeatureFlightsTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("GetFeatureFlightsQuery")]
    [TestClass]
    public class GetFeatureFlightsQueryTest
    {
        [TestMethod]
        public void Constructor_ShouldSetPropertiesCorrectly()
        {
            var tenant = "tenant1";
            var environment = "environment1";
            var correlationId = "correlationId1";
            var transactionId = "transactionId1";

            var query = new GetFeatureFlightsQuery(tenant, environment, correlationId, transactionId);

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
            var query = new GetFeatureFlightsQuery(tenant, environment, "correlationId1", "transactionId1");

            var isValid = query.Validate(out string validationErrorMessage);

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void Validate_ShouldReturnTrue_WhenTenantAndEnvironmentAreNotNull()
        {
            var query = new GetFeatureFlightsQuery("tenant1", "environment1", "correlationId1", "transactionId1");

            var isValid = query.Validate(out string validationErrorMessage);

            Assert.IsTrue(isValid);
        }
    }
}
