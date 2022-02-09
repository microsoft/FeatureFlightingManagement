using System;
using System.Linq;
using Newtonsoft.Json;
using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Common.Config;
using Microsoft.FeatureFlighting.Common.Webhook;
using Microsoft.FeatureFlighting.Core.Queries.GetEvaluationMetrics;

namespace Microsoft.FeatureFlighting.Core.Queries
{
    /// <summary>
    /// Handles <see cref="GetEvaluationMetrics"/>
    /// </summary>
    internal class GetEvaluationMetricsQueryHandler : QueryHandler<GetEvaluationMetricsQuery, EvaluationMetricsDto>
    {
        private readonly ITenantConfigurationProvider _tenantConfigurationProvider;
        private readonly IWebhookTriggerManager _webhookTriggerManager;

        public GetEvaluationMetricsQueryHandler(ITenantConfigurationProvider tenantConfigurationProvider, IWebhookTriggerManager webhookTriggerManager)
        {
            _tenantConfigurationProvider = tenantConfigurationProvider;
            _webhookTriggerManager = webhookTriggerManager;
        }

        /// <summary>
        /// Gets evaluation metrics from Kusto DB
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected override async Task<EvaluationMetricsDto> ProcessRequest(GetEvaluationMetricsQuery query)
        {
            TenantConfiguration tenantConfiguration = await _tenantConfigurationProvider.Get(query.Tenant);
            if (tenantConfiguration.Metrics == null || !tenantConfiguration.Metrics.Enabled)
                return null;

            KustoConfiguraton kustoConfiguration = tenantConfiguration.Metrics.Kusto;

            Task<IEnumerable<List<KeyValuePair<string, string>>>> getPerformanceMetrics =
                GetPerformanceMetrics(tenantConfiguration, query);
            Task<IEnumerable<List<KeyValuePair<string, string>>>> getLastUsageMetrics =
                GetLastUsageMetrics(tenantConfiguration, query);

            await Task.WhenAll(getPerformanceMetrics, getLastUsageMetrics);
            IEnumerable<List<KeyValuePair<string, string>>> performanceMetrics = getPerformanceMetrics.Result;
            IEnumerable<List<KeyValuePair<string, string>>> lastUsageMetrics = getLastUsageMetrics.Result;

            EvaluationMetricsDto evaluationMetrics = new()
            {
                EvaluationCount = (int)GetNumericMetric(performanceMetrics, kustoConfiguration.Column_Transformed_Count),
                AverageLatency = GetNumericMetric(performanceMetrics, kustoConfiguration.Column_Transformed_AvgTime),
                P95Latency = GetNumericMetric(performanceMetrics, kustoConfiguration.Column_Transformed_P95),
                P90Latency = GetNumericMetric(performanceMetrics, kustoConfiguration.Column_Transformed_P90),
                LastEvaluatedBy = GetMetric(lastUsageMetrics, kustoConfiguration.Column_Transformed_UserId),
                LastEvaluatedOn = DateTime.Parse(GetMetric(lastUsageMetrics, kustoConfiguration.Column_Transformed_Timestamp, DateTime.MinValue.ToString())),
                From = DateTime.UtcNow.AddDays(-query.TimespanInDays),
                To = DateTime.UtcNow
            };

            return evaluationMetrics;
        }

