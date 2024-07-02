﻿using System.Threading.Tasks;
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
            tenantConfiguration.IsDyanmic = true;
            _configurationCache.Add(tenantName.ToLowerInvariant(), tenantConfiguration);
            return Task.FromResult(tenantConfiguration);
        }

        public IEnumerable<TenantConfiguration> GetAllTenants()
        {
            return _configurationCache.Values;
        }

        private void Load()
        {
            IConfigurationSection tenantConfigurationSection = _configuration.GetSection("Tenants");
            IEnumerable<IConfigurationSection> rawTenantConfigurations = tenantConfigurationSection.GetChildren();
            _defaultTenantConfiguration = _configuration.GetSection("Tenants:Default").Get<TenantConfiguration>();
            _defaultTenantConfiguration.IsDyanmic = true;
            AddFlightsDatabaseConfiguration(_defaultTenantConfiguration);
            AddChangeNotificationWebhook(_defaultTenantConfiguration);
            AddMetricsConfiguration(_defaultTenantConfiguration);

            foreach (IConfigurationSection rawTenantConfiguration in rawTenantConfigurations)
            {
                TenantConfiguration tenantConfiguration = rawTenantConfiguration.Get<TenantConfiguration>();
                if (tenantConfiguration == null)
                    continue;

                if (string.IsNullOrWhiteSpace(tenantConfiguration.Name))
                    tenantConfiguration.Name = rawTenantConfiguration.Key;

                if (string.IsNullOrWhiteSpace(tenantConfiguration.ShortName))
                    tenantConfiguration.ShortName = tenantConfiguration.Name;

                AddBusinessRuleEngineConfiguration(tenantConfiguration);
                AddFlightsDatabaseConfiguration(tenantConfiguration);
                AddChangeNotificationWebhook(tenantConfiguration);
                AddMetricsConfiguration(tenantConfiguration);
                tenantConfiguration.MergeWithDefault(_defaultTenantConfiguration);
                tenantConfiguration.IsDyanmic = false;

                _configurationCache.AddOrUpdate(tenantConfiguration.Name.ToLowerInvariant(), tenantConfiguration);
                _configurationCache.AddOrUpdate(tenantConfiguration.ShortName.ToLowerInvariant(), tenantConfiguration);
            }
        }

        private void AddFlightsDatabaseConfiguration(TenantConfiguration tenantConfiguration)
        {
            if (tenantConfiguration.FlightsDatabase != null && !tenantConfiguration.FlightsDatabase.Disabled)
            {
                tenantConfiguration.FlightsDatabase.PrimaryKey = _configuration.GetValue<string>(tenantConfiguration.FlightsDatabase.PrimaryKeyLocation);
            }
        }

        private void AddMetricsConfiguration(TenantConfiguration tenantConfiguration)
        {
            if (tenantConfiguration.Metrics == null || !tenantConfiguration.Metrics.Enabled)
                return;

            tenantConfiguration.Metrics.Kusto = _configuration.GetSection("Kusto").Get<KustoConfiguraton>();
            tenantConfiguration.Metrics.Kusto.SetDefault();            
            if (tenantConfiguration.Metrics.MetricSource == null ||
                tenantConfiguration.Metrics.MetricSource.WebhookId.ToLowerInvariant() == "KustoAPI".ToLowerInvariant())
            {
                tenantConfiguration.Metrics.MetricSource = GetWebhook(webhookSection: "Kusto:Endpoint");
                return;
            }
        }

        private void AddChangeNotificationWebhook(TenantConfiguration tenantConfiguration)
        {
            if (tenantConfiguration.ChangeNotificationSubscription == null || tenantConfiguration.ChangeNotificationSubscription.IsSubscribed == false)
                return;

            if (tenantConfiguration.ChangeNotificationSubscription?.Webhook == null ||
                tenantConfiguration.ChangeNotificationSubscription.Webhook.WebhookId.ToLowerInvariant() == "EventStore".ToLowerInvariant())
            {
                tenantConfiguration.ChangeNotificationSubscription.Webhook = GetWebhook(webhookSection: "EventStore");
                return;
            }
        }

        private WebhookConfiguration GetWebhook(string webhookSection)
        {
            WebhookConfiguration eventStoreWebhook = _configuration.GetSection(webhookSection).Get<WebhookConfiguration>();
            return eventStoreWebhook;
        }

        private void AddBusinessRuleEngineConfiguration(TenantConfiguration tenantConfiguration)
        {
            if (tenantConfiguration.BusinessRuleEngine == null || !tenantConfiguration.BusinessRuleEngine.Enabled)
                return;

            if (tenantConfiguration.BusinessRuleEngine.Storage == null)
            {
                tenantConfiguration.BusinessRuleEngine.Enabled = false;
                return;
            }
        }
    }
}
