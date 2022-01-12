using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Common.Model;

namespace Microsoft.FeatureFlighting.Common.Storage
{
    /// <summary>
    /// Factory to create Flights database repository
    /// </summary>
    public interface IFlightsDbRepositoryFactory
    {
        /// <summary>
        /// Creates <see cref="IDocumentRepository{FeatureFlight}"/> for the given tenant
        /// </summary>
        /// <param name="tenantName">Name of the tenant</param>
        /// <returns cref="IDocumentRepository{FeatureFlight}">Flights database of the tenant. Null if flights DB is not configured for the tenant.</returns>
        Task<IDocumentRepository<FeatureFlightDto>?> GetFlightsRepository(string tenantName);
    }
}
