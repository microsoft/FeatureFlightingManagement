using AppInsights.EnterpriseTelemetry;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Core;
using Microsoft.FeatureFlighting.Core.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PS.FlightingService.Core.Tests.ServicesTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("FeatureFlagEvaluator")]
    [TestClass]
    public class FeatureFlagEvaluatorTest
    {
        private readonly Mock<IEvaluationStrategyBuilder> _mockStrategyBuilder;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<ITenantConfigurationProvider> _mockTenantConfigurationProvider;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly FeatureFlagEvaluator _featureFlagEvaluator;

        public FeatureFlagEvaluatorTest()
        {
            _mockStrategyBuilder = new Mock<IEvaluationStrategyBuilder>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockTenantConfigurationProvider = new Mock<ITenantConfigurationProvider>();
            _mockLogger = new Mock<ILogger>();
            _mockConfiguration = new Mock<IConfiguration>();

            // ...

            var configurationSectionMock = new Mock<IConfigurationSection>();

            // Mock GetChildren() to return a list of IConfigurationSection
            configurationSectionMock.Setup(x => x.GetChildren())
                .Returns(new List<IConfigurationSection> {
        new ConfigurationSection(new ConfigurationRoot(new List<IConfigurationProvider>()), "Child1"),
        new ConfigurationSection(new ConfigurationRoot(new List<IConfigurationProvider>()), "Child2")
                });
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["x-application"] = "test-Tenant";
            httpContext.Request.Headers["x-environment"] = "preprop";
            httpContext.Request.Headers["X-FlightContext"] = "test";

            _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            // Mock GetSection() to return the mocked IConfigurationSection
            _mockConfiguration.Setup(x => x.GetSection("FeatureManagement"))
                .Returns(configurationSectionMock.Object);

            // Now you can use configurationMock.Object in your tests

            // Assuming that FeatureFlagEvaluator has no dependencies
            _featureFlagEvaluator = new FeatureFlagEvaluator(_mockStrategyBuilder.Object,_mockHttpContextAccessor.Object,_mockTenantConfigurationProvider.Object,_mockLogger.Object,_mockConfiguration.Object);
        }

        [TestMethod]
        public async Task Evaluate_WithValidInputs_ShouldReturnExpectedResults()
        {
            // Arrange
            string applicationName = "TestApp";
            string environment = "TestEnv";
            List<string> featureFlags = new List<string> { "Feature1_test", "Feature2_test" };
            var _mockEvaluationStrategy = new Mock<IEvaluationStrategy>();

            var featureEvaluationResults = new Dictionary<string, bool> { { "Feature1_test", true }, { "Feature2_test", false } };

            _mockStrategyBuilder.Setup(s => s.GetStrategy(It.IsAny<List<string>>(), It.IsAny<TenantConfiguration>())).Returns(_mockEvaluationStrategy.Object);

            _mockEvaluationStrategy.Setup(e => e.Evaluate(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<TenantConfiguration>(), It.IsAny<string>(), It.IsAny<EventContext>())).Returns(Task.FromResult<IDictionary<string, bool>>(featureEvaluationResults));

            _mockTenantConfigurationProvider.Setup(t=>t.GetAllTenants()).Returns(GetTenantConfigurations());

            // Act
            var result = await _featureFlagEvaluator.Evaluate(applicationName, environment, featureFlags);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        public async Task Evaluate_ShouldReturnEmptyResults_when_empty_features(string feature)
        {
            // Arrange
            string applicationName = "TestApp";
            string environment = "TestEnv";
            List<string> featureFlags;
            if (feature == null)
            {
                featureFlags =null;
            }
            else
            {
                featureFlags = new List<string> { };
            }
            var _mockEvaluationStrategy = new Mock<IEvaluationStrategy>();

            var featureEvaluationResults = new Dictionary<string, bool> { { "Feature1_test", true }, { "Feature2_test", false } };

            _mockStrategyBuilder.Setup(s => s.GetStrategy(It.IsAny<List<string>>(), It.IsAny<TenantConfiguration>())).Returns(_mockEvaluationStrategy.Object);

            _mockEvaluationStrategy.Setup(e => e.Evaluate(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<TenantConfiguration>(), It.IsAny<string>(), It.IsAny<EventContext>())).Returns(Task.FromResult<IDictionary<string, bool>>(featureEvaluationResults));

            _mockTenantConfigurationProvider.Setup(t => t.GetAllTenants()).Returns(GetTenantConfigurations());

            // Act
            var result = await _featureFlagEvaluator.Evaluate(applicationName, environment, featureFlags);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }


        [TestMethod]
        public async Task Evaluate_Success()
        {
            // Arrange
            string applicationName = "TestApp";
            string environment = "TestEnv";
            List<string> featureFlags = new List<string> { "T1__test", "T2_test" };

            var _mockEvaluationStrategy = new Mock<IEvaluationStrategy>();

            var featureEvaluationResults = new Dictionary<string, bool> { { "Feature1_test", true }, { "Feature2_test", false } };

            _mockStrategyBuilder.Setup(s => s.GetStrategy(It.IsAny<List<string>>(), It.IsAny<TenantConfiguration>())).Returns(_mockEvaluationStrategy.Object);

            _mockEvaluationStrategy.Setup(e => e.Evaluate(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<TenantConfiguration>(), It.IsAny<string>(), It.IsAny<EventContext>())).Returns(Task.FromResult<IDictionary<string, bool>>(featureEvaluationResults));

            _mockTenantConfigurationProvider.Setup(t => t.GetAllTenants()).Returns(GetTenantConfigurations());

            // Act
            var result = await _featureFlagEvaluator.Evaluate(applicationName, environment, featureFlags);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);
        }

        [TestMethod]
        public async Task Evaluate_WithNoFeatureFlags_ShouldReturnEmptyResults()
        {
            // Arrange
            string applicationName = "TestApp";
            string environment = "TestEnv";
            List<string> featureFlags = new List<string> { "Feature1_test", "Feature2_test" };
            var _mockEvaluationStrategy = new Mock<IEvaluationStrategy>();

            var featureEvaluationResults = new Dictionary<string, bool> { };

            _mockStrategyBuilder.Setup(s => s.GetStrategy(It.IsAny<List<string>>(), It.IsAny<TenantConfiguration>())).Returns(_mockEvaluationStrategy.Object);

            _mockEvaluationStrategy.Setup(e => e.Evaluate(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<TenantConfiguration>(), It.IsAny<string>(), It.IsAny<EventContext>())).Returns(Task.FromResult<IDictionary<string, bool>>(featureEvaluationResults));

            _mockTenantConfigurationProvider.Setup(t => t.GetAllTenants()).Returns(GetTenantConfigurations());

            // Act
            var result = await _featureFlagEvaluator.Evaluate(applicationName, environment, featureFlags);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count,0);
        }

        private IEnumerable<TenantConfiguration> GetTenantConfigurations()
        {
            return new List<TenantConfiguration>
            {
                new TenantConfiguration()
                {
                    Name = "TestApp",
                    Contact="32323232323",
                    IsDyanmic=true,
                    ShortName="Test",
                    Evaluation=new FlagEvaluationConfiguration{ 
                    
                        AddDisabledContext=true,
                        AddEnabledContext=true,
                        IgnoreException=false,
                        ParallelEvaluation=new ParallelEvaluationConfiguration{ 
                        BatchSize=10,
                        ParallelMode="run parallely"
                        },
                    }
                },
                                new TenantConfiguration()
                {
                    Name = "T1",
                    Contact="32323232323",
                    IsDyanmic=true,
                    ShortName="Test",
                    Evaluation=new FlagEvaluationConfiguration{

                        AddDisabledContext=true,
                        AddEnabledContext=true,
                        IgnoreException=false,
                        ParallelEvaluation=new ParallelEvaluationConfiguration{
                        BatchSize=10,
                        ParallelMode="run parallely"
                        },
                    }
                },
                                                new TenantConfiguration()
                {
                    Name = "Test2",
                    Contact="32323232323",
                    IsDyanmic=true,
                    ShortName="Test",
                }
            };
        }
    }
}
