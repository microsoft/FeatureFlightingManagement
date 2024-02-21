using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Storage;
using Microsoft.FeatureFlighting.Core.RulesEngine;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RulesEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;

namespace Microsoft.PS.FlightingService.Core.Tests.RulesEngineTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("RulesEngineManager")]
    [TestClass]
    public class RulesEngineManagerTest
    {
        private Mock<IRulesEngineEvaluator> _mockRulesEngineEvaluator;
        private Mock<IOperatorStrategy> _mockOperatorEvaluatorStrategy;
        private Mock<ITenantConfigurationProvider> _mockTenantConfigurationProvider;
        private Mock<IBlobProviderFactory> _mockBlobProviderFactory;
        private Mock<ICacheFactory> _mockCacheFactory;
        private Mock<ICache> _mockCache;
        private Mock<IBlobProvider> _mockBlobProvider;
        private readonly RulesEngineManager _rulesEngineManager;

        public RulesEngineManagerTest()
        {
            _mockOperatorEvaluatorStrategy=new Mock<IOperatorStrategy>();
            _mockTenantConfigurationProvider=new Mock<ITenantConfigurationProvider>();
            _mockBlobProviderFactory=new Mock<IBlobProviderFactory>();
            _mockCacheFactory=new Mock<ICacheFactory>();
            _mockRulesEngineEvaluator = new Mock<IRulesEngineEvaluator>();
            _mockCache = new Mock<ICache>();
            _rulesEngineManager = new RulesEngineManager(_mockOperatorEvaluatorStrategy.Object,_mockTenantConfigurationProvider.Object,_mockBlobProviderFactory.Object,_mockCacheFactory.Object);
            _mockBlobProvider=new Mock<IBlobProvider>();
        }

        [TestMethod]
        public async Task Build_Should_Be_Return_Null()
        {
            // Arrange
            _mockTenantConfigurationProvider.Setup(m => m.Get(It.IsAny<string>())).ReturnsAsync(new TenantConfiguration());
            // Setup your mocks and test inputs

            // Act
            LoggerTrackingIds loggerTrackingIds = new LoggerTrackingIds() { CorrelationId = "test id", TransactionId = "t 2001" };
            var result = await _rulesEngineManager.Build("tenant1", "workflow1", loggerTrackingIds);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task Build_Should_Be_Return_cachedRuleEngine()
        {
            // Arrange
            var _rulesEngineMock = new Mock<IRulesEngine>();
            var _rulesEngineEvaluator = new RulesEngineEvaluator(_rulesEngineMock.Object, "workflow1", new TenantConfiguration());

            _mockTenantConfigurationProvider.Setup(m => m.Get(It.IsAny<string>())).ReturnsAsync(GetTenantConfiguration());
            _mockCacheFactory.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(_mockCache.Object);
            _mockCache.Setup(m => m.Get<RulesEngineEvaluator>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_rulesEngineEvaluator);

            // Act
            LoggerTrackingIds loggerTrackingIds = new LoggerTrackingIds() { CorrelationId = "test id", TransactionId = "t 2001" };
            var result = await _rulesEngineManager.Build("tenant1", "workflow1", loggerTrackingIds);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Build_Success()
        {
            // Arrange
            var jsonString = "{\"WorkflowName\":\"test name\",\"WorkflowsToInject\":[\"id1\",\"id2\"]}"; ;
            var _rulesEngineMock = new Mock<IRulesEngine>();
            var _rulesEngineEvaluator = new RulesEngineEvaluator(_rulesEngineMock.Object, "workflow1", new TenantConfiguration());

            _mockTenantConfigurationProvider.Setup(m => m.Get(It.IsAny<string>())).ReturnsAsync(GetTenantConfiguration());
            _mockCacheFactory.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new Mock<ICache>().Object);
            _mockCache.Setup(m => m.Get<RulesEngineEvaluator>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(_rulesEngineEvaluator));
            _mockTenantConfigurationProvider.Setup(m => m.Get(It.IsAny<string>())).ReturnsAsync(GetTenantConfiguration());
            _mockBlobProviderFactory.Setup(b=>b.CreateBreWorkflowProvider(It.IsAny<string>())).Returns(Task.FromResult(_mockBlobProvider.Object));
            _mockBlobProvider.Setup(b=>b.Get(It.IsAny<string>(),It.IsAny<LoggerTrackingIds>())).Returns(Task.FromResult(jsonString));

            // Act
            LoggerTrackingIds loggerTrackingIds = new LoggerTrackingIds() { CorrelationId = "test id", TransactionId = "t 2001" };
            var result = await _rulesEngineManager.Build("tenant1", "workflow1", loggerTrackingIds);

            // Assert
            Assert.IsNotNull(result);
        }

        private TenantConfiguration GetTenantConfiguration()
        {
            return new TenantConfiguration()
            {
                Name = "Test",
                Contact = "32323232323",
                IsDyanmic = true,
                ShortName = "Test",
                BusinessRuleEngine=new BusinessRuleEngineConfiguration()
                {
                    CacheDuration=100,
                    Enabled=true,
                    Storage=new StorageConfiguration()
                    {
                        StorageConnectionString="test",
                        StorageConnectionStringKey="test",
                        ContainerName="test"
                    },
                }
            };
        }
    }
}
