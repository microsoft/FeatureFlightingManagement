using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Common.Storage
{
    /// <summary>
    /// Document DB Repository
    /// </summary>
    /// <typeparam name="TDoc">Type of the document in the database</typeparam>
    public interface IDocumentRepository<TDoc> where TDoc: class, new()
    {
        /// <summary>
        /// Gets a document by ID
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="partitionKey">Partition Key of the docment</param>
        /// <param name="trackingIds" cref="LoggerTrackingIds">Tracking IDs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<TDoc?> Get(string documentId, string partitionKey, LoggerTrackingIds trackingIds, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Queries for all documents in the document by query
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="partitionKey">Partition Key to search the document</param>
        /// <param name="trackingIds" cref="LoggerTrackingIds">Tracking IDs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns cref="IEnumerable{TDoc}">Collection of documents mathcing the query</returns>
        Task<IEnumerable<TDoc>> QueryAll(string query, string partitionKey, LoggerTrackingIds trackingIds, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Saves a document in the database (create or update)
        /// </summary>
        /// <param name="document">Document to be saved</param>
        /// <param name="partitionKey">Partitione key of the document</param>
        /// <param name="trackingIds" cref="LoggerTrackingIds">Tracking IDs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Saved document</returns>
        Task<TDoc> Save(TDoc document, string partitionKey, LoggerTrackingIds trackingIds, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Deletes a document by ID
        /// </summary>
        /// <param name="id">ID of the document to be deleted</param>
        /// <param name="partitionKey">Partition key of the document</param>
        /// <param name="trackingIds" cref="LoggerTrackingIds">Tracking IDs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task Delete(string id, string partitionKey, LoggerTrackingIds trackingIds, CancellationToken cancellationToken = default);
    }
}
