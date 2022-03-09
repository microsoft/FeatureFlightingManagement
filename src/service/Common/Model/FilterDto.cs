namespace Microsoft.FeatureFlighting.Common.Model
{
    public class FilterDto
    {
        public string FilterType { get; set; }
        public string FilterName { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
    }
}
