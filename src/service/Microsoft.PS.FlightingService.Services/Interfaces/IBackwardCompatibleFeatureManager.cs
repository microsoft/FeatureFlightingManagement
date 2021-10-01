using System.Threading.Tasks;
using System.Collections.Generic;

namespace Microsoft.PS.FlightingService.Services.Interfaces
{
    public interface IBackwardCompatibleFeatureManager
    {
        bool IsBackwardCompatibityRequired(string componentName, string environment, string featureFlag);
        Task<Dictionary<string, bool>> IsEnabledAsync(string componentName, string environment, List<string> featureFlags, string flightContext);
    }
}
