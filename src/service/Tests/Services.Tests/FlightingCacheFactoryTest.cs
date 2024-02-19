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
using System.Transactions;


namespace Microsoft.FeatureFlighting.Infrastructure.Tests
{
    [TestClass]
    public class FlightingCacheFactoryTest
    {
        private ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly IConfiguration _configuration;
        //  private IConfiguration _mockConfiguration;
        private FlightingCacheFactory Setup()
        {
            var _tenantConfigurationProvider = new Mock<ITenantConfigurationProvider>();
            var _configuration = new Mock<IConfiguration>();
            var _logger = new Mock<ILogger>();
            var _memoryCache = new Mock<IMemoryCache>();


            return new FlightingCacheFactory(_memoryCache.Object, _tenantConfigurationProvider.Object, _configuration.Object, _logger.Object);
        }

        public void Create()
        {
            var _cacheFactory = new Mock<IFeatureFlightResultCacheFactory>();
            _cacheFactory.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

        }   
    }
}
