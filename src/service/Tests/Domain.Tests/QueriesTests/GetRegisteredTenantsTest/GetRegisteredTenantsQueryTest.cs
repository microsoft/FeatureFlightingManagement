using Microsoft.FeatureFlighting.Core.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.PS.FlightingService.Core.Tests.QueriesTests.GetRegisteredTenantsTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("GetRegisteredTenantsQuery")]
    [TestClass]
    public class GetRegisteredTenantsQueryTest
    {
        [TestMethod]
        public void Constructor_ShouldSetPropertiesCorrectly()
        {
            var includeDynamicTenants = true;

            var query = new GetRegisteredTenantsQuery(includeDynamicTenants);

            Assert.AreEqual(includeDynamicTenants, query.IncludeDynamicTenants);
            Assert.IsNotNull(query.Id);
        }

        [TestMethod]
        public void Validate_ShouldAlwaysReturnTrue()
        {
            var query = new GetRegisteredTenantsQuery(true);

            var isValid = query.Validate(out string validationErrorMessage);

            Assert.IsTrue(isValid);
            Assert.IsNull(validationErrorMessage);
        }
    }
}

