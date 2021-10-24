using System.Collections.Generic;
using Microsoft.FeatureFlighting.Core.Operators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Core.Tests.OperatorTests
{
    [TestClass]
    public class OperatorEvaluatorStrategyTest
    {
        private IList<BaseOperator> _evaluators = new List<BaseOperator>();
        private OperatorEvaluatorStrategy operatorEvaluatorStrategy;
        [TestInitialize]
        public void TestStartUp ()
        {
            BaseOperator equalOperator = new EqualOperator();
            _evaluators.Add(equalOperator);
             operatorEvaluatorStrategy = new OperatorEvaluatorStrategy(_evaluators);
        }
        [TestMethod]
        public void validate_get_for_equals_operator()
        {

            //Arrange
            Operator op = Operator.Equals;
            //Act
            var result = operatorEvaluatorStrategy.Get(op);
            //Assert
            Assert.AreEqual(result.Operator,op);
        }
        [TestMethod]
        public void validate_Get_Filter_Operator_Mapping_for_equals_operator()
        {

            
            //Act
            var result = operatorEvaluatorStrategy.GetFilterOperatorMapping();
            //Assert
            Assert.AreEqual(result["Alias"][0], "Equals");
            Assert.AreEqual(result["RoleGroup"][0], "Equals");
            Assert.AreEqual(result["Country"][0], "Equals");
            Assert.AreEqual(result["Region"][0], "Equals");
            Assert.AreEqual(result["UserUpn"][0], "Equals");
            Assert.AreEqual(result["Generic"][0], "Equals");
            Assert.AreEqual(result["Role"][0], "Equals");
        }
    
    }
}
