using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Tests.Functional.Utilities
{ 
   public class FilterSettings
    {
        public string Operator { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string IsActive { get; set; } = "false";
        public string StageId { get; set; } = "-1";
        public string StageName { get; set; } = string.Empty;
        public string FlightContextKey { get; set; } = string.Empty;
    }
}
