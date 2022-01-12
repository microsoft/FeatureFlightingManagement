namespace Microsoft.FeatureFlighting.Core.Domain.ValueObjects
{
    public class Feature
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public Feature(string featureName, string featureDescription)
        {
            Name = featureName;
            Description = featureDescription;
        }
    }
}
