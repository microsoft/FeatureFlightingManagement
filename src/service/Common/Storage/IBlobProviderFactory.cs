using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Common.Storage
{
    /// <summary>
    /// Factory for creating <see cref="IBlobProvider"/>
    /// </summary>
    public interface IBlobProviderFactory
    {
        /// <summary>
        /// Creates <see cref="IBlobProvider"/> for getting the Business rule engine workflows JSON files for the given tenant
        /// </summary>
        /// <param name="tenantName">Name of the tenant</param>
        /// <returns cref="IBlobProvider">Blob provider</returns>
        Task<IBlobProvider?> CreateBreWorkflowProvider(string tenantName);
    }
}
