using System.Text.RegularExpressions;

namespace Microsoft.FeatureFlighting.Common
{
    public static class Utility
    {
        public static string GetFeatureFlagId(string appName, string envName, string id)
        {
            return string.Format(Constants.Flighting.FEATUREFLAG_CONVENTION, appName.ToLowerInvariant(), envName.ToLowerInvariant(), id);
        }

        public static string GetFormattedTenantName(string tenant) =>
            Regex.Replace(tenant.ToUpperInvariant(), @"[\W\s]+", "_");
    }
}
