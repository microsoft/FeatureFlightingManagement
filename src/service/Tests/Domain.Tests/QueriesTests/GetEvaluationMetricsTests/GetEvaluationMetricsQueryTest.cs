using Microsoft.FeatureFlighting.Core.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.PS.FlightingService.Core.Tests.QueriesTests.GetEvaluationMetricsTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("GetEvaluationMetricsQuery")]
    [TestClass]
    public class GetEvaluationMetricsQueryTest
    {
        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenFeatureNameIsEmpty()
        {
            var query = new GetEvaluationMetricsQuery("", "tenant", "environment", 1, "correlationId", "transactionId");
            var result = query.Validate(out string errorMessage);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenTenantIsEmpty()
        {
            var query = new GetEvaluationMetricsQuery("feature", "", "environment", 1, "correlationId", "transactionId");
            var result = query.Validate(out string errorMessage);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenEnvironmentIsEmpty()
        {
            var query = new GetEvaluationMetricsQuery("feature", "tenant", "", 1, "correlationId", "transactionId");
            var result = query.Validate(out string errorMessage);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Validate_ShouldReturnFalse_WhenTimespanInDaysIsZeroOrNegative()
        {
            var query = new GetEvaluationMetricsQuery("feature", "tenant", "environment", 0, "correlationId", "transactionId");
            var result = query.Validate(out string errorMessage);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Validate_ShouldReturnTrue_WhenAllFieldsAreValid()
        {
            var query = new GetEvaluationMetricsQuery("feature", "tenant", "environment", 1, "correlationId", "transactionId");
            var result = query.Validate(out string errorMessage);
            Assert.IsTrue(result);
        }
    }
}
