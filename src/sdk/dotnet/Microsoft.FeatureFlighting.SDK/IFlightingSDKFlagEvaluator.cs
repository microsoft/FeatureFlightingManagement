using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.SDK
{
    public interface IFlightingSDKFlagEvaluator
    {
        Task<IDictionary<string, bool>> Evaluate(List<string> featureFlags, Dictionary<string, object> context, string correlationId = "", string transactionId = "");
    }
}
