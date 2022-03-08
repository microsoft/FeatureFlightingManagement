using Azure.Identity;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Azure.Security.KeyVault.Secrets;

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

        public async Task<string> GetSecret(string keyVaultEndpoint, string secretName)
        {
            if (SecretsCache.ContainsKey(secretName))
                return SecretsCache[secretName];

            SecretClient client = new(new System.Uri(keyVaultEndpoint), new DefaultAzureCredential());
            KeyVaultSecret secret = await client.GetSecretAsync(secretName);
            return secret.Value;
        }
    }
}