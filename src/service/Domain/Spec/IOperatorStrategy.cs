using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Core.Operators;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Core.Spec
{
    /// <summary>
    /// Strategy for getting operators
    /// </summary>
    public interface IOperatorStrategy
    {
        /// <summary>
        /// Gets the <see cref="BaseOperator"/> from <see cref="Operator"/> enumeration
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        BaseOperator Get(Operator op);

        /// <summary>
        /// Gets all the available operator types
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllOperators();

        /// <summary>
        /// Gets a mapping between all the available filters and operators allowed for them
        /// </summary>
        /// <param name="tenant">Name of the tenant</param>
        /// <param name="correlationId">Correlation ID of the operation</param>
        /// <param name="transactionId">Transaction ID of the operation</param>
        /// <returns cref="IDictionary{string, List<string>}">Filter-Operator mapping</returns>
        Task<IDictionary<string, List<string>>> GetFilterOperatorMapping(string tenant, string correlationId, string transactionId);
    }
}
