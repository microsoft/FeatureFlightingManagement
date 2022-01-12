using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace Microsoft.FeatureFlighting.Common.Config
{   
    /// <inheritdoc/>
    public class TenantConfigurationProvider : ITenantConfigurationProvider
    {
        private readonly IConfiguration _configuration;
        private TenantConfiguration _defaultTenantConfiguration;
        private readonly IDictionary<string, TenantConfiguration> _configurationCache = new ConcurrentDictionary<string, TenantConfiguration>();

        /// <summary>
        /// Creates the tenant configuration from <see cref="IConfiguration"/>. Used in API.
        /// </summary>
        /// <param name="configuration">ASP.NET root configuration provider</param>
        public TenantConfigurationProvider(IConfiguration configuration)
        {
            _configuration = configuration;
            Load();
        }
        
        /// <summary>
        /// Creates the tenant configurarion provider for a single tenant. Used in SDK.
        /// </summary>
        /// <param name="setConfiguration" cref="TenantConfiguration">Tenant configurtation</param>
        public TenantConfigurationProvider(TenantConfiguration setConfiguration)
        {
            _configurationCache.Add(setConfiguration.Name.ToLowerInvariant(), setConfiguration);
            if (setConfiguration.Name.ToLowerInvariant() != setConfiguration.ShortName.ToLowerInvariant())
                _configurationCache.Add(setConfiguration.ShortName.ToLowerInvariant(), setConfiguration);
        }

        /// <inheritdoc/>
        public Task<TenantConfiguration> Get(string tenantName)
        {
            if (_configurationCache.ContainsKey(tenantName.ToLowerInvariant()))
                return Task.FromResult(_configurationCache[tenantName.ToLowerInvariant()]);

            TenantConfiguration tenantConfiguration = (TenantConfiguration)_defaultTenantConfiguration.Clone();
            tenantConfiguration.Name = tenantName;
            tenantConfiguration.ShortName = tenantName;
            _configurationCache.Add(tenantName.ToLowerInvariant(), tenantConfiguration);
            return Task.FromResult(tenantConfiguration);
        }

        private void Load()
        {
            IConfigurationSection tenantConfigurationSection = _configuration.GetSection("Tenants");
            IEnumerable<IConfigurationSection> rawTenantConfigurations = tenantConfigurationSection.GetChildren();
            _defaultTenantConfiguration = _configuration.GetSection("Tenants:Default").Get<TenantConfiguration>();
            if (_defaultTenantConfiguration.FlightsDatabase != null && !_defaultTenantConfiguration.FlightsDatabase.Disabled)
                _defaultTenantConfiguration.FlightsDatabase.AddPrimaryKey(_configuration);

            foreach (IConfigurationSection rawTenantConfiguration in rawTenantConfigurations)
            {
                TenantConfiguration tenantConfiguration = rawTenantConfiguration.Get<TenantConfiguration>();
                if (tenantConfiguration == null)
                    continue;

                if (string.IsNullOrWhiteSpace(tenantConfiguration.Name))
                    tenantConfiguration.Name = rawTenantConfiguration.Key;

                if (string.IsNullOrWhiteSpace(tenantConfiguration.ShortName))
                    tenantConfiguration.ShortName = tenantConfiguration.Name;

                if (tenantConfiguration.BusinessRuleEngine != null && tenantConfiguration.BusinessRuleEngine.Enabled)
                {
                    if (tenantConfiguration.BusinessRuleEngine.Storage == null)
                    {
                        tenantConfiguration.BusinessRuleEngine.Enabled = false;
                    }
                    else
                    {
                        string storageKeyLocation = tenantConfiguration.BusinessRuleEngine.Storage.StorageConnectionStringKey;
                        string storageConnectionString = _configuration.GetValue<string>(storageKeyLocation);
                        tenantConfiguration.BusinessRuleEngine.Storage.StorageConnectionString = storageConnectionString;
                    }
                }

                if (tenantConfiguration.FlightsDatabase != null)
                    tenantConfiguration.FlightsDatabase.AddPrimaryKey(_configuration);

                tenantConfiguration.MergeWithDefault(_defaultTenantConfiguration);

                _configurationCache.AddOrUpdate(tenantConfiguration.Name.ToLowerInvariant(), tenantConfiguration);
                _configurationCache.AddOrUpdate(tenantConfiguration.ShortName.ToLowerInvariant(), tenantConfiguration);
            }
        }
    }
}
