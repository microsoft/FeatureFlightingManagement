namespace Microsoft.FeatureFlighting.Common
{
    /// <summary>
    /// Utilties to get feature flag ID and Name
    /// </summary>
    public static class FlagUtilities
    {
        /// <summary>
        /// Gets the feature flag ID from name
        /// </summary>
        /// <param name="appName">Tenant</param>
        /// <param name="envName">Environment</param>
        /// <param name="name">Feature Flag name</param>
        /// <returns>ID of the feature flag</returns>
        public static string GetFeatureFlagId(string appName, string envName, string name)
        {
            return string.Format(Constants.Flighting.FEATUREFLAG_CONVENTION, appName.ToLowerInvariant(), envName.ToLowerInvariant(), name);
        }

        /// <summary>
        /// Gets the feaure flag name from ID
        /// </summary>
        /// <param name="appName">Tenant</param>
        /// <param name="envName">Environment</param>
        /// <param name="id">ID of the feature flag</param>
        /// <returns>Name of the feature flag</returns>
        public static string GetFeatureFlagName(string appName, string envName, string id)
        {
            string featureFlagIdPrefix = string.Format(Constants.Flighting.FEATUREFLAG_CONVENTION, appName.ToLowerInvariant(), envName.ToLowerInvariant(), "");
            return id.Remove(0, featureFlagIdPrefix.Length);
        }
    }
}
