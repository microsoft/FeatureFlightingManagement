using System.Threading.Tasks;
using System.Collections.Generic;

namespace Microsoft.PS.FlightingService.Domain.Interfaces
{
    public interface IFeatureFlagEvaluator
    {
        Task<Dictionary<string, bool>> Evaluate(string applicationName, string environment, List<string> featureFlags);
    }
}
