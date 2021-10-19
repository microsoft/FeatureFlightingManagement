using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Domain.Configuration
{
    public class FeatureFlagDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Environment { get; set; }
        public bool Enabled { get; set; }
        public List<Stage> Stages { get; set; }
    }

    public class Stage
    {
        public string StageName { get; set; }
        public int StageId { get; set; }
        public bool IsActive { get; set; }
        public bool IsFirstStage { get; set; }
        public bool IsLastStage { get; set; }
    }
}
