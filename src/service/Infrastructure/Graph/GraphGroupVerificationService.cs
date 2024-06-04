using System;
using System.Linq;
using System.Text;
using Microsoft.Graph;
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

namespace Microsoft.FeatureFlighting.Infrastructure.Graph
{
    /// <inheritdoc>/>
    internal class GraphGroupVerificationService : IGroupVerificationService, IBackgroundCacheable<List<string>>
    {
        private readonly IGraphServiceClient _graphServiceClient;
        private readonly ICacheFactory _cacheFactory;
        private readonly bool _isCachingEnabled;
        private readonly int _cacheInterval;
        private readonly ILogger _logger;
        private readonly bool _verboseLogging;

        public event EventHandler<BackgroundCacheParameters> ObjectCached;

        public string CacheableServiceId => nameof(GraphGroupVerificationService);

        public GraphGroupVerificationService(IConfiguration configuration, ICacheFactory cacheFactory, ILogger logger)
        {
            _cacheFactory = cacheFactory;
            _logger = logger;
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
      
        private IGraphServiceClient CreateGraphClient(IConfiguration configuration)
        {
            try
            {
                string tenant = configuration["Graph:Tenant"];
                string authority = string.Format(configuration["Graph:Authority"], tenant);
                string[] scopes = new string[] { configuration["Graph:Scope"] };
                //string secretLocation = configuration["Graph:ClientSecretLocation"];
                //string clientSecret = configuration[secretLocation];

                IConfidentialClientApplication confidentialClient = ConfidentialClientApplicationBuilder
                    .Create(configuration["Graph:ClientId"])
                    .WithAuthority(authority)
                    //.WithClientSecret(clientSecret)
                    .Build();

                IGraphServiceClient graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
                {
                    AuthenticationResult authResult = await confidentialClient
                        .AcquireTokenForClient(scopes)
                        .ExecuteAsync();

                    requestMessage.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                }));
                return graphServiceClient;
            }
            catch (Exception ex)
            {
                throw HandleGraphError(ex, null);
            }
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

            var transitiveMembers = await _graphServiceClient.Groups[groupId]
                   .TransitiveMembers
                   .Request()
                   .GetAsync()
                   .ConfigureAwait(false);

            var groupMembers = transitiveMembers?.ToList() ?? new();
            while (transitiveMembers != null && transitiveMembers.NextPageRequest != null)
            {
                transitiveMembers = await transitiveMembers.NextPageRequest.GetAsync().ConfigureAwait(false);
                if (transitiveMembers != null)
                    groupMembers.AddRange(transitiveMembers?.ToList() ?? new());
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
    }
}
