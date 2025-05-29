using Azure.Identity;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;

namespace Microsoft.FeatureFlighting.Tests.Functional.Helper
{
    public sealed class KeyVaultHelper
    {
        private KeyVaultHelper() { }

        private static KeyVaultHelper instance = null;
        private static readonly IDictionary<string, string> SecretsCache = new ConcurrentDictionary<string, string>();
        private static readonly object _lock = new();
        public static KeyVaultHelper Instance
        {
            get
            {
                lock(_lock)
                {
                    if (instance == null)
                    {
                        instance = new KeyVaultHelper();
                    }
                    return instance;
                }
            }
        }

        public async Task<string> GetSecret(string keyVaultEndpoint, string secretName,string userAssignedClientId)
        {
            if (SecretsCache.ContainsKey(secretName))
                return SecretsCache[secretName];
            TokenCredential credential;

            #if DEBUG
                credential = new VisualStudioCredential();
            #else
                credential = new ManagedIdentityCredential(
                ManagedIdentityId.FromUserAssignedClientId(userAssignedClientId));
            #endif
            
            SecretClient client = new(new System.Uri(keyVaultEndpoint), credential);
            KeyVaultSecret secret = await client.GetSecretAsync(secretName);
            return secret.Value;
        }
    }
}