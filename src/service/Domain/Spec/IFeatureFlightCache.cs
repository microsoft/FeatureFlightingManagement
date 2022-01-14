using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Model;

namespace Microsoft.FeatureFlighting.Core.Spec
{
    /// <summary>
    /// Cache for feature flights
    /// </summary>
    public interface IFeatureFlightCache
    {
        /// <summary>
        /// Gets feature flights from cache
        /// </summary>
        /// <param name="tenant">Name of the tenant</param>
        /// <param name="environment">Environment</param>
        /// <param name="trackingIds" cref="LoggerTrackingIds">Tracking IDs</param>
        /// <returns cref="IEnumerable{FeatureFlightDto}">Collection of cached <see cref="FeatureFlightDto"/></returns>
        Task<IEnumerable<FeatureFlightDto>> GetFeatureFlights(string tenant, string environment, LoggerTrackingIds trackingIds);

        /// <summary>
        /// Gets the feature names from cache (not the complete feature flag, only the names of the features)
        /// </summary>
        /// <param name="tenant">Name of the tenant</param>
        /// <param name="environment">Environment</param>
        /// <param name="trackingIds" cref="LoggerTrackingIds">Tracking IDs</param>
        /// <returns cref="IEnumerable{string}">Collection of feature names</returns>
        Task<IEnumerable<string>> GetFeatureNames(string tenant, string environment, LoggerTrackingIds trackingIds);

        /// <summary>
        /// Sets feature flights in cache (both the flight and feature names are set in the cache)
        /// </summary>
        /// <param name="tenant">Name of the tenant</param>
        /// <param name="environment">Environment</param>
        /// <param name="featureFlights" cref="IEnumerable{FeatureFlightDto}">Collection of feature flights</param>
        /// <param name="trackingIds" cref="LoggerTrackingIds">Tracking IDs</param>
        Task SetFeatureFlights(string tenant, string environment, IEnumerable<FeatureFlightDto> featureFlights, LoggerTrackingIds trackingIds);

        /// <summary>
        /// Deletes feature flights from cache (deletes both the feature flags and the feature names)
        /// </summary>
        /// <param name="tenant">Name of the tenant</param>
        /// <param name="environment">Environment</param>
        /// <param name="trackingIds" cref="LoggerTrackingIds">Tracking IDs</param>
        Task DeleteFeatureFlights(string tenant, string environment, LoggerTrackingIds trackingIds);
    }
}
