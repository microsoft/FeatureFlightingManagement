using System;

namespace Microsoft.FeatureFlighting.Tests.Functional.Utilities
{

    [Serializable]
    public class FeatureFlag
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public string Label { get; set; }
        public string Name { get; set; }
        public string Environment { get; set; }
        public Condition Conditions { get; set; }
    }

    public class Condition
    {
        public Filter[] Client_Filters { get; set; }
    }

    public class Filter
    {
        public string Name { get; set; } 
        public FilterSettings Parameters { get; set; }
    }
}
