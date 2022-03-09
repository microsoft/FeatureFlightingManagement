using System;
using CQRS.Mediatr.Lite.SDK.Domain;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Core.Domain.ValueObjects
{
    public class Filter: ValueObject
    {
        public string Name { get; private set; } // Context-Key
        public string Type { get; private set; } // Name
        public Operator Operator { get; private set; }
        public string Value { get; private set; }

        public Filter(AzureFilter azureFilter)
        {
            Name = azureFilter.Parameters.FlightContextKey;
            Type = azureFilter.Name;
            Enum.TryParse<Operator>(azureFilter.Parameters.Operator, out Operator op);
            Operator = op;
            Value = azureFilter.Parameters.Value;
        }

        public Filter(FilterDto filter)
        {
            Name = filter.FilterName;
            Type = filter.FilterType;
            Enum.TryParse<Operator>(filter.Operator, out Operator op);
            Operator = op;
            Value = filter.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Filter otherFilter)
                return false;

            return Name.ToLowerInvariant() == otherFilter.Name.ToLowerInvariant()
                && Type.ToLowerInvariant() == otherFilter.Type.ToLowerInvariant()
                && Operator == otherFilter.Operator
                && Value.ToLowerInvariant() == otherFilter.Value.ToLowerInvariant();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Type, Operator, Value);
        }
    }
}
