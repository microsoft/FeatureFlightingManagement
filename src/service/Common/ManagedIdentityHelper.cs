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
            credential = new VisualStudioCredential();
#else
            credential = new ManagedIdentityCredential();
#endif

            return credential;
        }
    }
}
