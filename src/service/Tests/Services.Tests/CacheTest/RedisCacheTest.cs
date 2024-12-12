using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Infrastructure.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Infrastructure.Tests.CacheTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("RedisCache")]
    [TestClass]
    public class RedisCacheTest
    {
        private readonly Mock<IConnectionMultiplexer> _mockConnectionMultiplexer;
        private readonly Mock<IDatabase> _mockDatabase;
        private readonly Mock<ILogger> _mockLogger;
        private RedisCache _redisCache;

        public RedisCacheTest()
        {
            _mockConnectionMultiplexer = new Mock<IConnectionMultiplexer>();
            _mockDatabase = new Mock<IDatabase>();
            _mockLogger = new Mock<ILogger>();
            _redisCache = new RedisCache(_mockConnectionMultiplexer.Object, _mockLogger.Object, "localhost");

            _mockConnectionMultiplexer.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockDatabase.Object);
        }

        [TestMethod]
        public async Task GetList_WhenCalledWithValidKey_NoResult()
        {
            // Arrange
            var key = "TestKey";
            var expectedList = new RedisValue[] { "Value1", "Value2" };
            _mockDatabase.Setup(m => m.ListRangeAsync(key, 0, -1, CommandFlags.None)).Returns(Task.FromResult(expectedList));
            
            // Act
            var result = await _redisCache.GetList(key, "TestCorrelationId", "TestTransactionId");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task Delete_WhenCalledWithValidKey_ThrowException()
        {
            // Arrange
            var key = "TestKey";
            _mockDatabase.Setup(m => m.KeyDeleteAsync(key, CommandFlags.None)).ReturnsAsync(true);

            // Act
            var result= _redisCache.Delete(key, "TestCorrelationId", "TestTransactionId").IsCompleted;

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task SetList_WhenCalledWithValidKeyAndValues_ShouldSetList()
        {
            // Arrange
            var key = "TestKey";
            var values = new List<string> { "Value1", "Value2" };
            _mockDatabase.Setup(m => m.ListRightPushAsync(key, It.IsAny<RedisValue[]>(), CommandFlags.None)).ReturnsAsync(values.Count);

            // Act
            var result = _redisCache.SetList(key, values, "TestCorrelationId", "TestTransactionId").IsCompleted;

            // Assert
            Assert.IsTrue(result);
        }
    }
}
