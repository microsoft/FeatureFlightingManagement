using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.FeatureFlighting.Tests.Functional.Helper
{
    
    public class ClientCreator
    {
        private static FeatureFlagClient SingletonFeatureFlagClient;
        private static readonly object lockObj = new();
        
        public static FeatureFlagClient CreateFeatureFlagClient(TestContext _testContext)
        {
            lock (lockObj)
            {
                if (SingletonFeatureFlagClient == null)
                {
                    var featureFlagClient = new FeatureFlagClient(_testContext);
                    SingletonFeatureFlagClient = featureFlagClient;
                }
                return SingletonFeatureFlagClient;
            }
        }
    }
}
