using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Operators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Core.Tests.OperatorTests
{
    [TestClass]
    public class LesserThanEvaluatorTest
    {
        private LesserThanOperator evaluator;
        private string[] listOfFilters;

        [TestInitialize]
        public void TestStartup()
        {
            evaluator = new LesserThanOperator();
            listOfFilters = evaluator.SupportedFilters;
        }

        [TestMethod]
        public async Task evaluate_less_than_operator_returns_true_for_correct_value_for_alias()
        {
            //Arrange
            string contextValue = "pratikb";
            string configuredValue = "twsharma";
            string filterType = "Alias";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };

            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, true);
        }

        [TestMethod]
        public async Task evaluate_less_than_operator_returns_false_for_incorrect_value_for_alias()
        {
            //Arrange
            string contextValue = "twsh";
            string configuredValue = "pratikb";
            string filterType = "Alias";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };


            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, false);
        }
        [TestMethod]
        public async Task evaluate_less_than_operator_returns_true_for_correct_value_for_country()
        {
            //Arrange
            string contextValue = "India";
            string configuredValue = "Italy";
            string filterType = "Country";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };


            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, true);
        }

        [TestMethod]
        public async Task evaluate_less_than_operator_returns_false_for_incorrect_value_for_country()
        {
            //Arrange
            string contextValue = "Italy";
            string configuredValue = "India";
            string filterType = "Country";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };

            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, false);
        }
        [TestMethod]
        public async Task evaluate_less_than_operator_returns_true_for_correct_value_for_upn()
        {
            //Arrange
            string contextValue = "pratikb@microsoft.com";
            string configuredValue = "twsharma@microsoft.com";
            string filterType = "UserUpn";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };
            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, true);
        }

        [TestMethod]
        public async Task evaluate_less_than_operator_returns_false_for_incorrect_value_for_upn()
        {
            //Arrange
            string contextValue = "twsharma@microsoft.com";
            string configuredValue = "pratikb@microsoft.com";
            string filterType = "UserUpn";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };
            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, false);
        }

        [TestMethod]
        public async Task evaluate_less_than_operator_returns_false_for_incorrect_value_for_date()
        {
            //Arrange
            string contextValue = "1586277849000";
            string configuredValue = "1586105049000";
          
            string filterType = "Date";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };

            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, false);
        }
        [TestMethod]
        public async Task evaluate_less_than_operator_returns_true_for_correct_value_for_date()
        {
            //Arrange
            string contextValue = "1586105049000";
            string configuredValue = "1586277849000";
            string filterType = "Date";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };

            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, true);
        }

        [TestMethod]
        public async Task evaluate_less_than_operator_returns_true_for_correct_value_for_generic()
        {
            //Arrange
            string contextValue = "1";
            string configuredValue = "3";
            string filterType = "Generic";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };
            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, true);
        }

        [TestMethod]
        public async Task evaluate_less_than_operator_returns_false_for_incorrect_value_for_generic()
        {
            //Arrange
            string contextValue = "9";
            string configuredValue = "3";
            string filterType = "Generic";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };

            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, false);
        }
        [TestMethod]
        public async Task evaluate_less_than_operator_returns_true_for_correct_value_for_role()
        {
            //Arrange
            string contextValue = "Designer";
            string configuredValue = "Manager";
            
            string filterType = "Role";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };
            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, true);
        }

        [TestMethod]
        public async Task evaluate_less_than_operator_returns_false_for_incorrect_value_for_role()
        {
            //Arrange
            string contextValue = "Manager";
            string configuredValue = "Designer";
            string filterType = "Role";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };
            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, false);
        }
        [TestMethod]
        public async Task evaluate_less_than_operator_returns_true_for_correct_value_for_region()
        {
            //Arrange
            string contextValue = "abc";
            string configuredValue = "def";
            string filterType = "Region";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };
            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, true);
        }

        [TestMethod]
        public async Task evaluate_less_than_operator_returns_false_for_incorrect_value_for_region()
        {
            //Arrange
            string contextValue = "def";
            string configuredValue = "abc";
            string filterType = "Region";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };

            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, false);
        }
        [TestMethod]
        public async Task evaluate_less_than_operator_returns_true_for_correct_value_for_roleGroup()
        {
            //Arrange
            string contextValue = "abc";
            string configuredValue = "def";
            string filterType = "RoleGroup";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };

            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, true);
        }

        [TestMethod]
        public async Task evaluate_less_than_operator_returns_false_for_incorrect_value_for_roleGroup()
        {
            //Arrange
            string contextValue = "def";
            string configuredValue = "abc";
            string filterType = "RoleGroup";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };

            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, false);
        }
    }
}
