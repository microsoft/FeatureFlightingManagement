using Microsoft.FeatureFlighting.Core.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PS.FlightingService.Core.Tests.QueriesTests.VerifyRuleEngineTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("VerifyRulesEngineQuery")]
    [TestClass]
    public class VerifyRulesEngineQueryTest
    {
        [TestMethod]
        public void Constructor_ShouldSetPropertiesCorrectly()
        {
            var tenant = "tenant1";
            var workflowName = "workflow1";
            var workflowPayload = "payload1";
            var flightContext = "context1";
            var debug = true;
            var correlationId = "correlationId1";
            var transactionId = "transactionId1";

            var query = new VerifyRulesEngineQuery(tenant, workflowName, workflowPayload, flightContext, debug, correlationId, transactionId);

            Assert.AreEqual(tenant, query.Tenant);
            Assert.AreEqual(workflowName, query.WorkflowName);
            Assert.AreEqual(workflowPayload, query.WorkflowPayload);
            Assert.AreEqual(flightContext, query.FlightContext);
            Assert.AreEqual(debug, query.Debug);
            Assert.AreEqual(correlationId, query.CorrelationId);
            Assert.AreEqual(transactionId, query.TransactionId);
            Assert.IsNotNull(query.Id);
        }

        [DataTestMethod]
        [DataRow(null, "workflow1", "payload1", "context1")]
        [DataRow("tenant1", null, "payload1", "context1")]
        [DataRow("tenant1", "workflow1", null, "context1")]
        [DataRow("tenant1", "workflow1", "payload1", null)]
        public void Validate_ShouldReturnFalse_WhenAnyPropertyIsNull(string tenant, string workflowName, string workflowPayload, string flightContext)
        {
            var query = new VerifyRulesEngineQuery(tenant, workflowName, workflowPayload, flightContext, true, "correlationId1", "transactionId1");

            var isValid = query.Validate(out string validationErrorMessage);

            Assert.IsFalse(isValid);
            
        }

        [TestMethod]
        public void Validate_ShouldReturnTrue_WhenAllPropertiesAreNotNull()
        {
            var query = new VerifyRulesEngineQuery("tenant1", "workflow1", "payload1", "context1", true, "correlationId1", "transactionId1");

            var isValid = query.Validate(out string validationErrorMessage);

            Assert.IsTrue(isValid);
        }
    }
}
