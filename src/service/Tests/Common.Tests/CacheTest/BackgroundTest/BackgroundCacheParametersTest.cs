using Microsoft.FeatureFlighting.Common.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.FeatureFlighting.Common.Tests.CacheTest.BackgroundTest
{
    [ExcludeFromCodeCoverage]
    [TestCategory("BackgroundCacheParameters")]
    [TestClass]
    public class BackgroundCacheParametersTest
    {
        [TestMethod]
        public void UpdateRebuildTimestamp_ShouldSetNextRebuildTimestampCorrectly_WhenCacheDurationIsGreaterThanZero()
        {
            var parameters = new BackgroundCacheParameters { CacheDuration = 10 };

            parameters.UpdateRebuildTimestamp();

            Assert.IsTrue(parameters.NextRebuildTimestamp > DateTime.UtcNow);
            Assert.IsTrue(parameters.NextRebuildTimestamp < DateTime.UtcNow.AddMinutes(11));
        }

        [TestMethod]
        public void UpdateRebuildTimestamp_ShouldSetNextRebuildTimestampToMaxValue_WhenCacheDurationIsZero()
        {
            var parameters = new BackgroundCacheParameters { CacheDuration = 0 };

            parameters.UpdateRebuildTimestamp();

            Assert.AreEqual(DateTime.MaxValue, parameters.NextRebuildTimestamp);
        }

        [DataTestMethod]
        [DataRow(10, 5, false)]
        [DataRow(10, 15, true)]
        public void ShouldRebuildCache_ShouldReturnCorrectValue(int cacheDuration, int gracePeriod, bool expected)
        {
            var parameters = new BackgroundCacheParameters { CacheDuration = cacheDuration };
            parameters.UpdateRebuildTimestamp();

            var result = parameters.ShouldRebuildCache(gracePeriod);

            Assert.AreEqual(expected, result);
        }
    }
}
