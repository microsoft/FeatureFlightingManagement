using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Api.Controllers;
using Microsoft.FeatureFlighting.Domain.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Api.Tests.ControllerTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("ConfigurationController")]
    [TestClass]
    public class ConfigurationControllerTests
    {   

        [TestMethod]
        public void Get_Operators_Must_Return_List_Of_Operators()
        {
            var configMock = SetConfigurationMock();

            ConfigurationController controller = new ConfigurationController(configMock, null);


            var operators = controller.GetOperators() as OkObjectResult;
            Assert.IsNotNull(operators);
        }

        [TestMethod]
        public void Get_Filters_Must_Return_List_Of_Filters()
        {
            var configMock = SetConfigurationMock();

            ConfigurationController controller = new ConfigurationController(configMock, null);


            var operators = controller.GetFilters() as OkObjectResult;
            Assert.IsNotNull(operators);
        }
        [TestMethod]
        public void Get_Filter_Operator_Mapping_Must_Return_List_Of_Filter_Operator_Mapping()
        {
            var configMock = SetConfigurationMock();
            var mockOperatorevaluatorStrategy = SetOpeartorEvaluationStrategy();
            ConfigurationController controller = new ConfigurationController(configMock, mockOperatorevaluatorStrategy.Object);


            var mapping = controller.GetFilterOperatorMapping() as OkObjectResult;
            Assert.IsNotNull(mapping);
        }

        public IConfiguration SetConfigurationMock()
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            IConfiguration configuration = new ConfigurationBuilder()
               .AddInMemoryCollection(keyValuePairs)
               .Build();

            return configuration;
        }
        public Mock<IOperatorEvaluatorStrategy> SetOpeartorEvaluationStrategy()
        {
            List<string> listOfOps = new List<string>() { "equals" };
            Dictionary<string, List<string>> mapping = new Dictionary<string, List<string>>() { { "alais", listOfOps } };
            Mock<IOperatorEvaluatorStrategy> mockOperatorevaluatorStrategy = new Mock<IOperatorEvaluatorStrategy>();
            mockOperatorevaluatorStrategy.Setup(m => m.GetFilterOperatorMapping()).Returns(mapping);
            
            return mockOperatorevaluatorStrategy;
        }
    }
}
