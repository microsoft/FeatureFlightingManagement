using Microsoft.FeatureFlighting.Common.Cache;
using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Services.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PS.FlightingService.Core.Tests.ServicesTest.CacheTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("FeatureFlightResultCache")]
    [TestClass]
    public class FeatureFlightResultCacheTest
    {
        private readonly Mock<IFeatureFlightResultCacheFactory> _cacheFactoryMock;
        private readonly FeatureFlightResultCache _featureFlightResultCache;

        public FeatureFlightResultCacheTest()
        {
            _cacheFactoryMock = new Mock<IFeatureFlightResultCacheFactory>();
            _featureFlightResultCache = new FeatureFlightResultCache(_cacheFactoryMock.Object);
        }

        [TestMethod]
        public async Task GetFeatureFlightResults_WithValidInputs_ShouldReturnExpectedResults()
        {
            // Arrange
            string tenant = "TestTenant";
            string environment = "TestEnv";
            var trackingIds = new LoggerTrackingIds();
            var cacheMock = new Mock<ICache>();
            var expectedResults = new List<KeyValuePair<string, bool>> { new KeyValuePair<string, bool>("TestFeature", true) };
            cacheMock.Setup(c => c.GetListObject<KeyValuePair<string, bool>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(expectedResults);
            _cacheFactoryMock.Setup(cf => cf.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(cacheMock.Object);

            // Act
            var result = await _featureFlightResultCache.GetFeatureFlightResults(tenant, environment, trackingIds);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResults, result);
        }

        [TestMethod]
        public async Task SetFeatureFlightResult_WithValidInputs_ShouldNotThrowException()
        {
            // Arrange
            string tenant = "TestTenant";
            string environment = "TestEnv";
            var trackingIds = new LoggerTrackingIds();
            var featureFlightResult = new KeyValuePair<string, bool>("TestFeature", true);
            var cacheMock = new Mock<ICache>();
            _cacheFactoryMock.Setup(cf => cf.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(cacheMock.Object);

            // Act
            await _featureFlightResultCache.SetFeatureFlightResult(tenant, environment, featureFlightResult, null, trackingIds);

            // Assert
            cacheMock.Verify(c => c.SetListObjects(It.IsAny<string>(), It.IsAny<IList<KeyValuePair<string, bool>>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }
    }
}
