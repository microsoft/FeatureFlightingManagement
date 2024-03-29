﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Common.Model
{   
    public class FeatureFlightDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        public string Name { get; set; }

        public string Tenant { get; set; }

        public string Description { get; set; }

        public string Environment { get; set; }
        
        public bool Enabled { get; set; }

        public List<StageDto> Stages { get; set; }

        public bool IsIncremental { get; set; }

        public bool IsAzureFlightOptimized { get; set; }

        public List<string> Optimizations { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AuditDto Audit { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public EvaluationMetricsDto EvaluationMetrics { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public FeatureUsageReportDto UsageReport { get; set; }
    }
}
