using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Operators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Core.Tests.OperatorTests
{
    [TestClass]
    public class NotInEvaluatorTests
    {

        private NotInOperator evaluator;
        private string[] listOfFilters;

        [TestInitialize]
        public void TestStartup()
        {
            evaluator = new NotInOperator();
            listOfFilters = evaluator.SupportedFilters;
        }
        [TestMethod]
        public async Task evaluate_not_in_operator_returns_true_for_null_value_for_alias()
        {
            //Arrange
            string contextValue = "twsharma";
            string configuredValue = "";
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
        public async Task evaluate_not_in_operator_returns_false_for_incorrect_value_for_alias()
        {
            //Arrange
            string contextValue = "twsharma";
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
            Assert.AreEqual(evaluationResult.Result, false);
        }

        [TestMethod]
        public async Task evaluate_not_in_operator_returns_true_for_correct_value_for_alias()
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
        public async Task evaluate_not_in_operator_returns_true_for_correct_value_for_country()
        {
            //Arrange
            string contextValue = "India";
            string configuredValue = "Bcd";
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
        public async Task evaluate_not_in_operator_returns_false_for_incorrect_value_for_country()
        {
            //Arrange
            string contextValue = "India";
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
        public async Task evaluate_not_in_operator_returns_true_for_correct_value_for_upn()
        {
            //Arrange
            string contextValue = "twsharma@microsoft.com";
            string configuredValue = "jgh";
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
        public async Task evaluate_not_in_operator_returns_false_for_incorrect_value_for_upn()
        {
            //Arrange
            string contextValue = "twsharma@microsoft.com";
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
            Assert.AreEqual(evaluationResult.Result, false);
        }
       
        [TestMethod]
        public async Task evaluate_not_in_operator_returns_true_for_correct_value_for_generic()
        {
            //Arrange
            string contextValue = "3";
            string configuredValue = "9";
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
        public async Task evaluate_not_in_operator_returns_false_for_incorrect_value_for_generic()
        {
            //Arrange
            string contextValue = "1";
            string configuredValue = "1";
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
        public async Task evaluate_not_in_operator_returns_true_for_correct_value_for_role()
        {
            //Arrange
            string contextValue = "Manager";
            string configuredValue = "dsa";
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
        public async Task evaluate_not_in_operator_returns_false_for_incorrect_value_for_role()
        {
            //Arrange
            string contextValue = "Designer";
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
        public async Task evaluate_not_in_operator_returns_true_for_correct_value_for_region()
        {
            //Arrange
            string contextValue = "local";
            string configuredValue = "global";
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
        public async Task evaluate_not_in_operator_returns_false_for_incorrect_value_for_region()
        {
            //Arrange
            string contextValue = "abc";
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
        public async Task evaluate_not_in_operator_returns_true_for_correct_value_for_roleGroup()
        {
            //Arrange
            string contextValue = "abc";
            string configuredValue = "ghe";
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
        public async Task evaluate_not_in_operator_returns_false_for_incorrect_value_for_roleGroup()
        {
            //Arrange
            string contextValue = "abc";
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
