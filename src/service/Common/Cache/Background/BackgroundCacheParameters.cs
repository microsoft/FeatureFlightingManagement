using System;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Common.Cache
{
    public class BackgroundCacheParameters
    {
        public string CacheKey { get; set; }
        public string ObjectId { get; set; }
        public string Tenant { get; set; }
        public int CacheDuration { get; set; }
        public Dictionary<string, string> AdditionalParmeters { get; set; } = null;
        public DateTime NextRecacheTimestamp { get; private set; }

        public void UpdateNextRecacheTimestamp()
        {
            if (CacheDuration > 0)
                NextRecacheTimestamp = DateTime.UtcNow.AddMinutes(CacheDuration);
            else
                NextRecacheTimestamp = DateTime.MaxValue;
        }

        public bool ShouldRecache(int gracePeriod)
        {
            DateTime graceTimestamp = DateTime.UtcNow.AddMinutes(gracePeriod);
            return NextRecacheTimestamp <= graceTimestamp;
        }
    }
}
