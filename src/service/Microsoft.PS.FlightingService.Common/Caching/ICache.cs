using System.Threading.Tasks;
using System.Collections.Generic;

namespace Microsoft.PS.FlightingService.Common.Caching
{
    public interface ICache
    {
        Task<List<string>> GetList(string key, string correlationId, string transactionId);
        Task SetList(string key, List<string> values, string correlationId, string transactionId, int relativeExpirationMins = -1);
        Task Delete(string key, string correlationId, string transactionId);
    }
}
