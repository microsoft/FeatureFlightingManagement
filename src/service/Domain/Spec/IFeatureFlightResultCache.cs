using Microsoft.FeatureFlighting.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Core.Spec
{
    public interface IFeatureFlightResultCache
    {
        /// <summary>
        /// Gets the feature flight result from cache 
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="environment"></param>
        /// <param name="trackingIds"></param>
        /// <returns></returns>
        public Task<IList<KeyValuePair<string, bool>>> GetFeatureFlightResults(string tenant, string environment, LoggerTrackingIds trackingIds);

        /// <summary>
        /// Sets feature flight result on the cache
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="environment"></param>
        /// <param name="featureFlightResults"></param>
        /// <param name="trackingIds"></param>
        /// <returns></returns>
        public Task SetFeatureFlightResult(string tenant, string environment, KeyValuePair<string, bool> featureFlightResult, IList<KeyValuePair<String, bool>> cachedFlightResult, LoggerTrackingIds trackingIds);

    }
}
