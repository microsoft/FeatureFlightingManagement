using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Core.Queries.GetEvaluationMetrics
{
    internal class KustoRequest
    {
        public string Tenant { get; set; }
        public string Query { get; set; }
        public Schema Schema { get; set; }
    }

    internal class Schema
    {
        public string Name { get; set; }
        public List<PropertyMetadata> Properties { get; set; }
    }

    internal class PropertyMetadata
    {
        public string PropertyName { get; set; }
        public string? PropertyDisplayName { get; set; }
        public bool IsMandatory { get; set; } = true;
    }
}
