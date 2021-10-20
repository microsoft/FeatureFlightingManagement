﻿namespace Microsoft.FeatureFlighting.Infrastructure.Cache
{
    public static class CacheConstants
    {
        public enum CacheType
        {
            NoCache = 0,
            URP = 1,
            Redis = 2,
            InMemory = 3
        }
    }
}
