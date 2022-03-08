using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Api.Controllers;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

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

            ConfigurationController controller = new ConfigurationController(null, configMock);


            var operators = controller.GetOperators() as OkObjectResult;
            Assert.IsNotNull(operators);
        }

        [TestMethod]
        public async Task Get_Filters_Must_Return_List_Of_Filters()
        {
            var configMock = SetConfigurationMock();
            var mockOperatorevaluatorStrategy = SetOpeartorEvaluationStrategy();
            ConfigurationController controller = new ConfigurationController(mockOperatorevaluatorStrategy.Object, configMock);


            var operators = (await controller.GetFilters()) as OkObjectResult;
            Assert.IsNotNull(operators);
        }
        [TestMethod]
        public async Task Get_Filter_Operator_Mapping_Must_Return_List_Of_Filter_Operator_Mapping()
        {
            var configMock = SetConfigurationMock();
            var mockOperatorevaluatorStrategy = SetOpeartorEvaluationStrategy();
            ConfigurationController controller = new ConfigurationController(mockOperatorevaluatorStrategy.Object, configMock);


            var mapping = (await controller.GetFilterOperatorMapping()) as OkObjectResult;
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
        public Mock<IOperatorStrategy> SetOpeartorEvaluationStrategy()
        {
            List<string> listOfOps = new List<string>() { "equals" };
            IDictionary<string, List<string>> mapping = new Dictionary<string, List<string>>() { { "alais", listOfOps } };
            Mock<IOperatorStrategy> mockOperatorevaluatorStrategy = new Mock<IOperatorStrategy>();
            mockOperatorevaluatorStrategy.Setup(m => m.GetFilterOperatorMapping(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(mapping));
            
            return mockOperatorevaluatorStrategy;
        }
    }
}
