using AppInsights.EnterpriseTelemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Authorization;
using Microsoft.FeatureFlighting.Common.Cache;
using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Infrastructure.Authentication;
using Microsoft.FeatureFlighting.Infrastructure.Authorization;
using Microsoft.FeatureFlighting.Infrastructure.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Infrastructure.Tests.CacheTest
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FeatureFlightResultCacheFactoryTest
    {
        private Mock<ILogger> _mockLogger;
        private Mock<IMemoryCache> _mockMemoryCache;
        private Mock<IFeatureFlightResultCacheFactory> _mockFeatureFlightResultCacheFactory;

        private FeatureFlightResultCacheFactory _factory;

        public FeatureFlightResultCacheFactoryTest()
        {
            _mockLogger = new Mock<ILogger>();
            _mockMemoryCache = new Mock<IMemoryCache>();

            _factory = new FeatureFlightResultCacheFactory(_mockMemoryCache.Object, _mockLogger.Object);

            _mockFeatureFlightResultCacheFactory = new Mock<IFeatureFlightResultCacheFactory>();
        }


        [TestMethod]
        public void Create()
        {
            Mock<ICache> cacheMock = new Mock<ICache>();
            _mockFeatureFlightResultCacheFactory.Setup(feature => feature.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(cacheMock.Object);
            var result = _mockFeatureFlightResultCacheFactory.Object.Create("tenant", "operation", "121242324", "sdfsfdf");
            Assert.IsNotNull(result);
        }

    }
}