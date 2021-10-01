using Microsoft.Graph;
using Microsoft.PS.FlightingService.Domain.Evaluators;
using Microsoft.PS.FlightingService.Domain.FeatureFilters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.PS.FlightingService.Domain.Tests.OperatorTests
{
    [TestClass]
    public class OperatorEvaluatorStrategyTest
    {
        private IList<BaseOperatorEvaluator> _evaluators = new List<BaseOperatorEvaluator>();
        private OperatorEvaluatorStrategy operatorEvaluatorStrategy;
        [TestInitialize]
        public void TestStartUp ()
        {
            BaseOperatorEvaluator equalOperator = new EqualEvaluator();
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
