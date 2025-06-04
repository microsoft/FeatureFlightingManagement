using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common;
using Microsoft.Extensions.Configuration;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Storage;
using Microsoft.FeatureFlighting.Common.AppExceptions;
using Microsoft.FeatureFlighting.Common.Config;
using Azure.Identity;
using Azure.Core;

namespace Microsoft.FeatureFlighting.Infrastructure.Storage
{
    /// <summary>
    /// Azure Cosmos DB Document repository
    /// </summary>
    internal class CosmosDbRepository<TDoc> : IDocumentRepository<TDoc> where TDoc : class, new()
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly Container _container;

        private const string GET_ALL_DOCS_QUERY = "SELECT * FROM C";
        private const string GET_DOC_BY_ID_QUERY = "SELECT * FROM C WHERE C.id = {0}";

        public CosmosDbRepository(CosmosDbConfiguration cosmosConfiguration, IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            CosmosClientOptions options = new()
            {
                ApplicationName = "FeatureFlightingManagement",
                EnableTcpConnectionEndpointRediscovery = true,
                MaxRequestsPerTcpConnection = int.Parse(_configuration["CosmosDb:MaxRequestsPerTcpConnection"]),
                ConnectionMode = ConnectionMode.Direct,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(int.Parse(_configuration["CosmosDb:MaxRetryWaitTimeOnRateLimitedRequests"])),
                MaxRetryAttemptsOnRateLimitedRequests = int.Parse(_configuration["CosmosDb:MaxRetryAttemptsOnRateLimitedRequests"])
            };
            TokenCredential credential;
            credential = ManagedIdentityHelper.GetTokenCredential();            
            CosmosClient client = new(cosmosConfiguration.Endpoint, credential, options);
            Database database = client.GetDatabase(cosmosConfiguration.DatabaseId);
            _container = database.GetContainer(cosmosConfiguration.ContainerId);
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<TDoc?> Get(string documentId, string partitionKey, LoggerTrackingIds trackingIds, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(partitionKey))
            {
                string getDocByIdQuery = string.Format(GET_DOC_BY_ID_QUERY, documentId);
                IEnumerable<TDoc> results = await QueryAll(getDocByIdQuery, String.Empty, trackingIds, cancellationToken);
                return results.FirstOrDefault();
            }

            DependencyContext dependencyContext = new("Cosmos", _configuration["ComsosDb:Endpoint"], "HTTP", true, $"Get {documentId}", trackingIds.CorrelationId, trackingIds.TransactionId, "CosmosDbRepository:Get", "", "");
            try
            {
                ItemRequestOptions itemRequestOptions = new();
                ItemResponse<TDoc> response = await _container.ReadItemAsync<TDoc>(documentId, new PartitionKey(partitionKey), itemRequestOptions, cancellationToken);
                LogDependency(response, dependencyContext, trackingIds, "Get");
                return response.Resource;
            }
            catch (CosmosException cosmosException)
            {
                LogDependency(cosmosException, dependencyContext, trackingIds, "Get");
                if (cosmosException.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return default;
                }
                CosmosDbException dataException = new(cosmosException, _container.Id, "DATA-COSMOS-READ-001", $"CosmosDbRepository.Get", trackingIds.CorrelationId, trackingIds.TransactionId);
                throw dataException;
            }
            catch (Exception exception)
            {
                LogDependency(exception, dependencyContext, trackingIds, "Get");
                CosmosDbException dataException = new(exception, _container.Id, "DATA-COSMOS-READ-002", $"CosmosDbRepository.Get", trackingIds.CorrelationId, trackingIds.TransactionId);
                throw dataException;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TDoc>> QueryAll(string query, string partitionKey, LoggerTrackingIds trackingIds, CancellationToken cancellationToken = default)
        {
            DependencyContext dependencyContext = new("Cosmos", _configuration["ComsosDb:Endpoint"], "HTTP", true, $"Query Documents: {query}", trackingIds.CorrelationId, trackingIds.TransactionId, "CosmosDbRepository:QueryAll", "", "");
            try
            {
                query = !string.IsNullOrWhiteSpace(query) ? query : GET_ALL_DOCS_QUERY;
                QueryRequestOptions queryRequestOptions = new();
                queryRequestOptions.PartitionKey = !string.IsNullOrWhiteSpace(partitionKey) ? new PartitionKey(partitionKey) : null;
                queryRequestOptions.MaxItemCount = int.Parse(_configuration["CosmosDb:MaxItemsPerQuery"]);

                FeedIterator<TDoc> iterator = _container.GetItemQueryIterator<TDoc>(query, continuationToken: null, requestOptions: queryRequestOptions);
                List<TDoc> results = new();
                FeedResponse<TDoc>? feedResponse = null;

                while (iterator.HasMoreResults)
                {
                    feedResponse = await iterator.ReadNextAsync(cancellationToken);
                    results.AddRange(feedResponse);
                }
                LogDependency(feedResponse, dependencyContext, trackingIds, "QueryAll");

                return results;
            }
            catch (CosmosException cosmosException)
            {
                LogDependency(cosmosException, dependencyContext, trackingIds, "QueryAll");
                throw new CosmosDbException(cosmosException, _container.Id, "DATA-COSMOS-READ-003", $"CosmosDbRepository:QueryAll", trackingIds.CorrelationId, trackingIds.TransactionId);
            }
            catch (Exception exception)
            {
                LogDependency(exception, dependencyContext, trackingIds, "QueryAll");
                CosmosDbException dataException = new(exception, _container.Id, "DATA-COSMOS-READ-004", $"CosmosDbRepository:QueryAll", trackingIds.CorrelationId, trackingIds.TransactionId);
                throw dataException;
            }
        }

        /// <inheritdoc/>
        public async Task<TDoc> Save(TDoc document, string partitionKey, LoggerTrackingIds trackingIds, CancellationToken cancellationToken = default)
        {
            DependencyContext dependencyContext = new("Cosmos", _configuration["ComsosDb:Endpoint"], "HTTP", true, "Save", trackingIds.CorrelationId, trackingIds.TransactionId, "CosmosDbRepository:Save", "", "");
            try
            {
                ItemRequestOptions itemRequestOptions = new();
                ItemResponse<TDoc> response = await _container.UpsertItemAsync(document, new PartitionKey(partitionKey), itemRequestOptions, cancellationToken);
                LogDependency(response, dependencyContext, trackingIds, "Save");
                return response.Resource;
            }
            catch (CosmosException cosmosException)
            {
                LogDependency(cosmosException, dependencyContext, trackingIds, "Save");
                throw new CosmosDbException(cosmosException, _container.Id, "DATA-COSMOS-SAVE-001", $"CosmosDbRepository:Save", trackingIds.CorrelationId, trackingIds.TransactionId);
            }
            catch (Exception exception)
            {
                LogDependency(exception, dependencyContext, trackingIds, "Save");
                throw new CosmosDbException(exception, _container.Id, "DATA-COSMOS-SAVE-002", $"CosmosDbRepository:Save", trackingIds.CorrelationId, trackingIds.TransactionId);
            }
        }

        /// <inheritdoc/>
        public async Task Delete(string id, string partitionKey, LoggerTrackingIds trackingIds, CancellationToken cancellationToken = default)
        {
            DependencyContext dependencyContext = new("Cosmos", _configuration["ComsosDb:Endpoint"], "HTTP", true, "Delete", trackingIds.CorrelationId, trackingIds.TransactionId, "CosmosDbRepository:Delete", "", "");
            try
            {
                ItemRequestOptions itemRequestOptions = new();
                ItemResponse<TDoc> response = await _container.DeleteItemAsync<TDoc>(id, new PartitionKey(partitionKey), itemRequestOptions, cancellationToken);
                LogDependency(response, dependencyContext, trackingIds, "Delete");
            }
            catch (CosmosException cosmosException)
            {
                LogDependency(cosmosException, dependencyContext, trackingIds, "Delete");
                throw new CosmosDbException(cosmosException, _container.Id, "DATA-COSMOS-DELETE-001", $"ComosDbRepository:Delete", trackingIds.CorrelationId, trackingIds.TransactionId);
            }
            catch (Exception exception)
            {
                LogDependency(exception, dependencyContext, trackingIds, "Delete");
                throw new CosmosDbException(exception, _container.Id, "DATA-COSMOS-DELETE-002", $"ComosDbRepository:Delete", trackingIds.CorrelationId, trackingIds.TransactionId);
            }
        }

        #region Private:Loggers
        private void LogDependency(ItemResponse<TDoc> response, DependencyContext dependencyContext, LoggerTrackingIds trackingIds, string method)
        {
            try
            {
                dependencyContext.CompleteDependency(response.StatusCode.ToString(), "");
                dependencyContext.AddProperty(nameof(response.Diagnostics), response.Diagnostics?.ToString());
                dependencyContext.AddProperty(nameof(response.ActivityId), response.ActivityId);
                dependencyContext.AddProperty(nameof(response.RequestCharge), response.RequestCharge);
                dependencyContext.AddProperty(nameof(response.ETag), response.ETag);
                dependencyContext.AddProperty("DatabaseId", _container.Database.Id);
                dependencyContext.AddProperty("ContainerId", _container.Id);
                dependencyContext.CorrelationId = trackingIds.CorrelationId;
                dependencyContext.TransactionId = trackingIds.TransactionId;
                dependencyContext.Source = new StringBuilder().Append("CosmosDbRepository").Append(":").Append(method).ToString();
                _logger.Log(dependencyContext);
            }
            catch (Exception) { } // Do not throw exception when logging fails
        }

        private void LogDependency(FeedResponse<TDoc>? response, DependencyContext dependencyContext, LoggerTrackingIds trackingIds, string method)
        {
            try
            {
                if (response == null)
                    return;

                dependencyContext.CompleteDependency(response.StatusCode.ToString(), "OBJECTS");
                dependencyContext.AddProperty(nameof(response.RequestCharge), response.RequestCharge);
                dependencyContext.AddProperty(nameof(response.ActivityId), response.ActivityId);
                dependencyContext.AddProperty("ContactedRegions", response.Diagnostics?.GetContactedRegions());
                dependencyContext.AddProperty("ClientElapsedTime", response.Diagnostics?.GetClientElapsedTime().TotalMilliseconds);
                dependencyContext.AddProperty("DatabaseId", _container?.Database?.Id);
                dependencyContext.AddProperty("ContainerId", _container?.Id);
                dependencyContext.CorrelationId = trackingIds.CorrelationId;
                dependencyContext.TransactionId = trackingIds.TransactionId;
                dependencyContext.Source = new StringBuilder().Append("ComosDbRepository").Append(":").Append(method).ToString();
                _logger.Log(dependencyContext);
            }
            catch (Exception) { } // Do not throw exception when logging fails
        }

        private void LogDependency(CosmosException cosmosException, DependencyContext dependencyContext, LoggerTrackingIds trackingIds, string method)
        {
            try
            {
                dependencyContext.FailDependency(cosmosException, cosmosException.StatusCode.ToString());
                dependencyContext.AddProperty(nameof(cosmosException.Diagnostics), cosmosException.Diagnostics?.ToString());
                dependencyContext.AddProperty(nameof(cosmosException.ActivityId), cosmosException.ActivityId);
                dependencyContext.AddProperty(nameof(cosmosException.SubStatusCode), cosmosException.SubStatusCode);
                dependencyContext.AddProperty(nameof(cosmosException.RequestCharge), cosmosException.RequestCharge);
                dependencyContext.AddProperty("CosmosExceptionSource", cosmosException.Source);
                dependencyContext.AddProperty(nameof(cosmosException.RetryAfter), cosmosException.RetryAfter.ToString());
                dependencyContext.AddProperty("DatabaseId", _container.Database.Id);
                dependencyContext.AddProperty("ContainerId", _container.Id);
                dependencyContext.CorrelationId = trackingIds.CorrelationId;
                dependencyContext.TransactionId = trackingIds.TransactionId;
                dependencyContext.Source = new StringBuilder().Append("CosmosDbRepository").Append(":").Append(method).ToString();
                _logger.Log(dependencyContext);
            }
            catch (Exception) { } // Do not throw exception when logging fails
        }

        private void LogDependency(Exception exception, DependencyContext dependencyContext, LoggerTrackingIds trackingIds, string method)
        {
            try
            {
                dependencyContext.FailDependency(exception, "UNHANDLED_FAILURE");
                dependencyContext.CorrelationId = trackingIds.CorrelationId;
                dependencyContext.TransactionId = trackingIds.TransactionId;
                dependencyContext.AddProperty("DatabaseId", _container.Database.Id);
                dependencyContext.AddProperty("ContainerId", _container.Id);
                dependencyContext.Source = new StringBuilder().Append("CosmosDbRepository").Append(":").Append(method).ToString();
                _logger.Log(dependencyContext);
            }
            catch (Exception) { } // Do not throw exception when logging fails
        }
        #endregion Private:Loggers
    }
}
