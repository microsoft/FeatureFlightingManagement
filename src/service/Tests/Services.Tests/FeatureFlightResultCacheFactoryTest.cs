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

namespace Microsoft.FeatureFlighting.Infrastructure.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FeatureFlightResultCacheFactoryTest
    {
        private FeatureFlightResultCacheFactory Setup()
        {
            var _memoryCache = new Mock<IMemoryCache>();
            var _logger = new Mock<ILogger>();

           
            return new FeatureFlightResultCacheFactory(_memoryCache.Object, _logger.Object);
        }



        //[TestMethod]
        //public void GetCurrentUserPrincipalName()
        //{
        //    Assert.IsNull(Setup());
        //}

       
        [TestMethod]
        public void Create()
        {
             var _cacheFactory= new Mock<IFeatureFlightResultCacheFactory>() ;
            _cacheFactory.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>(), It.IsAny<string>()));
        
        }

    }
}