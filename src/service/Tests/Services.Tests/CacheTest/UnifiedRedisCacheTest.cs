using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Infrastructure.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Infrastructure.Tests.CacheTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("UnifiedRedisCache")]
    [TestClass]
    public class UnifiedRedisCacheTest
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly UnifiedRedisCache _unifiedRedisCache;

        public UnifiedRedisCacheTest()
        {
            _mockLogger = new Mock<ILogger>();
            _unifiedRedisCache = new UnifiedRedisCache("TestCluster", "TestApp", "TestAppSecret", "TestLocation", _mockLogger.Object);
        }
    }
}
