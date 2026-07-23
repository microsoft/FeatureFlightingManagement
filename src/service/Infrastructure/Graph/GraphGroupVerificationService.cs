using System;
using System.Linq;
using System.Text;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Cache;
using Microsoft.FeatureFlighting.Common.Group;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Common.AppExceptions;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Concurrent;
using Azure.Identity;
using Azure.Core;
using System.Threading;

namespace Microsoft.FeatureFlighting.Infrastructure.Graph
{
    /// <inheritdoc>/>
    internal class GraphGroupVerificationService : IGroupVerificationService, IBackgroundCacheable<List<string>>
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly ICacheFactory _cacheFactory;
        private readonly bool _isCachingEnabled;
        private readonly int _cacheInterval;
        private readonly ILogger _logger;
        private readonly bool _verboseLogging;
        private readonly IDictionary<string, object> _cache;

        public event EventHandler<BackgroundCacheParameters> ObjectCached;

        public string CacheableServiceId => nameof(GraphGroupVerificationService);

        public GraphGroupVerificationService(IConfiguration configuration, ICacheFactory cacheFactory, ILogger logger)
        {
            _cacheFactory = cacheFactory;
            _logger = logger;
            _cache = new ConcurrentDictionary<string, object>();
            _cacheInterval = int.Parse(configuration["Graph:CacheExpiration"]);
            _isCachingEnabled = cacheFactory != null && _cacheInterval > 0 && bool.Parse(configuration["Graph:CachingEnabled"]);
            _graphServiceClient = CreateGraphClient(configuration);
            _verboseLogging = configuration["Logging:LogLevel:Default"].ToLowerInvariant() == "Debug".ToLowerInvariant();
        }

