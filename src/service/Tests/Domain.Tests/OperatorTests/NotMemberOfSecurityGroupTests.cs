using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Domain.Evaluators;
using Microsoft.FeatureFlighting.Domain.FeatureFilters;
using Microsoft.FeatureFlighting.Domain.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Services.Interfaces;
using Microsoft.FeatureFlighting.Services;
using System;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Domain.Tests.OperatorTests
{
    [TestClass]
    public class NotMemberOfSecurityGroupEvaluatorTests
    {
        private NotMemberOfSecurityGroupEvaluator evaluator;

        private Mock<IConfiguration> mockConfig;
        private Mock<IGraphApiAccessProvider> mockGraphApiProviderWithUpnInSG, mockGraphApiProviderWithUpnNotInSG, mockGraphApiProviderWithAliasNotInSG, mockGraphApiProviderWithAliasInSG;

        [TestInitialize]
        public void TestStartup()
        {

            mockConfig = setconfig();
            mockGraphApiProviderWithUpnInSG = setGraphApiAccessProvider(true, false);
            mockGraphApiProviderWithUpnNotInSG = setGraphApiAccessProvider(false, false);
            mockGraphApiProviderWithAliasNotInSG = setGraphApiAccessProvider(false, false);
            mockGraphApiProviderWithAliasInSG = setGraphApiAccessProvider(false, true);


        }

        private Mock<IGraphApiAccessProvider> setGraphApiAccessProvider(bool upnInSG, bool aliasInSG)
        {
            var mockApiProvider = new Mock<IGraphApiAccessProvider>();
            mockApiProvider.Setup(x => x.IsUserUpnPartOfSecurityGroup(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<LoggerTrackingIds>())).
                Returns(Task.FromResult(upnInSG));
            mockApiProvider.Setup(x => x.IsUserAliasPartOfSecurityGroup(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<LoggerTrackingIds>())).
               Returns(Task.FromResult(aliasInSG));
            return mockApiProvider;
        }

        private Mock<IConfiguration> setconfig()
        {
            Mock<IConfiguration> configMock = new Mock<IConfiguration>();
            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(a => a.Value).Returns("microsoft.com");
            configMock.Setup(a => a.GetSection("Authentication:AllowedUpnDomains")).Returns(configurationSection.Object);
            return configMock;
        }

        [TestMethod]
        public async Task evaluate_sg_operator_returns_false_for_incorrect_filter_value_for_upn()
        {
            //Arrange
            string contextValue = "twsharma@microsoft.com";
            string configuredValue = "";
            string filterType = "Userupn";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };
            evaluator = new NotMemberOfSecurityGroupEvaluator(mockGraphApiProviderWithUpnInSG.Object, mockConfig.Object);
            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, false);
            Assert.AreEqual(evaluationResult.Message, "No security groups are configured");
        }
        [TestMethod]
        public async Task evaluate_sg_operator_throws_exception_for_incorrect_filter_value_without_sgID_for_upn()
        {
            //Arrange
            string contextValue = "twsharma@microsoft.com";
            string configuredValue = "fxpswe";
            string filterType = "Userupn";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };

            evaluator = new NotMemberOfSecurityGroupEvaluator(mockGraphApiProviderWithUpnInSG.Object, mockConfig.Object);
            Exception ex = null;
            //Act
            try
            {
                var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            }
            catch (Exception e)
            {
                ex = e;
            }
            //Assert
            Assert.IsNotNull(ex);

        }

        [TestMethod]
        public async Task evaluate_sg_operator_returns_false_for_invalid_upn_value_for_upn()
        {
            //Arrange
            string contextValue = "twsharma@micro.com";
            string configuredValue = "[{\"Name\":\"fxpswe\",\"ObjectId\":\"6525439e-8512-4859-bb13-5a97ba5c0ff3\"}]";
            string filterType = "Userupn";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };

            evaluator = new NotMemberOfSecurityGroupEvaluator(mockGraphApiProviderWithUpnInSG.Object, mockConfig.Object);
            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, false);
            Assert.AreEqual(evaluationResult.Message, "The UPN is incorrect. Check the format and allowed domains");
        }


        [TestMethod]
        public async Task evaluate_sg_operator_returns_true_for_correct_value_NotInSG_for_upn()
        {
            //Arrange
            string contextValue = "morat@microsoft.com";
            string configuredValue = "[{\"Name\":\"fxpswe\",\"ObjectId\":\"6525439e-8512-4859-bb13-5a97ba5c0ff3\"}]";
            string filterType = "Userupn";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };
            evaluator = new NotMemberOfSecurityGroupEvaluator(mockGraphApiProviderWithUpnNotInSG.Object, mockConfig.Object);
            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, true);
        }

        [TestMethod]
        public async Task evaluate_sg_operator_returns_false_for_value_inSG_for_upn()
        {
            //Arrange
            string contextValue = "pratikb@microsoft.com";
            string configuredValue = "[{\"Name\":\"fxpswe\",\"ObjectId\":\"6525439e-8512-4859-bb13-5a97ba5c0ff3\"}]";
            string filterType = "Userupn";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };
            evaluator = new NotMemberOfSecurityGroupEvaluator(mockGraphApiProviderWithUpnInSG.Object, mockConfig.Object);
            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, false);
        }
        [TestMethod]
        public async Task evaluate_sg_operator_returns_true_for_correct_value_NotInSG_for_alias()
        {
            //Arrange
            string contextValue = "morat";
            string configuredValue = "[{\"Name\":\"fxpswe\",\"ObjectId\":\"6525439e-8512-4859-bb13-5a97ba5c0ff3\"}]";
            string filterType = "Alias";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };
            evaluator = new NotMemberOfSecurityGroupEvaluator(mockGraphApiProviderWithAliasNotInSG.Object, mockConfig.Object);
            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, true);
        }

        [TestMethod]
        public async Task evaluate_sg_operator_returns_false_for_value_inSG_for_alais()
        {
            //Arrange
            string contextValue = "twsharma";
            string configuredValue = "[{\"Name\":\"fxpswe\",\"ObjectId\":\"6525439e-8512-4859-bb13-5a97ba5c0ff3\"}]";
            string filterType = "Alias";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };
            evaluator = new NotMemberOfSecurityGroupEvaluator(mockGraphApiProviderWithAliasInSG.Object, mockConfig.Object);
            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, false);
        }

    }
}
