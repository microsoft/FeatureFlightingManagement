using Microsoft.PS.FlightingService.Common;
using Microsoft.PS.FlightingService.Domain.Evaluators;
using Microsoft.PS.FlightingService.Domain.FeatureFilters;
using Microsoft.PS.FlightingService.Domain.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Microsoft.PS.FlightingService.Domain.Tests.OperatorTests
{
    [TestClass]
    public class InEvaluatorTest
    {
        private InEvaluator evaluator;
        private string[] listOfFilters;

        [TestInitialize]
        public void TestStartup()
        {
            evaluator = new InEvaluator();
            listOfFilters = evaluator.SupportedFilters;
        }
        [TestMethod]
        public async Task evaluate_in_operator_returns_true_for_null_value_for_alias()
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
            Assert.AreEqual(evaluationResult.Result, false);
            Assert.AreEqual(evaluationResult.Message, "Configured Value is empty");
        }
        [TestMethod]
        public async Task evaluate_in_operator_returns_true_for_correct_value_for_alias()
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
            Assert.AreEqual(evaluationResult.Result, true);
        }

        [TestMethod]
        public async Task evaluate_in_operator_returns_false_for_incorrect_value_for_alias()
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
            Assert.AreEqual(evaluationResult.Result, false);
        }
        [TestMethod]
        public async Task evaluate_in_operator_returns_true_for_correct_value_for_country()
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
            Assert.AreEqual(evaluationResult.Result, true);
        }

        [TestMethod]
        public async Task evaluate_in_operator_returns_false_for_incorrect_value_for_country()
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
            Assert.AreEqual(evaluationResult.Result, false);
        }
        [TestMethod]
        public async Task evaluate_in_operator_returns_true_for_correct_value_for_upn()
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
            Assert.AreEqual(evaluationResult.Result, true);
        }

        [TestMethod]
        public async Task evaluate_in_operator_returns_false_for_incorrect_value_for_upn()
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
            Assert.AreEqual(evaluationResult.Result, false);
        }
        [TestMethod]
        
        public async Task evaluate_in_operator_returns_true_for_correct_value_for_generic()
        {
            //Arrange
            string contextValue = "3";
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
        public async Task evaluate_in_operator_returns_false_for_incorrect_value_for_generic()
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
            Assert.AreEqual(evaluationResult.Result, false);
        }
        [TestMethod]
        public async Task evaluate_in_operator_returns_true_for_correct_value_for_role()
        {
            //Arrange
            string contextValue = "Manager";
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
        public async Task evaluate_in_operator_returns_false_for_incorrect_value_for_role()
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
        public async Task evaluate_in_operator_returns_true_for_correct_value_for_region()
        {
            //Arrange
            string contextValue = "local";
            string configuredValue = "local";
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
        public async Task evaluate_in_operator_returns_false_for_incorrect_value_for_region()
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
            Assert.AreEqual(evaluationResult.Result, false);
        }
        [TestMethod]
        public async Task evaluate_in_operator_returns_true_for_correct_value_for_roleGroup()
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
            Assert.AreEqual(evaluationResult.Result, true);
        }

        [TestMethod]
        public async Task evaluate_in_operator_returns_false_for_incorrect_value_for_roleGroup()
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
            Assert.AreEqual(evaluationResult.Result, false);
        }
    }
}
