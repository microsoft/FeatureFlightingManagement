using System;
using Azure.Storage.Blobs;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Storage;

namespace Microsoft.FeatureFlighting.Infrastructure.Storage
{   
    /// <inheritdoc/>
    public class BlobProviderFactory: IBlobProviderFactory
    {   
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly ILogger _logger;
        private readonly IDictionary<string, IBlobProvider> _blobProviderCache;

        public BlobProviderFactory(ITenantConfigurationProvider tenantConfigurationProvider, ILogger logger)
        {
            _tenantConfigurationProvider = tenantConfigurationProvider ?? throw new ArgumentNullException(nameof(tenantConfigurationProvider));
            _blobProviderCache = new ConcurrentDictionary<string, IBlobProvider>(StringComparer.InvariantCultureIgnoreCase);
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IBlobProvider?> CreateBreWorkflowProvider(string tenantName)
        {
            if (_blobProviderCache.ContainsKey(tenantName))
                return _blobProviderCache[tenantName];

            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(tenantName);
            if (tenantConfiguration.BusinessRuleEngine == null || tenantConfiguration.BusinessRuleEngine.Storage == null)
                return null;

            BlobServiceClient blobServiceClient = new(tenantConfiguration.BusinessRuleEngine.Storage.StorageConnectionString);
            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(tenantConfiguration.BusinessRuleEngine.Storage.ContainerName);
            IBlobProvider blobProvider = new BlobProvider(blobContainerClient, _logger);
            _blobProviderCache.Add(tenantName, blobProvider);
            return blobProvider;
        }
    }
}
