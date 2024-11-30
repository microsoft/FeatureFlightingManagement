using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Common.Config;
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
    [TestClass]
    public class NoCacheTest
    {
        private NoCache noCache;
        private Mock<ICache> _mockCache;
        public NoCacheTest()
        {
            noCache=new NoCache();
            _mockCache=new Mock<ICache>();
        }

        [TestMethod]
        public void Delete_Success()
        {
            var result = noCache.Delete("test", "1234", "210").IsCompleted;
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Get_NoResult()
        {
            var result = await noCache.Get<TenantConfiguration>("test", "1234", "210");
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetList_NoResult()
        {
            var result = await noCache.GetList("tested", "202", "101");
            Assert.IsNull(result);
        }


        [TestMethod]
        public async Task GetListObject_NoResult()
        {
            try
            {
                var result = await noCache.GetListObject<TenantConfiguration>("test", "1234", "210");
                
            } catch (Exception ex){
                Assert.IsNotNull(ex.Message);
            }
        }

        [TestMethod]
        public void Set_success()
        {
            TenantConfiguration configuration = GetTenantConfiguration();
            var result = noCache.Set<TenantConfiguration>("test",configuration, "1234", "210").IsCompleted;
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SetList_success()
        {
            TenantConfiguration configuration = GetTenantConfiguration();
            var result = noCache.SetList("test", new List<string> { "test1","test2"}, "1234", "210").IsCompleted;
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task SetListObject_NoResult()
        {
            try
            {
                await noCache.SetListObjects<string>("test", new List<string> { "test1", "test2" }, "1234", "210");

            }
            catch (Exception ex)
            {
                Assert.IsNotNull(ex.Message);
            }
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
