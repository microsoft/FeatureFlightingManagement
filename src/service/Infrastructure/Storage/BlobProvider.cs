using System;
using Azure.Storage.Blobs;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Storage;
using Microsoft.FeatureFlighting.Common.AppExceptions;

namespace Microsoft.FeatureFlighting.Infrastructure.Storage
{   
    /// <inheritdoc/>
    internal class BlobProvider: IBlobProvider
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly ILogger _logger;

        public BlobProvider(BlobContainerClient blobContainerClient, ILogger logger)
        {
            _blobContainerClient = blobContainerClient ?? throw new ArgumentNullException(nameof(blobContainerClient));
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<string?> Get(string blobName, LoggerTrackingIds trackingIds)
        {
            DependencyContext dependency = new("Download Blob", _blobContainerClient.AccountName, "HTTP", true, $"GET {blobName}", trackingIds.CorrelationId, trackingIds.TransactionId, "FeatureFlighting:BlobProvider:Get", "", "");
            try
            {
                BlobClient blobClient = _blobContainerClient.GetBlobClient(blobName);
                bool blobExists = await blobClient.ExistsAsync();
                if (!blobExists)
                    return null;

                BlobDownloadResult result = await blobClient.DownloadContentAsync();
                string content = result.Content.ToString();
                dependency.CompleteDependency("200", content);
                _logger.Log(dependency);
                return content;
            }
            catch (Exception exception) 
            {
                dependency.FailDependency(exception, "");
                _logger.Log(dependency);
                throw new StorageException(_blobContainerClient.AccountName, _blobContainerClient.Name, blobName, exception, "Flighting:BlobProvider:Get", trackingIds.CorrelationId, trackingIds.TransactionId);
            }
        }
    }
}
