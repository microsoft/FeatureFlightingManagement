namespace Microsoft.FeatureFlighting.Common
{
    public struct Constants
    {
        public struct Flighting
        {
            public const string FLIGHT_CONTEXT_HEADER = "X-FlightContext";
            public const string FEATUREFLAG_CONVENTION = "{0}_{1}_{2}";
            public const string FLIGHTING_AUTH_PERM = "manageexperimentation";
            public const string FLIGHTING_ADMIN_RESOURCE = "Experimentation";
            public const string FLIGHTING_ADMIN_PERM = "All";
            public const string FLIGHT_TRACKER_PARAM = "FlightTrackIds";
            public const string FEATURE_FILTER_PARAM = "FeatureFilter";
            public const string FEATURE_NAME_PARAM = "FeatureName";
            public const string FEATURE_ENV_PARAM = "FeatureEnvironment";
            public const string FEATURE_APP_PARAM = "FeatureApplication";
            public const string ALL = "*";

            public struct Environment
            {
                public const string Production = "Production";
                public const string PreProduction = "PPE";
            }
        }

        public struct Logging
        {
            public const string MissingFlightingContextHeader = "X-FlightContext Header is missing in the request";
            public const string InvalidFilterSettings = "Invalid filter settings";
            public const string GenericError = "Something went wrong: {0}";
        }

        public struct Exception
        {
            public struct Types
            {
                public const string GRAPH = "Graph";
                public const string GENERAL = "General";
                public const string DOMAIN = "Domain";
                public const string AZURERREQUESTEXCEPTION = "AzureRequest";
                public const string ACCESS_FORBIDDEN = "AccessForbidden";
                public const string STORAGE = "Storage";
                public const string BRE = "RuleEngine";
            }
            public struct GeneralException
            {
                public const string ExceptionCode = "GE-1001";
                public const string ExceptionMessage = "Unhandled exception occured";
                public const string DisplayMessage = "OOPS! Something went wrong. Please contact support with this ID {0}";
            }

            public struct DomainException
            {
                public const string DisplayMessage = "Domain operation Failed: {0}.\nCorrelation ID - {1}";

                public struct FlagDoesntExist
                {
                    public const string Message = "Not feature flag with name - {0} exists for {1} in {2}";
                    public const string ExceptionCode = "DOM-FF-10001";
                }

                public struct FlagAlreadyExists
                {
                    public const string Message = "The feature flag {0} has been already been created for application {1} in environment {2}";
                    public const string ExceptionCode = "DOM-FF-10002";
                }

                public struct RequestValidationFailed
                {
                    public const string ExceptionCode = "DOM-10001";
                }
            }

            public struct EvaluationException
            {
                public const string DisplayMessage = "Feature Falge Evaluation Failed: {0}.\nCorrelation ID - {1}";
                public const string ExceptionCode = "DOM-EVAL-10001";
            }


            public struct AzureRequestException
            {
                public const string DisplayMessage = "OOPS! There was an error while connecting to Azure. Contact support with correlation ID - {0}";
            }

            public struct GraphException
            {
                public const string DisplayMessage = "Graph Service Failed: {0}.\nCorrelation ID - {1}";
            }

            public struct AccessForbiddenException
            {
                public const string Message = "Current user in not authorized to perform {0} on tenant - {1}";
                public const string DisplayMessage = "The user is not authorized to perform this operation. Please contact support with Correlation ID {0}";
            }

            public struct StorageException
            {
                public const string BlobFailureMessage = "Error occurred in downloading blob {0} from container {1} at storage account {2}";
                public const string BlobExceptionCode = "STG-BLOB-001";
                public const string DisplayMessage = "OOPS! There was an error in connecting to Azure Storage. Please contact support with Correlatio ID {0}";
            }

            public struct RulesEngineException
            {
                public const string Message = "There was an exception in evaluating rule engine {0} under tenant {1}";
                public const string GeneralCode = "BRE-001";
                public const string EvaluationFailureCode = "BRE-002";
                public const string DisplayMessage = "OOPS! There was an error in evaluating the Business Rule Engine. Please contact support with Correlation ID {0}";
            }
        }

        public struct FilterKeys
        {
            public const string Generic = "Generic";
            public const string Alias = "Alias";
            public const string RoleGroup = "RoleGroup";
            public const string Country = "Country";
            public const string Region = "Region";
            public const string Role = "Role";
            public const string Date = "Date";
            public const string UserUpn = "Userupn";
            public const string RulesEngine = "RulesEngine";
        }

        public struct FlightingContextParams
        {
            public const string Alias = "alias";
            public const string Upn = "upn";
            public const string Rolegroup = "rolegroup";
            public const string Country = "country";
            public const string Region = "region";
            public const string Role = "role";
        }

        public struct Caching
        {
            public const string UserObjectIdKey = "SG_{0}_OIDS";
            public const string UserUpnKey = "SG_{0}_UPNS";
        }

        public struct Authorization
        {
            public enum AuthorizationTypes
            {
                AuthorizationService = 0,
                Configuration = 1
            }
        }
    }
}
