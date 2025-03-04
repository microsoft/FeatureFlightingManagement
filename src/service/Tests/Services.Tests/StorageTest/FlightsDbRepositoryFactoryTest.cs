using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Cache;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Infrastructure.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;


namespace Microsoft.FeatureFlighting.Infrastructure.Tests.StorageTest
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FlightsDbRepositoryFactoryTest
    {
        private Mock<ITenantConfigurationProvider> _mockTenantConfigurationProvider;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<ILogger> _mockLogger;

        private FlightsDbRepositoryFactory _factory;

        //  private IConfiguration _mockConfiguration;
        public FlightsDbRepositoryFactoryTest()
        {
            _mockTenantConfigurationProvider = new Mock<ITenantConfigurationProvider>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger>();

            _factory = new FlightsDbRepositoryFactory(_mockTenantConfigurationProvider.Object, _mockConfiguration.Object, _mockLogger.Object);
        }



        [TestMethod]
        public async Task GetFlightsRepository_when_FlightsDatabase_is_null()
        {
            TenantConfiguration tenantConfiguration = GetTenantConfiguration();
            _mockTenantConfigurationProvider.Setup(t => t.Get(It.IsAny<string>())).Returns(Task.FromResult(tenantConfiguration));
            var result = await _factory.GetFlightsRepository("Test");
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetFlightsRepository_when_Disabled_property_of_flightsDatabase_is_true()
        {
            TenantConfiguration tenantConfiguration = GetTenantConfiguration();
            tenantConfiguration.FlightsDatabase = new CosmosDbConfiguration { Disabled = true };
            _mockTenantConfigurationProvider.Setup(t => t.Get(It.IsAny<string>())).Returns(Task.FromResult(tenantConfiguration));
            var result = await _factory.GetFlightsRepository("Test");
            Assert.IsNull(result);
        }

        private TenantConfiguration GetTenantConfiguration()
        {
            return new TenantConfiguration
            {
                Contact = "test contact",
                IsDyanmic = true,
                FlightsDatabase = null
            };
        }
    }
}
