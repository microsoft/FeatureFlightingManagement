using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Core.Queries.GetEvaluationMetrics
{
    internal class KustoRequest
    {
        public string Query { get; set; }
        public List<Column> Columns { get; set; }
    }

    internal class Column
    {
        public string ColumnName { get; set; }
        public string DisplayName { get; set; }
    }
}