        /// <inheritdoc>/>
        public async Task<bool> IsMember(string userUpn, List<string> securityGroupIds, LoggerTrackingIds trackingIds)
        {
            try
            {
               foreach(string securityGroup in securityGroupIds)
                {
                    IList<string> groupMembers = await GetGroupMembers(securityGroup, trackingIds);
                    if (groupMembers.Any(member => member.ToLowerInvariant() == userUpn.ToLowerInvariant()))
                    {
                        LogDebugMessage(new StringBuilder().Append(userUpn).Append(" found in ").Append(securityGroup), trackingIds);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleGraphError(ex, trackingIds);
                return false;
            }
            return false;
        }

        [Obsolete("Use IsMember")]
        public async Task<bool> IsUserAliasPartOfSecurityGroup(string userAlias, List<string> securityGroupIds, LoggerTrackingIds trackingIds)
        {
            try
            {
                foreach (string securityGroup in securityGroupIds)
                {
                    IList<string> groupMembers = await GetGroupMembers(securityGroup, trackingIds);
                    if (groupMembers.Any(member => member.Split('@')[0].ToLowerInvariant().Equals(userAlias)))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleGraphError(ex, trackingIds);
                return false;
            }
            return false;
        }

        private void LogDebugMessage(StringBuilder message, LoggerTrackingIds trackingIds)
        {
            if (_verboseLogging)
                _logger.Log(message.ToString(), trackingIds.CorrelationId, trackingIds.TransactionId, "GraphGroupVerificationService");
        }

        private async Task<IList<string>> GetGroupMembers(string securityGroupId, LoggerTrackingIds trackingIds)
        {
            BackgroundCacheParameters userCacheParameter = new()
            {
                CacheKey = string.Format(Common.Constants.Caching.UserUpnKey, securityGroupId),
                ObjectId = securityGroupId,
                CacheDuration = _cacheInterval
            };
            List<string> groupMembers = await GetCachedObject(userCacheParameter, trackingIds);
            if (groupMembers != null) 
            {
                LogDebugMessage(new StringBuilder().Append(securityGroupId).Append(" members found in cache"), trackingIds);
                return groupMembers;
            }
                
 
            var cacheableGroupMembers = await CreateCacheableObject(userCacheParameter, trackingIds);
            groupMembers = cacheableGroupMembers?.Object ?? new();
            if (groupMembers != null)
            {
                LogDebugMessage(new StringBuilder().Append(securityGroupId).Append(" members added in cache"), trackingIds);
                await SetCacheObject(cacheableGroupMembers, trackingIds);
            }   

            return groupMembers ?? new();
        }
      
        private GraphServiceClient CreateGraphClient(IConfiguration configuration)
        {
            try
            {
                string tenant = configuration["Graph:Tenant"];
                string authority = string.Format(configuration["Graph:Authority"], tenant);
                string[] scopes = new string[] { configuration["Graph:Scope"] };
                string confidentialAppCacheKey = CreateConfidentialAppCacheKey(authority, configuration["Graph:ClientId"]);

#if DEBUG
                var certificate = GetCertificate("27D6D3122675FCC4FE11E4977A540FC74169E1F1");
                IConfidentialClientApplication client =
                    ConfidentialClientApplicationBuilder
                        .Create(configuration["Graph:ClientId"])
                        .WithAuthority(AzureCloudInstance.AzurePublic, "microsoft.onmicrosoft.com")
                        .WithCertificate(certificate, true)
                        .Build();

                _cache.Add(confidentialAppCacheKey, client);

#else
                var credential = ManagedIdentityHelper.GetTokenCredential();
                IConfidentialClientApplication client =
                ConfidentialClientApplicationBuilder
                    .Create(configuration["Graph:ClientId"])
                    .WithAuthority(new Uri(authority))
                    .WithClientAssertion((AssertionRequestOptions options) =>
                                                {
                                                    var accessToken = credential.GetToken(new TokenRequestContext(new string[] { $"api://AzureADTokenExchange/.default" }), CancellationToken.None);
                                                    return Task.FromResult(accessToken.Token);
                                                })
                    .Build();
            _cache.Add(confidentialAppCacheKey, client);
#endif

                GraphServiceClient graphServiceClient = new GraphServiceClient(
                    new MsalConfidentialClientAuthenticationProvider(client, scopes));
                return graphServiceClient;
            }
            catch (Exception ex)
            {
                throw HandleGraphError(ex, null);
            }
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

        private GraphException HandleGraphError(Exception error, LoggerTrackingIds? trackingIds)
        {
            GraphException graphException = new(
                message: error.Message,
                exceptionCode: "GRAPH-GEN-001",
                correlationId: trackingIds?.CorrelationId,
                transactionId: trackingIds?.TransactionId,
                source: "GraphApiAccessProvider.IsMemberOfSecurityGroup",
                innerException: error);
            _logger.Log(new ExceptionContext()
            {
                Exception = graphException,
                CorrelationId = trackingIds?.CorrelationId,
                TransactionId = trackingIds?.TransactionId
            });
            return graphException;
        }

        public async Task<List<string>?> GetCachedObject(BackgroundCacheParameters parameters, LoggerTrackingIds trackingIds)
        {
            ICache cache = _cacheFactory.Create("Default", "Graph", trackingIds.CorrelationId, trackingIds.TransactionId);
            return (await cache.GetList(parameters.CacheKey, trackingIds.CorrelationId, trackingIds.TransactionId))?.ToList();
        }

        public async Task SetCacheObject(BackgroundCacheableObject<List<string>> cacheableObject, LoggerTrackingIds trackingIds)
        {
            ICache cache = _cacheFactory.Create("Default", "Graph", trackingIds.CorrelationId, trackingIds.TransactionId);
            await cache.SetList(cacheableObject.CacheParameters.CacheKey, cacheableObject.Object, trackingIds.CorrelationId, trackingIds.TransactionId, _cacheInterval + 10);
            ObjectCached?.Invoke(this, cacheableObject.CacheParameters);
        }

        public async Task<BackgroundCacheableObject<List<string>>> CreateCacheableObject(BackgroundCacheParameters cacheParameters, LoggerTrackingIds trackingIds)
        {
            string cacheKey = cacheParameters.CacheKey;
            string groupId = cacheParameters.ObjectId;

            var transitiveMembersResponse = await _graphServiceClient.Groups[groupId]
                   .TransitiveMembers
                   .GetAsync()
                   .ConfigureAwait(false);

            var groupMembers = new List<DirectoryObject>();
            if (transitiveMembersResponse?.Value != null)
            {
                var pageIterator = PageIterator<DirectoryObject, DirectoryObjectCollectionResponse>
                    .CreatePageIterator(_graphServiceClient, transitiveMembersResponse, (member) =>
                    {
                        groupMembers.Add(member);
                        return true;
                    });
                await pageIterator.IterateAsync().ConfigureAwait(false);
            }

            List<string> userPrincipalNames = groupMembers
                .Where(member => member is User)?
                .Select(member => ((User)member).UserPrincipalName)?
                .ToList() ?? new();

            BackgroundCacheableObject<List<string>> cacheableGroupMembers = new()
            {
                Object = userPrincipalNames,
                CacheParameters = cacheParameters
            };
            return cacheableGroupMembers;
        }

        public async Task RebuildCache(BackgroundCacheParameters cacheParameters, LoggerTrackingIds trackingIds)
        {
            var cacheableObject = await CreateCacheableObject(cacheParameters, trackingIds).ConfigureAwait(false);
            if (cacheableObject.Object != null && cacheableObject.Object.Any())
                await SetCacheObject(cacheableObject, trackingIds).ConfigureAwait(false);
        }

        private sealed class MsalConfidentialClientAuthenticationProvider : IAuthenticationProvider
        {
            private readonly IConfidentialClientApplication _client;
            private readonly string[] _scopes;

            public MsalConfidentialClientAuthenticationProvider(IConfidentialClientApplication client, string[] scopes)
            {
                _client = client;
                _scopes = scopes;
            }

            public async Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object> additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
            {
                AuthenticationResult authResult = await _client
                    .AcquireTokenForClient(_scopes)
                    .ExecuteAsync(cancellationToken)
                    .ConfigureAwait(false);

                request.Headers.Add("Authorization", $"Bearer {authResult.AccessToken}");
            }
        }
    }
}
