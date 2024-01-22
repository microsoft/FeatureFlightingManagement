﻿using System;
using Azure.Storage.Blobs;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Storage;
using Azure.Identity;
using Microsoft.FeatureFlighting.Common;

namespace Microsoft.FeatureFlighting.Infrastructure.Storage
{
    /// <inheritdoc/>
    internal class BlobProviderFactory : IBlobProviderFactory
    {
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly ILogger _logger;
        private readonly IDictionary<string, IBlobProvider> _blobProviderCache;
        private readonly DefaultAzureCredential _defaultAzureCredential;

        public BlobProviderFactory(ITenantConfigurationProvider tenantConfigurationProvider, ILogger logger)
        {
            _tenantConfigurationProvider = tenantConfigurationProvider ?? throw new ArgumentNullException(nameof(tenantConfigurationProvider));
            _blobProviderCache = new ConcurrentDictionary<string, IBlobProvider>(StringComparer.InvariantCultureIgnoreCase);
            _logger = logger;
            if (_defaultAzureCredential == null)
                _defaultAzureCredential = new DefaultAzureCredential();

        }

        /// <inheritdoc/>
        public async Task<IBlobProvider?> CreateBreWorkflowProvider(string tenantName)
        {
            if (_blobProviderCache.ContainsKey(tenantName))
                return _blobProviderCache[tenantName];

            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(tenantName);
            if (tenantConfiguration.BusinessRuleEngine == null || tenantConfiguration.BusinessRuleEngine.Storage == null)
                return null;

            BlobServiceClient blobServiceClient = new(new Uri(string.Format(Constants.Flighting.BLOB_CONTAINER_URI, tenantConfiguration.BusinessRuleEngine.Storage.StorageAccountName)), _defaultAzureCredential);
            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(tenantConfiguration.BusinessRuleEngine.Storage.ContainerName);
            IBlobProvider blobProvider = new BlobProvider(blobContainerClient, _logger);
            _blobProviderCache.Add(tenantName, blobProvider);
            return blobProvider;
        }
    }
}
