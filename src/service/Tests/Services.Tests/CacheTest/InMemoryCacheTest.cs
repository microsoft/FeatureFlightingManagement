using AppInsights.EnterpriseTelemetry;
using Castle.DynamicProxy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Infrastructure.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Infrastructure.Tests.CacheTest
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class InMemoryCacheTest
    {
        private Mock<IMemoryCache> _mockMemoryCache;
        private IMemoryCache _memoryCache;
        private Mock<ILogger> _mockLogger;
        private string tenant;
        private InMemoryCache inMemoryCache;
        public InMemoryCacheTest() {

            tenant = "test";
            _mockLogger = new Mock<ILogger>();
            _mockMemoryCache = new Mock<IMemoryCache>();
            //_mockMemoryCache.Setup(m => m.CreateEntry(It.IsAny<Object>())).Returns(new ICacheEntry);
            inMemoryCache = new InMemoryCache(_mockMemoryCache.Object,tenant,_mockLogger.Object);
        }

        [TestMethod]
        public async Task Get_NoData()
        {
            string? keyPayload = null;
            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockMemoryCache.Setup(mc => mc.CreateEntry(It.IsAny<object>()))
        .Callback((object k) => keyPayload = (string)k)
        .Returns(mockCacheEntry.Object);
            inMemoryCache = new InMemoryCache(_mockMemoryCache.Object, tenant, _mockLogger.Object);
            var result = await inMemoryCache.Get<bool>("1", "1212n2bn1b2", "212121");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetList_NoData()
        {
            string? keyPayload = null;
            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockMemoryCache.Setup(mc => mc.CreateEntry(It.IsAny<object>()))
            .Callback((object k) => keyPayload = (string)k)
            .Returns(mockCacheEntry.Object);
            inMemoryCache = new InMemoryCache(_mockMemoryCache.Object, tenant, _mockLogger.Object);
            var result = await inMemoryCache.GetList("1", "1212n2bn1b2", "212121");
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetListObject_NoData()
        {
            string? keyPayload = null;
            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockMemoryCache.Setup(mc => mc.CreateEntry(It.IsAny<object>()))
            .Callback((object k) => keyPayload = (string)k)
            .Returns(mockCacheEntry.Object);
            inMemoryCache = new InMemoryCache(_mockMemoryCache.Object, tenant, _mockLogger.Object);
            var result = await inMemoryCache.GetListObject<TenantConfiguration>("1", "1212n2bn1b2", "212121");
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task Delete_NoData()
        {
            string? keyPayload = null;
            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockMemoryCache.Setup(mc => mc.CreateEntry(It.IsAny<object>()))
            .Callback((object k) => keyPayload = (string)k)
            .Returns(mockCacheEntry.Object);
            inMemoryCache = new InMemoryCache(_mockMemoryCache.Object, tenant, _mockLogger.Object);
            var result=inMemoryCache.Delete("1", "1212n2bn1b2", "212121").IsCompleted;
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Set_NoData()
        {
            TenantConfiguration tenantConfiguration = GetTenantConfiguration();
            string? keyPayload = null;
            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockMemoryCache.Setup(mc => mc.CreateEntry(It.IsAny<object>()))
            .Callback((object k) => keyPayload = (string)k)
            .Returns(mockCacheEntry.Object);
            inMemoryCache = new InMemoryCache(_mockMemoryCache.Object, tenant, _mockLogger.Object);
            var result = inMemoryCache.Set<TenantConfiguration>("1", tenantConfiguration,"1212n2bn1b2", "212121").IsCompleted;
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task SetList_NoData()
        {
            TenantConfiguration tenantConfiguration = GetTenantConfiguration();
            string? keyPayload = null;
            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockMemoryCache.Setup(mc => mc.CreateEntry(It.IsAny<object>()))
            .Callback((object k) => keyPayload = (string)k)
            .Returns(mockCacheEntry.Object);
            inMemoryCache = new InMemoryCache(_mockMemoryCache.Object, tenant, _mockLogger.Object);
            var result = inMemoryCache.SetList("1", new List<string> { "tenant1", "tenant2" , "tenant3" }, "1212n2bn1b2", "212121").IsCompleted;
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task SetListObjects_NoData()
        {
            TenantConfiguration tenantConfiguration = GetTenantConfiguration();
            string? keyPayload = null;
            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockMemoryCache.Setup(mc => mc.CreateEntry(It.IsAny<object>()))
            .Callback((object k) => keyPayload = (string)k)
            .Returns(mockCacheEntry.Object);
            inMemoryCache = new InMemoryCache(_mockMemoryCache.Object, tenant, _mockLogger.Object);
            var result = inMemoryCache.SetListObjects<string>("1", new List<string> { "tenant1", "tenant2", "tenant3" }, "1212n2bn1b2", "212121").IsCompleted;
            Assert.IsTrue(result);
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
