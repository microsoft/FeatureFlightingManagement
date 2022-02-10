using System.Linq;
using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common.Config;

namespace Microsoft.FeatureFlighting.Core.Queries
{
    /// <summary>
    /// Handles <see cref="GetRegisteredTenantsQuery"/>
    /// </summary>
    internal class GetRegisteredTenantsQueryHandler : QueryHandler<GetRegisteredTenantsQuery, IEnumerable<TenantConfiguration>>
    {
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;

        public GetRegisteredTenantsQueryHandler(ITenantConfigurationProvider tenantConfigurationProvider)
        {
            _tenantConfigurationProvider = tenantConfigurationProvider;
        }

        /// <summary>
        /// Gets all tenants from configuration cache
        /// </summary>
        /// <param name="query" cref="GetRegisteredTenantsQuery">Query</param>
        /// <returns cref="IEnumerable{TenantConfiguration}">Collection of tenant configuration</returns>
        protected override async Task<IEnumerable<TenantConfiguration>> ProcessRequest(GetRegisteredTenantsQuery query)
        {
            IEnumerable<TenantConfiguration> tenants = await Task.Run(() => _tenantConfigurationProvider.GetAllTenants());
            tenants = tenants.Distinct(TenantConfigurationComparer.Default as IEqualityComparer<TenantConfiguration>);
            if (!query.IncludeDynamicTenants)
                tenants = tenants.Where(tenant => !tenant.IsDyanmic).ToList();
            return tenants;
        }
    }
}
