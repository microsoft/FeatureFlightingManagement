using System;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Common.Model
{
    /// <summary>
    /// Report for a feature flag exceeding a certain threshold
    /// </summary>
    public class ThresholdExceededReportDto
    {
        /// <summary>
        /// ID of the feature
        /// </summary>
        public string FeatureId { get; set; }

        /// <summary>
        /// Name of the feature
        /// </summary>
        public string FeatureName { get; set; }

        /// <summary>
        /// Environment
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// Configured threshold value
        /// </summary>
        public int Threshold { get; set; }
        
        /// <summary>
        /// Current value
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Unit of threshold (default - days)
        /// </summary>
        public string ThresholdUnit { get; set; }
    }
}
