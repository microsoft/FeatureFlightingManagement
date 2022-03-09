using System.Linq;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common.Config;

namespace Microsoft.FeatureFlighting.Core.Evaluation
{
    /// <summary>
    /// Batches features into executable groups
    /// </summary>
    internal interface IFeatureBatchBuilder
    {
        /// <summary>
        /// Groups feature names based on tenant configuration
        /// </summary>
        /// <param name="features">Collection of feature names</param>
        /// <param name="tenantConfiguration" cref="TenantConfiguration">Tenant Configuration</param>
        /// <returns cref="IEnumerable{IGrouping{int, string}}">Collection of feature batches</returns>
        IEnumerable<IGrouping<int, string>> CreateBatches(IEnumerable<string> features, TenantConfiguration tenantConfiguration);
    }
}