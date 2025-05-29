using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.FeatureFlighting.Common.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using Azure.Core;
using Azure.Identity;
using System.Threading;

namespace Microsoft.FeatureFlighting.Infrastructure.Authentication
{
    /// <summary>
    /// Generates authentication token using MSAL
    /// </summary>
    internal class AadTokenGenerator: ITokenGenerator
    {
        private readonly IDictionary<string, object> _cache;

        public AadTokenGenerator()
        {
            _cache = new ConcurrentDictionary<string, object>();
        }

        // <inheritdoc/>
        public async Task<string> GenerateToken(string authority, string clientId, string resourceId, string userAssignedClientId)
        {
            IConfidentialClientApplication client = GetOrCreateConfidentialApp(authority, clientId, userAssignedClientId);
            var scopes = new string[] { resourceId };
            AuthenticationResult authenticationResult = await client
                .AcquireTokenForClient(scopes)
                .ExecuteAsync();
            return authenticationResult.AccessToken;
        }

        private IConfidentialClientApplication GetOrCreateConfidentialApp(string authority, string clientId, string userAssignedClientId)
        {
            string confidentialAppCacheKey = CreateConfidentialAppCacheKey(authority, clientId);
            if (_cache.ContainsKey(confidentialAppCacheKey))
            {
                if (_cache[confidentialAppCacheKey] is IConfidentialClientApplication cachedClient)
                {
                    return cachedClient;
                }
            }
#if DEBUG
            var certificate = GetCertificate("27D6D3122675FCC4FE11E4977A540FC74169E1F1");
            IConfidentialClientApplication client =
                ConfidentialClientApplicationBuilder
                    .Create(clientId)                    
                    .WithAuthority(AzureCloudInstance.AzurePublic, "microsoft.onmicrosoft.com")
                    .WithCertificate(certificate,true)
                    .Build();
            _cache.Add(confidentialAppCacheKey, client);
            return client;

#else
            var credential = new ManagedIdentityCredential(userAssignedClientId);

            IConfidentialClientApplication client =
                ConfidentialClientApplicationBuilder
                    .Create(clientId)
                    .WithAuthority(new Uri(authority))
                    .WithClientAssertion((AssertionRequestOptions options) =>
                                                {
                                                    var accessToken = credential.GetToken(new TokenRequestContext(new string[] { $"api://AzureADTokenExchange/.default" }), CancellationToken.None);
                                                    return Task.FromResult(accessToken.Token);
                                                })
                    .Build();
            _cache.Add(confidentialAppCacheKey, client);
            return client;
#endif
        }

        public X509Certificate2 GetCertificate(string certificateThumbprint)
        {
            var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            var cert = store.Certificates.OfType<X509Certificate2>()
                .FirstOrDefault(x => x.Thumbprint == certificateThumbprint);
            store.Close();
            return cert;
        }
        private string CreateConfidentialAppCacheKey(string authority, string clientId)
        {
            return new StringBuilder()
                .Append(authority)
                .Append("-")
                .Append(clientId)
                .ToString()
                .ToUpperInvariant();
        }
    }
}
