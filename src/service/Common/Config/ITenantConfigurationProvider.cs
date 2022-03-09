using System.Threading.Tasks;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Common.Config
{
    /// <summary>
    /// Provides configuration for tenants of the Flighting Service
    /// </summary>
    public interface ITenantConfigurationProvider
    {
        /// <summary>
        /// Gets <see cref="TenantConfiguration"/> by the tenant name (or short name). If tenant configuration is not specified then the default configuration is returned.
        /// </summary>
        /// <param name="tenantName">Tenant Name (or the short name)</param>
        /// <returns cref="TenantConfiguration">Tenant Configuration</returns>
        Task<TenantConfiguration> Get(string tenantName);

        /// <summary>
        /// Gets all tenant configurations
        /// </summary>
        /// <returns cref="IEnumerable{TenantConfiguration}">Collection of <see cref="TenantConfiguration"/></returns>
        IEnumerable<TenantConfiguration> GetAllTenants();
    }
}