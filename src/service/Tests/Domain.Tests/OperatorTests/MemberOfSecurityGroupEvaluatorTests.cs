using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Operators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common.AppExceptions;
using Microsoft.FeatureFlighting.Common.Group;

namespace Microsoft.FeatureFlighting.Core.Tests.OperatorTests
{
    [TestClass]
    public class MemberOfSecurityGroupOperatorTests
    {
        private MemberOfSecurityGroupOperator evaluator;
        
        private Mock<IConfiguration> mockConfig;
        private Mock<IGroupVerificationService> mockGraphApiProviderWithUpnAliasInSGException,mockGraphApiProviderWithUpnInSG, mockGraphApiProviderWithUpnNotInSG, mockGraphApiProviderWithAliasNotInSG, mockGraphApiProviderWithAliasInSG;

        [TestInitialize]
        public void TestStartup()
        {  
            mockConfig = setconfig();
            mockGraphApiProviderWithUpnInSG = SetGroupVerificationService(true,false,false);
            mockGraphApiProviderWithUpnNotInSG = SetGroupVerificationService(false,false,false);
            mockGraphApiProviderWithAliasNotInSG = SetGroupVerificationService(false,false,false);
            mockGraphApiProviderWithAliasInSG = SetGroupVerificationService(false,true,false);
            mockGraphApiProviderWithUpnAliasInSGException = SetGroupVerificationService(false, true, true);
        }

        private Mock<IGroupVerificationService> SetGroupVerificationService(bool upnInSG,bool aliasInSG, bool withException)
        {
          var mockApiProvider = new Mock<IGroupVerificationService>();
            if(withException)
            {
                Exception ex = new GraphException("IsUserAliasPartOfSecurityGroup Failed", "500");
                mockApiProvider.Setup(x => x.IsUserAliasPartOfSecurityGroup(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<LoggerTrackingIds>())).Throws(ex);
                return mockApiProvider;
            }
            mockApiProvider.Setup(x => x.IsMember(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<LoggerTrackingIds>())).
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
            evaluator = new MemberOfSecurityGroupOperator(mockGraphApiProviderWithUpnInSG.Object, mockConfig.Object);
            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, false);
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

            evaluator = new MemberOfSecurityGroupOperator(mockGraphApiProviderWithUpnInSG.Object, mockConfig.Object);
            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, false);
        }
       
     
        [TestMethod]
        public async Task evaluate_sg_operator_returns_true_for_correct_value_inSG_for_upn()
        {
            //Arrange
            string contextValue = "twsharma@microsoft.com";
            string configuredValue = "[{\"Name\":\"fxpswe\",\"ObjectId\":\"6525439e-8512-4859-bb13-5a97ba5c0ff3\"}]";
            string filterType = "Userupn";
            LoggerTrackingIds trackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "TCId",
                TransactionId = "TTId"
            };
            evaluator = new MemberOfSecurityGroupOperator(mockGraphApiProviderWithUpnInSG.Object, mockConfig.Object);
            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, true);
        }

        [TestMethod]
        public async Task evaluate_sg_operator_returns_false_for_value_not_inSG_for_upn()
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
            evaluator = new MemberOfSecurityGroupOperator(mockGraphApiProviderWithUpnNotInSG.Object, mockConfig.Object);
            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, false);
        }
        [TestMethod]
        public async Task evaluate_sg_operator_returns_true_for_correct_value_inSG_for_alias()
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
            evaluator = new MemberOfSecurityGroupOperator(mockGraphApiProviderWithAliasInSG.Object, mockConfig.Object);
            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, true);
        }

        [TestMethod]
        public async Task evaluate_sg_operator_returns_false_for_value_not_inSG_for_alais()
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
            evaluator = new MemberOfSecurityGroupOperator(mockGraphApiProviderWithAliasNotInSG.Object, mockConfig.Object);
            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            //Assert
            Assert.AreEqual(evaluationResult.Result, false);
        }
        [TestMethod]
        [ExpectedException(typeof(GraphException))]
        public async Task evaluate_sg_operator_throws_exception_whenfor_value_not_inSG_for_alais()
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
            evaluator = new MemberOfSecurityGroupOperator(mockGraphApiProviderWithUpnAliasInSGException.Object, mockConfig.Object);
            //Act
            var evaluationResult = await evaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds);
            
        }
    }
}
