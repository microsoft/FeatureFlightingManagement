using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Cache;
using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Infrastructure.Authentication;
using Microsoft.FeatureFlighting.Infrastructure.Cache;
using Microsoft.FeatureFlighting.Infrastructure.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Transactions;


namespace Microsoft.FeatureFlighting.Infrastructure.Tests.CacheTest
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FlightingCacheFactoryTest
    {
        private Mock<ITenantConfigurationProvider> _mockTenantConfigurationProvider;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<ILogger> _mockLogger;
        private Mock<IMemoryCache> _mockMemoryCache;

        private FlightingCacheFactory _factory;
        public FlightingCacheFactoryTest()
        { 
            _mockTenantConfigurationProvider = new Mock<ITenantConfigurationProvider>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger>();
            _mockMemoryCache = new Mock<IMemoryCache>();

            _factory = new FlightingCacheFactory(_mockMemoryCache.Object, _mockTenantConfigurationProvider.Object, _mockConfiguration.Object, _mockLogger.Object);
        }

        [TestMethod]
        public void Create_success_when_cache_is_null()
        {
            var _tenantConfigurationProvider = new Mock<ITenantConfigurationProvider>();
            _mockTenantConfigurationProvider.Setup(t => t.Get(It.IsAny<string>())).Returns(Task.FromResult(GetTenantConfiguration()));
            var result = _factory.Create("tenant", "122122", "eweqwe23233");

            Assert.IsNotNull(result);
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow(null)]
        public void Create_success_when_cache_type_is_null(string cacheType)
        {
            TenantConfiguration tenantConfiguration = GetTenantConfiguration();
            tenantConfiguration.Cache = new CacheConfiguration { Type = cacheType };

            _mockTenantConfigurationProvider.Setup(t => t.Get(It.IsAny<string>())).Returns(Task.FromResult(tenantConfiguration));
            var result = _factory.Create("tenant", "122122", "eweqwe23233");

            Assert.IsNotNull(result);
        }

        [DataTestMethod]
        [DataRow("NoCache")]
        [DataRow("Redis")]
        [DataRow("")]
        [DataRow("InMemory")]
        public void Create_success_when_cache_type_is_not_null(string cacheType)
        {
            TenantConfiguration tenantConfiguration = GetTenantConfiguration();
            tenantConfiguration.Cache = new CacheConfiguration { Type = cacheType };

            _mockTenantConfigurationProvider.Setup(t => t.Get(It.IsAny<string>())).Returns(Task.FromResult(tenantConfiguration));
            var result = _factory.Create("tenant", "122122", "eweqwe23233");

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Create_success_when_redis_is_null()
        {
            TenantConfiguration tenantConfiguration = GetTenantConfiguration();
            tenantConfiguration.Cache = new CacheConfiguration { Redis = null };

            _mockTenantConfigurationProvider.Setup(t => t.Get(It.IsAny<string>())).Returns(Task.FromResult(tenantConfiguration));
            var result = _factory.Create("tenant", "122122", "eweqwe23233");

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Create_success_when_URP_is_null()
        {
            TenantConfiguration tenantConfiguration = GetTenantConfiguration();
            tenantConfiguration.Cache = new CacheConfiguration { URP = null };

            _mockTenantConfigurationProvider.Setup(t => t.Get(It.IsAny<string>())).Returns(Task.FromResult(tenantConfiguration));
            var result = _factory.Create("tenant", "122122", "eweqwe23233");

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
            };
        }
    }
}
