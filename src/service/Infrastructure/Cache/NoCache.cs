using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.FeatureFlighting.Common.Caching;

[assembly: InternalsVisibleTo("Microsoft.PS.FlightingService.Core.Tests")]
namespace Microsoft.FeatureFlighting.Infrastructure.Cache
{
    /// <summary>
    /// Dummy cache for non cacheable scenarios
    /// </summary>
    internal class NoCache : ICache
    {
        public Task Delete(string key, string correlationId, string transactionId)
        {
            return Task.CompletedTask;
        }

        public Task<T> Get<T>(string key, string correlationId, string transactionId)
        {
            return Task.FromResult<T>(default);
        }

        public Task<IList<string>> GetList(string key, string correlationId, string transactionId)
        {
            return Task.FromResult<IList<string>>(null);
        }

        public Task Set<T>(string key, T value, string correlationId, string transactionId, int relativeExpirationMins = -1)
        {
            return Task.CompletedTask;
        }

        public Task SetList(string key, IList<string> values, string correlationId, string transactionId, int relativeExpirationMins = -1)
        {
            return Task.CompletedTask;
        }
    }
}
