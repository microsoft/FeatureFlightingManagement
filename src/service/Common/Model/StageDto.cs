using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Common.Model
{
    public class StageDto
    {
        public int StageId { get; set; }
        public string StageName { get; set; }
        public bool IsActive { get; set; }
        public bool IsFirstStage { get; set; }
        public bool IsLastStage { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<FilterDto> Filters { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? LastActivatedOn { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? LastDeactivatedOn { get; set; }
    }
}
