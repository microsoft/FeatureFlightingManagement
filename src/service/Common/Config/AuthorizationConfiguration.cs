using System.Linq;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Common.Config
{
    /// <summary>
    /// Configuration for authorizing apps to administer the tenant's feature flags in Azure App Configuration
    /// </summary>
    public class AuthorizationConfiguration
    {
        public AuthorizationConfiguration() { }

        /// <summary>
        /// Type of authorization (configuration or service-based)
        /// </summary>
        /// <value>Configuration</value>
        public string Type { get; set; }

        /// <summary>
        /// Comma-separated UPN (for user) or App ID (for applications) of administrators
        /// </summary>
        public string Administrators { get; set; }

        /// <summary>
        /// Name of the current application
        /// </summary>
        public string SenderAppName { get; set; }

        /// <summary>
        /// Gets a list of administrators (split from comma-separated Administers field)
        /// </summary>
        /// <returns>UPN (for user) or App ID (for applications) of administrators</returns>
        public IEnumerable<string> GetAdministrators()
        {
            if (string.IsNullOrWhiteSpace(Administrators))
                return new List<string>();

            return Administrators.Split(",").ToList();
        }

        /// <summary>
        /// Merges the configuration values with default authorization configuration
        /// </summary>
        /// <param name="defaultConfiguration" cref="AuthorizationConfiguration">Configuration with default values</param>
        public void MergeWithDefault(AuthorizationConfiguration defaultConfiguration)
        {
            if (defaultConfiguration == null)
                return;

            Type = !string.IsNullOrWhiteSpace(Type) ? Type : defaultConfiguration.Type;
            Administrators = !string.IsNullOrWhiteSpace(Administrators) ? Administrators : defaultConfiguration.Administrators;
            SenderAppName = !string.IsNullOrWhiteSpace(SenderAppName) ? SenderAppName : defaultConfiguration.SenderAppName;
        }
    }
}
