using CQRS.Mediatr.Lite.SDK.Domain;

namespace Microsoft.FeatureFlighting.Core.Domain.ValueObjects
{
    public class Tenant: ValueObject
    {
        public string Id { get; private set; }
        public string Environment { get; private set; }

        public Tenant(string tenantId, string environment)
        {
            Id = tenantId;
            Environment = environment;
        }
    }
}
