using System;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Common.Cache
{
    /// <summary>
    /// Parameters for background caching
    /// </summary>
    public class BackgroundCacheParameters
    {
        public string CacheKey { get; set; }
        public string ObjectId { get; set; }
        public string Tenant { get; set; }
        public int CacheDuration { get; set; }
        public Dictionary<string, string> AdditionalParmeters { get; set; } = null;
        public DateTime NextRebuildTimestamp { get; private set; }

        /// <summary>
        /// Updates the nexr re-build timestamp
        /// </summary>
        public void UpdateRebuildTimestamp()
        {
            if (CacheDuration > 0)
                NextRebuildTimestamp = DateTime.UtcNow.AddMinutes(CacheDuration);
            else
                NextRebuildTimestamp = DateTime.MaxValue;
        }

        /// <summary>
        /// Verifies if the cache should be re-built
        /// </summary>
        public bool ShouldRebuildCache(int gracePeriod)
        {
            DateTime graceTimestamp = DateTime.UtcNow.AddMinutes(gracePeriod);
            return NextRebuildTimestamp <= graceTimestamp;
        }
    }
}
