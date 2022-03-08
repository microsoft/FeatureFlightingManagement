namespace Microsoft.FeatureFlighting.Tests.Functional
{
    public struct FeatureControlConstants
    {
        public const string ApplicationHeader = "X-Application";
        public const string EnvironmentHeader = "X-Environment";
        public const string FLIGHT_CONTEXT_HEADER = "X-FlightContext";
        public const string ContentTypeHeader = "Content-Type";
        public const string JsonContentHeaderValue = "application/json";
        public const string FeatureFlagsRoute = "/api/v1/featureflags";
        public const string FeatureConfigurationRoute = "/api/v1/configuration/";
        public const string GetOperatorsRoute = "operators";
        public const string GetFiltersRoute = "filters";
        public const string FilterOperatorsMappingRoute = "filters/operators/map";
        public const string GetAllApplicationsRoute = "applications";
        public const string ActivateStageRoute = "{0}/activatestage/{1}";
        public const string EnableFeatureFlagRoute = "{0}/enable";
        public const string DisableFeatureFlagRoute = "{0}/disable";
        public const string DeleteFeatureFlagRoute = "{0}";
        public const string Evaluate = "Evaluate?featureNames=";
        public const string Evaluate_Backwards = "{0}/{1}/flighting?featureNames=";
   
        //public const string SignedInUserObjectIdIdentifier = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        public enum FlightingEnvironments
        {
            DEV,
            SIT,
            UAT,
            Prod
        }
    }
}