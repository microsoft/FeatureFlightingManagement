using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.PS.FlightingService.KustoRepository.Interfaces
{
    public interface IKustoDocumentRepository
    {
        /// <summary>
        /// Get all fault events from multiple databases
        /// </summary>
        /// <param name="clusterUrl">cluster url</param>
        /// <param name="database"></param>
        /// <param name="query">query to execute</param>
        /// <param name="token"></param>
        /// <returns>List of FaultEvents</returns>
        Task<IList<string>> GetApplicationList(string query);
    }
}
