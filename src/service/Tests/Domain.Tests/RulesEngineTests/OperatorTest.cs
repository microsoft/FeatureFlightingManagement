using Microsoft.FeatureFlighting.Core.Operators;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.PS.FlightingService.Core.Tests.RulesEngineTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("Operator")]
    [TestClass]
    public class OperatorTest
    {
        private readonly Mock<IOperatorStrategy> _operatorStrategyMock;
        private readonly BaseOperator _baseOperatorMock;

        public OperatorTest()
        {
            var result = new EvaluationResult(true, "pass");
            Task<EvaluationResult> evaluationResult = Task.FromResult(result);
            _operatorStrategyMock = new Mock<IOperatorStrategy>();
            _baseOperatorMock = Mock.Of<BaseOperator>(op => op.Evaluate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FeatureFlighting.Common.LoggerTrackingIds>()) == evaluationResult);
        }

        [TestMethod]
        public void In_ShouldReturnTrue_WhenOperatorStrategyReturnsTrue()
        {
            _operatorStrategyMock.Setup(os => os.Get(FeatureFlighting.Core.FeatureFilters.Operator.In)).Returns(_baseOperatorMock);
            FeatureFlighting.Core.RulesEngine.Operator.Initialize(_operatorStrategyMock.Object);

            var result = FeatureFlighting.Core.RulesEngine.Operator.In("contextValue", "configuredValue");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void NotIn_ShouldReturnTrue_WhenOperatorStrategyReturnsTrue()
        {
            _operatorStrategyMock.Setup(os => os.Get(FeatureFlighting.Core.FeatureFilters.Operator.NotIn)).Returns(_baseOperatorMock);
            FeatureFlighting.Core.RulesEngine.Operator.Initialize(_operatorStrategyMock.Object);

            var result = FeatureFlighting.Core.RulesEngine.Operator.NotIn("contextValue", "configuredValue");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsMember_ShouldReturnTrue_WhenOperatorStrategyReturnsTrue()
        {
            _operatorStrategyMock.Setup(os => os.Get(FeatureFlighting.Core.FeatureFilters.Operator.MemberOfSecurityGroup)).Returns(_baseOperatorMock);
            FeatureFlighting.Core.RulesEngine.Operator.Initialize(_operatorStrategyMock.Object);

            var result = FeatureFlighting.Core.RulesEngine.Operator.IsMember("contextValue", "configuredValue");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsNotMember_ShouldReturnTrue_WhenOperatorStrategyReturnsTrue()
        {
            _operatorStrategyMock.Setup(os => os.Get(FeatureFlighting.Core.FeatureFilters.Operator.NotMemberOfSecurityGroup)).Returns(_baseOperatorMock);
            FeatureFlighting.Core.RulesEngine.Operator.Initialize(_operatorStrategyMock.Object);

            var result = FeatureFlighting.Core.RulesEngine.Operator.IsNotMember("contextValue", "configuredValue");

            Assert.IsTrue(result);
        }

    }
}
