using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Common.Storage
{
    /// <summary>
    /// Provides Azure Storage blob
    /// </summary>
    public interface IBlobProvider
    {
        /// <summary>
        /// Gets the stringified blob content from Azure Storage
        /// </summary>
        /// <param name="blobName">Name of the blob</param>
        /// <param name="trackingIds">Tracking IDs</param>
        /// <returns>Blob content</returns>
        Task<string?> Get(string blobName, LoggerTrackingIds trackingIds);
    }
}
