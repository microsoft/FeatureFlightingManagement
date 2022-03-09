using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.AppExceptions;

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

        public void Validate(LoggerTrackingIds trackingIds)
        {
            if (Name.Contains("__") || Name.Contains("."))
                throw new DomainException($"Feature name cannot contain `__` or`.`", "CREATE_NAME_001", trackingIds.CorrelationId, trackingIds.TransactionId, "Feature:Validate");
        }
    }
}
