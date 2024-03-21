using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Core.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PS.FlightingService.Core.Tests.ServicesTest.CacheTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("FeatureFlightCache")]
    [TestClass]
    public class FeatureFlightCacheTest
    {
        private readonly Mock<ICacheFactory> _cacheFactoryMock;
        private readonly FeatureFlightCache _featureFlightCache;

        public FeatureFlightCacheTest()
        {
            _cacheFactoryMock = new Mock<ICacheFactory>();
            _featureFlightCache = new FeatureFlightCache(_cacheFactoryMock.Object);
        }

        [TestMethod]
        public async Task GetFeatureFlights_WithValidInputs_ShouldReturnExpectedResults()
        {
            // Arrange
            string tenant = "TestTenant";
            string environment = "TestEnv";
            var trackingIds = new LoggerTrackingIds();
            var cacheMock = new Mock<ICache>();
            var expectedResults = new List<FeatureFlightDto> { new FeatureFlightDto { Name = "TestFeature", Enabled = true } };
            var serializedExpectedResults = expectedResults.Select(JsonConvert.SerializeObject).ToList();
            cacheMock.Setup(c => c.GetList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(serializedExpectedResults);
            _cacheFactoryMock.Setup(cf => cf.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(cacheMock.Object);

            // Act
            var result = await _featureFlightCache.GetFeatureFlights(tenant, environment, trackingIds);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
        }

        [TestMethod]
        public async Task GetFeatureNames_WithValidInputs_ShouldReturnExpectedResults()
        {
            // Arrange
            string tenant = "TestTenant";
            string environment = "TestEnv";
            var trackingIds = new LoggerTrackingIds();
            var cacheMock = new Mock<ICache>();
            var expectedResults = new List<FeatureFlightDto> { new FeatureFlightDto { Name = "TestFeature", Enabled = true } };
            var serializedExpectedResults = expectedResults.Select(JsonConvert.SerializeObject).ToList();
            cacheMock.Setup(c => c.GetList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(serializedExpectedResults);
            _cacheFactoryMock.Setup(cf => cf.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(cacheMock.Object);

            // Act
            var result = await _featureFlightCache.GetFeatureNames(tenant, environment, trackingIds);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
        }

        [TestMethod]
        public async Task SetFeatureFlights_WithValidInputs_ShouldNotThrowException()
        {
            // Arrange
            string tenant = "TestTenant";
            string environment = "TestEnv";
            var trackingIds = new LoggerTrackingIds();
            var featureFlights = new List<FeatureFlightDto> { new FeatureFlightDto { Name = "TestFeature", Enabled = true } };
            var cacheMock = new Mock<ICache>();
            _cacheFactoryMock.Setup(cf => cf.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(cacheMock.Object);

            // Act
            await _featureFlightCache.SetFeatureFlights(tenant, environment, featureFlights, trackingIds);

            // Assert
            cacheMock.Verify(c => c.SetList(It.IsAny<string>(), It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(2));
        }

        [TestMethod]
        public async Task DeleteFeatureFlights_WithValidInputs_ShouldNotThrowException()
        {
            // Arrange
            string tenant = "TestTenant";
            string environment = "TestEnv";
            var trackingIds = new LoggerTrackingIds();
            var cacheMock = new Mock<ICache>();
            _cacheFactoryMock.Setup(cf => cf.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(cacheMock.Object);

            // Act
            await _featureFlightCache.DeleteFeatureFlights(tenant, environment, trackingIds);

            // Assert
            cacheMock.Verify(c => c.Delete(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }
    }
}
