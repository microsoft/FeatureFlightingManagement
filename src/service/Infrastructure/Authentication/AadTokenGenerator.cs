using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.FeatureFlighting.Common.Authentication;

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
        public async Task<string> GenerateToken(string authority, string clientId, string clientSecret, string resourceId)
        {
            IConfidentialClientApplication client = GetOrCreateConfidentialApp(authority, clientId, clientSecret);
            var scopes = new string[] { resourceId };
            AuthenticationResult authenticationResult = await client
                .AcquireTokenForClient(scopes)
                .ExecuteAsync();
            return authenticationResult.AccessToken;
        }

        private IConfidentialClientApplication GetOrCreateConfidentialApp(string authority, string clientId, string clientSecret)
        {
            string confidentialAppCacheKey = CreateConfidentialAppCacheKey(authority, clientId);
            if (_cache.ContainsKey(confidentialAppCacheKey))
            {
                if (_cache[confidentialAppCacheKey] is IConfidentialClientApplication cachedClient)
                {
                    return cachedClient;
                }
            }
            
            IConfidentialClientApplication client =
                ConfidentialClientApplicationBuilder
                    .Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithAuthority(new Uri(authority))
                    .Build();
            _cache.Add(confidentialAppCacheKey, client);
            return client;
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
