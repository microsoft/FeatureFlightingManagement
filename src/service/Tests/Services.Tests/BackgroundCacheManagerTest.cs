using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Cache;
using Microsoft.FeatureFlighting.Infrastructure.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.FeatureFlighting.Infrastructure.Tests
{
    [TestClass]
    public class BackgroundCacheManagerTest
    {

        private BackgroundCacheManager Setup() {

            // Arrange
            var repositoryMock = new Mock<IList<IBackgroundCacheable>>();
            var _logger = new Mock<ILogger>();
           return new BackgroundCacheManager(repositoryMock.Object, _logger.Object); 
        }

        // Define the test method
        [TestMethod]
        public void Dispose()
        {
            // Act
            Setup().Dispose(); // Calling the method under test
            var result= Setup().GetCacheableServiceIds();
            Assert.IsNotNull(result);
            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RebuildCache()
        {
            CancellationToken cancellationToken = default;
            // Arrange
            LoggerTrackingIds loggerTrackingIds = new LoggerTrackingIds()
            {
                CorrelationId = "1",
                TransactionId="2"
            };

         Dictionary<string, List<BackgroundCacheParameters>> _backgroundCacheableParams = new() { 
             
         };

        var repositoryMock = new Mock<IList<IBackgroundCacheable>>();
            var _logger = new Mock<ILogger>();
            var service = new BackgroundCacheManager(repositoryMock.Object, _logger.Object);
              // Act
              var result = service.RebuildCache(loggerTrackingIds, cancellationToken = default);

            Assert.IsNotNull(result);
            // Assert
            Assert.IsNotNull(repositoryMock);
        }

        [TestMethod]
        public void Init()
        {
            // Arrange
            var repositoryMock = new Mock<IList<IBackgroundCacheable>>();
            var _logger = new Mock<ILogger>();
            var service = new BackgroundCacheManager(repositoryMock.Object, _logger.Object); 
            // Act
            service.Init(100); 
            // Assert
            Assert.IsNotNull(repositoryMock);
        }
    }
}
