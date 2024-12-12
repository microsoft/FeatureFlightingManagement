using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Core.RulesEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RulesEngine.Interfaces;
using RulesEngine.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.PS.FlightingService.Core.Tests.RulesEngineTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("RulesEngineEvaluator")]
    [TestClass]
    public class RulesEngineEvaluatorTest
    {
        private readonly Mock<IRulesEngine> _rulesEngineMock;
        private readonly RulesEngineEvaluator _rulesEngineEvaluator;

        public RulesEngineEvaluatorTest()
        {
            _rulesEngineMock = new Mock<IRulesEngine>();
            _rulesEngineEvaluator = new RulesEngineEvaluator(_rulesEngineMock.Object, "workflow1", new TenantConfiguration());
        }

        [TestMethod]
        public async Task Evaluate_ShouldReturnSuccessfulResult_WhenRulesEngineReturnsSuccessfulResult()
        {
            var ruleResultTree = new List<RuleResultTree> { new RuleResultTree { IsSuccess = true } };
            _rulesEngineMock.Setup(re => re.ExecuteAllRulesAsync(It.IsAny<string>(), It.IsAny<RuleParameter[]>())).ReturnsAsync(ruleResultTree);
            LoggerTrackingIds loggerTrackingIds = new LoggerTrackingIds() { CorrelationId="test id",TransactionId="t 2001"};
            var result = await _rulesEngineEvaluator.Evaluate(new Dictionary<string, object>(), loggerTrackingIds);

            Assert.IsTrue(result.Result);
        }

        [TestMethod]
        public async Task Evaluate_ShouldReturnFailedResult_WhenRulesEngineReturnsFailedResult()
        {
            var ruleResultTree = new List<RuleResultTree> { new RuleResultTree { IsSuccess = false, ExceptionMessage = "Error" } };
            _rulesEngineMock.Setup(re => re.ExecuteAllRulesAsync(It.IsAny<string>(), It.IsAny<RuleParameter[]>())).ReturnsAsync(ruleResultTree);
            LoggerTrackingIds loggerTrackingIds = new LoggerTrackingIds() { CorrelationId = "test id", TransactionId = "t 2001" };
            var result = await _rulesEngineEvaluator.Evaluate(new Dictionary<string, object>(), loggerTrackingIds);

            Assert.IsFalse(result.Result);
            Assert.AreEqual("Error", result.Message);
        }
    }
}