        private async Task<IEnumerable<List<KeyValuePair<string, string>>>> GetPerformanceMetrics(TenantConfiguration tenantConfiguration, GetEvaluationMetricsQuery query)
        {
            KustoConfiguraton kustoConfiguration = tenantConfiguration.Metrics.Kusto;
            string performanceQuery = string.Format(kustoConfiguration.PerformancQuery,
                query.FeatureName,
                tenantConfiguration.Metrics.TrackingEventName,
                tenantConfiguration.Name,
                tenantConfiguration.Metrics.AppInsightsName,
                query.TimespanInDays);

            KustoRequest kustoRequest = new()
            {
                Query = performanceQuery,
                Columns = new List<Column>
                {
                    new Column
                    {
                        ColumnName = kustoConfiguration.Column_Count,
                        DisplayName = kustoConfiguration.Column_Transformed_Count,
                    },
                    new Column
                    {
                        ColumnName = kustoConfiguration.Column_AvgTime,
                        DisplayName = kustoConfiguration.Column_Transformed_AvgTime,
                    },
                    new Column
                    {
                        ColumnName = kustoConfiguration.Column_P95,
                        DisplayName = kustoConfiguration.Column_Transformed_P95,
                    },
                    new Column
                    {
                        ColumnName = kustoConfiguration.Column_P90,
                        DisplayName = kustoConfiguration.Column_Transformed_P90,
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(kustoRequest);
            Dictionary<string, string> headers = new()
            {
                { "x-ms-tenant", kustoConfiguration.KustoTenant }
            };
            string response = await _webhookTriggerManager.Trigger(tenantConfiguration.Metrics.MetricSource, requestBody, headers, query.TrackingIds);
            if (string.IsNullOrWhiteSpace(response))
                return null;

            return JsonConvert.DeserializeObject<IEnumerable<List<KeyValuePair<string, string>>>>(response);
        }

        private async Task<IEnumerable<List<KeyValuePair<string, string>>>> GetLastUsageMetrics(TenantConfiguration tenantConfiguration, GetEvaluationMetricsQuery query)
        {
            KustoConfiguraton kustoConfiguration = tenantConfiguration.Metrics.Kusto;
            string lastUsageQuery = string.Format(kustoConfiguration.LastUsageQuery,
                query.FeatureName,
                tenantConfiguration.Metrics.TrackingEventName,
                tenantConfiguration.Name,
                tenantConfiguration.Metrics.AppInsightsName,
                query.TimespanInDays);

            KustoRequest kustoRequest = new()
            {
                Query = lastUsageQuery,
                Columns = new List<Column>
                {
                    new Column
                    {
                        ColumnName = kustoConfiguration.Column_Timestamp,
                        DisplayName = kustoConfiguration.Column_Transformed_Timestamp,
                    },
                    new Column
                    {
                        ColumnName = kustoConfiguration.Column_UserId,
                        DisplayName = kustoConfiguration.Column_Transformed_UserId,
                    }
                }
            };

            string requestBody = JsonConvert.SerializeObject(kustoRequest);
            Dictionary<string, string> headers = new()
            {
                { "x-ms-tenant", kustoConfiguration.KustoTenant }
            };
            string response = await _webhookTriggerManager.Trigger(tenantConfiguration.Metrics.MetricSource, requestBody, headers, query.TrackingIds);
            if (string.IsNullOrWhiteSpace(response))
                return null;

            return JsonConvert.DeserializeObject<IEnumerable<List<KeyValuePair<string, string>>>>(response);
        }

        private string GetMetric(IEnumerable<List<KeyValuePair<string, string>>> metricResult, string metricKey, string defaultValue = null)
        {
            if (metricResult == null || !metricResult.Any())
                return defaultValue;

            return metricResult
                .FirstOrDefault()?
                .FirstOrDefault(metricKV => metricKV.Key.ToLowerInvariant() == metricKey.ToLowerInvariant())
                .Value;
        }

        private double GetNumericMetric(IEnumerable<List<KeyValuePair<string, string>>> metricResult, string metricKey, double defaultValue = 0.0)
        {
            if (metricResult == null || !metricResult.Any())
                return defaultValue;

            string metric = 
                metricResult
                .FirstOrDefault()?
                .FirstOrDefault(metricKV => metricKV.Key.ToLowerInvariant() == metricKey.ToLowerInvariant())
                .Value;

            if (string.IsNullOrWhiteSpace(metric))
                return defaultValue;

            if (!double.TryParse(metric, out double metricValue))
                return defaultValue;

            return metricValue;
        }
    }
}
