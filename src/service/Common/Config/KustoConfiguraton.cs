namespace Microsoft.FeatureFlighting.Common.Config
{
    /// <summary>
    /// Configuration to connect to Kusto
    /// </summary>
    public class KustoConfiguraton
    {
        public WebhookConfiguration Endpoint { get; set; }
        public string KustoTenant { get; set; }

        public string PerformancQuery { get; set; }
        public string Column_Count { get; set; }
        public string Column_Transformed_Count { get; set; }
        public string Column_AvgTime { get; set; }
        public string Column_Transformed_AvgTime { get; set; }
        public string Column_P95 { get; set; }
        public string Column_Transformed_P95 { get; set; }
        public string Column_P90 { get; set; }
        public string Column_Transformed_P90 { get; set; }


        public string LastUsageQuery { get; set; }
        public string Column_Timestamp { get; set; }
        public string Column_Transformed_Timestamp { get; set; }
        public string Column_UserId { get; set; }
        public string Column_Transformed_UserId { get; set; }

        public void SetDefault()
        {
            PerformancQuery = !string.IsNullOrWhiteSpace(PerformancQuery) ? PerformancQuery :
                 "customEvents " +
                 "| where timestamp > ago({4}d) " +
                 "| where name == '{1}' " +
                 "| where appName == '{3}' " +
                 "| where customDimensions.Tenant =~ '{2}' " +
                 "| where tostring(customDimensions.Features) contains_cs '{0}' " +
                 "| extend TimeTaken=toreal(customDimensions['{0}:TimeTaken']) " +
                 "| summarize count(), avg(TimeTaken), percentiles(TimeTaken, 95, 90)";

            Column_Count = !string.IsNullOrWhiteSpace(Column_Count) ? Column_Count : "count_";
            Column_Transformed_Count = !string.IsNullOrWhiteSpace(Column_Transformed_Count) ? Column_Transformed_Count : "Count";

            Column_AvgTime = !string.IsNullOrWhiteSpace(Column_AvgTime) ? Column_AvgTime : "avg_TimeTaken";
            Column_Transformed_AvgTime = !string.IsNullOrWhiteSpace(Column_Transformed_AvgTime) ? Column_Transformed_AvgTime : "Average";

            Column_P95 = !string.IsNullOrWhiteSpace(Column_P95) ? Column_P95 : "percentile_TimeTaken_95";
            Column_Transformed_P95 = !string.IsNullOrWhiteSpace(Column_Transformed_P95) ? Column_P95 : "P95";

            Column_P90 = !string.IsNullOrWhiteSpace(Column_P90) ? Column_P90 : "percentile_TimeTaken_90";
            Column_Transformed_P90 = !string.IsNullOrWhiteSpace(Column_Transformed_P90) ? Column_P90 : "P90";

            LastUsageQuery = !string.IsNullOrWhiteSpace(LastUsageQuery) ? LastUsageQuery :
                "customEvents " +
                "| where timestamp > ago({4}d) " +
                "| where name == '{1}' " +
                "| where appName == '{3}' " +
                "| where customDimensions.Tenant =~ '{2}' " +
                "| where tostring(customDimensions.Features) contains_cs '{0}' " +
                "| order by timestamp desc  " +
                "| limit 1 " +
                "| project timestamp, user_Id";

            Column_Timestamp = !string.IsNullOrWhiteSpace(Column_Timestamp) ? Column_Timestamp : "timestamp";
            Column_Transformed_Timestamp = !string.IsNullOrWhiteSpace(Column_Transformed_Timestamp) ? Column_Transformed_Timestamp : "Timestamp";

            Column_UserId = !string.IsNullOrWhiteSpace(Column_UserId) ? Column_Timestamp : "user_Id";
            Column_Transformed_UserId = !string.IsNullOrWhiteSpace(Column_Transformed_UserId) ? Column_Transformed_UserId : "userId";
        }
        
    }
}
