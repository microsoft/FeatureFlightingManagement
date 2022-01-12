using System.Linq;
using System.Collections.Generic;
using CQRS.Mediatr.Lite.SDK.Domain;
using Microsoft.FeatureFlighting.Common.Config;

namespace Microsoft.FeatureFlighting.Core.Domain.ValueObjects
{
    public class Settings: ValueObject
    {
        public bool EnableOptimization { get; private set; }
        public List<string> OptimizationRules { get; private set; }

        public Settings(FlightOptimizationConfiguration optimizationConfiguration)
        {   
            if (optimizationConfiguration == null)
            {
                EnableOptimization = false;
                return;
            }
            EnableOptimization = optimizationConfiguration.EnableOptimization && string.IsNullOrWhiteSpace(optimizationConfiguration.OptimizationRules);
            OptimizationRules =  !string.IsNullOrWhiteSpace(optimizationConfiguration.OptimizationRules)
                ? optimizationConfiguration.OptimizationRules.Split(';').ToList()
                : null;
        }
    }
}
