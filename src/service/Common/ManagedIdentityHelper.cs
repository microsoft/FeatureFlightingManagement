using Azure.Core;
using Azure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FeatureFlighting.Common
{
    public class ManagedIdentityHelper
    {
        /// <summary>
        /// Get the token credential based on the environment (Debug/Release).
        /// </summary>
        /// <returns>Token Credential</returns>
        public static TokenCredential GetTokenCredential()
        {
            TokenCredential credential = null;

#if DEBUG
            // Use AzureCliCredential in local/debug scenarios: it authenticates with the identity
            // from `az login`, avoiding the VisualStudioCredential token-service hangs/timeouts that
            // can cause Key Vault / App Configuration loading to fail. Make sure you are signed in
            // (`az login`) with an account that has access to the target Key Vault and App Configuration.
            credential = new AzureCliCredential();
#else
            credential = new ManagedIdentityCredential();
#endif

            return credential;
        }
    }
}
