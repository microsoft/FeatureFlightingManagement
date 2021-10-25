using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Core.Operators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using Microsoft.FeatureFlighting.Infrastructure.Cache;

namespace Microsoft.FeatureFlighting.Core.Tests.OperatorTests
{
    [TestClass]
    public class OperatorEvaluatorStrategyTest
    {
        private readonly IList<BaseOperator> _evaluators = new List<BaseOperator>();
        private OperatorStrategy operatorEvaluatorStrategy;
        private ITenantConfigurationProvider _tenantConfigurationProvider;
        private ICacheFactory _cacheFactory;

        [TestInitialize]
        public void TestStartUp()
        {
            BaseOperator equalOperator = new EqualOperator();
            _evaluators.Add(equalOperator);
            var mockTenantConfigurationProvider = new Mock<TenantConfigurationProvider>();
            mockTenantConfigurationProvider.Setup(provider => provider.Get(It.IsAny<string>()))
                .Returns(Task.FromResult(new TenantConfiguration()));
            _tenantConfigurationProvider = mockTenantConfigurationProvider.Object;

            var mockCacheFactory = new Mock<ICacheFactory>();
            mockCacheFactory.Setup(fac => fac.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new NoCache());
            _cacheFactory = mockCacheFactory.Object;

            operatorEvaluatorStrategy = new OperatorStrategy(_evaluators, _tenantConfigurationProvider, _cacheFactory);
        }
        [TestMethod]
        public void validate_get_for_equals_operator()
        {

            //Arrange
            Operator op = Operator.Equals;
            //Act
            var result = operatorEvaluatorStrategy.Get(op);
            //Assert
            Assert.AreEqual(result.Operator, op);
        }
        [TestMethod]
        public async Task validate_Get_Filter_Operator_Mapping_for_equals_operator()
        {
            //Act
            var result = await operatorEvaluatorStrategy.GetFilterOperatorMapping("Default", "", "");
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
