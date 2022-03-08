using System;
using System.Linq;
using Microsoft.Graph;
using System.Globalization;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Group;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Common.AppExceptions;

namespace Microsoft.FeatureFlighting.Infrastructure.Graph
{   
    /// <inheritdoc>/>
    internal class GraphGroupVerificationService: IGroupVerificationService
    {
        private readonly IGraphServiceClient _graphServiceClient;
        private readonly ICacheFactory _cacheFactory;
        private readonly bool _isCachingEnabled;
        private readonly int _cacheInterval;
        private readonly ILogger _logger;

        public GraphGroupVerificationService(IConfiguration configuration, ICacheFactory cacheFactory, ILogger logger)
        {
            _cacheFactory = cacheFactory;
            _logger = logger;
            _cacheInterval = int.Parse(configuration["Graph:CacheExpiration"]);
            _isCachingEnabled = cacheFactory != null && _cacheInterval > 0 && bool.Parse(configuration["Graph:CachingEnabled"]);
            _graphServiceClient = CreateGraphClient(configuration);
        }

        /// <inheritdoc>/>
        public async Task<bool> IsMember(string userUpn, List<string> groupOids, LoggerTrackingIds trackingIds)
        {
            try
            {
                IList<string>? cachedUsers = await GetCachedUserPrincipalNames(securityGroupIds: groupOids, trackingIds);
                if (cachedUsers != null && cachedUsers.Any())
                    return cachedUsers.Any(user => userUpn.ToLowerInvariant() == user.ToLowerInvariant());

                var memberGroups = await _graphServiceClient.Users[userUpn]
                    .CheckMemberGroups(groupOids)
                    .Request()
                    .PostAsync();

                await CacheUsers(securityGroupIds: groupOids, trackingIds);
                return memberGroups.Any();
            }
            catch (Exception ex)
            {
                HandleGraphError(ex, trackingIds);
                return false;
            }
        }

        /// <inheritdoc>/>
        [Obsolete("Checking graph by Alias has issues. Use IsMember and pass the User UPN")]
        public async Task<bool> IsUserAliasPartOfSecurityGroup(string userAlias, List<string> securityGroupIds, LoggerTrackingIds trackingIds)
        {
            IList<string>? cachedUsers = await GetCachedUserPrincipalNames(securityGroupIds, trackingIds);
            if (cachedUsers != null && cachedUsers.Any())
            {
                return cachedUsers.Any(user => user.StartsWith(userAlias, ignoreCase: true, culture: CultureInfo.InvariantCulture));
            }

            foreach (var securityGroupId in securityGroupIds)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(securityGroupId))
                        return false;
                    var transitiveMembers = await _graphServiceClient.Groups[securityGroupId]
                        .TransitiveMembers
                        .Request()
                        .GetAsync();
                    if (transitiveMembers == null || !transitiveMembers.Any())
                        continue;
                    do
                    {
                        if (transitiveMembers.Any(member => (member is User) && (member as User).UserPrincipalName.StartsWith(userAlias, ignoreCase: true, culture: CultureInfo.InvariantCulture)))
                        {
                            await CacheUsers(securityGroupIds, trackingIds);
                            return true;
                        }
                        if (transitiveMembers.NextPageRequest == null)
                            break;
                        transitiveMembers = await transitiveMembers.NextPageRequest.GetAsync();
                    } while (transitiveMembers != null);
                }
                catch (Exception ex)
                {
                    HandleGraphError(ex, trackingIds);
                    return false;
                }
            }
            await CacheUsers(securityGroupIds, trackingIds);
            return false;
        }

        private async Task<IList<string>?> GetCachedUserPrincipalNames(List<string> securityGroupIds, LoggerTrackingIds trackingIds)
        {
            try
            {
                if (!_isCachingEnabled)
                    return null;

                var cache = _cacheFactory.Create("Default", "Graph", trackingIds.CorrelationId, trackingIds.TransactionId);
                var cachingTasks = new List<Task<IList<string>>>();
                foreach (var securityGroupId in securityGroupIds)
                {
                    string cacheKey = GetUserPrincipalNamesCacheKey(securityGroupId);
                    cachingTasks.Add(cache.GetList(cacheKey, trackingIds.CorrelationId, trackingIds.TransactionId));
                }
                await Task.WhenAll(cachingTasks).ConfigureAwait(false);
                return cachingTasks.SelectMany(task => task.Result ?? new List<string>()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Log(new Exception("An error ocurred while getting cached UPNs", ex), trackingIds.CorrelationId, trackingIds.TransactionId, "GraphApiAccessProvider:GetCachedUserPrincipalNames");
                return null;
            }
        }

        private async Task CacheUsers(List<string> securityGroupIds, LoggerTrackingIds trackingIds)
        {
            try
            {
                var cachingTasks = new List<Task>();
                foreach (var securityGroupId in securityGroupIds)
                {
                    cachingTasks.Add(CacheUsers(securityGroupId, trackingIds));
                }
                await Task.WhenAll(cachingTasks);
            }
            catch (Exception ex)
            {
                _logger.Log(new Exception("An error ocurred while caching users", ex), trackingIds.CorrelationId, trackingIds.TransactionId, "GraphApiAccessProvider:GetCachedUserPrincipalNames");
            }
        }

        private async Task CacheUsers(string securityGroupId, LoggerTrackingIds trackingIds)
        {
            if (!_isCachingEnabled)
                return;
            
            ICache cache = _cacheFactory.Create("Default", "Graph", trackingIds.CorrelationId, trackingIds.TransactionId);
            List<string> cachedObjectIds = new();
            List<string> cachedUserPrincipalNames = new();

            var transitiveMembers = await _graphServiceClient.Groups[securityGroupId]
                    .TransitiveMembers
                    .Request()
                    .GetAsync()
                    .ConfigureAwait(false);
            var groupMembers = transitiveMembers.ToList();
            while (transitiveMembers != null && transitiveMembers.NextPageRequest != null)
            {
                transitiveMembers = await transitiveMembers.NextPageRequest.GetAsync();
                if (transitiveMembers != null)
                    groupMembers.AddRange(transitiveMembers.ToList());
            }

            foreach (var member in groupMembers)
            {
                if (member is not User user)
                    continue;

                cachedObjectIds.Add(member.Id);
                cachedUserPrincipalNames.Add(user.UserPrincipalName);
            }

            await cache.SetList(GetUserObjectIdsCacheKey(securityGroupId), cachedObjectIds, trackingIds.CorrelationId, trackingIds.TransactionId, _cacheInterval).ConfigureAwait(false);
            await cache.SetList(GetUserPrincipalNamesCacheKey(securityGroupId), cachedUserPrincipalNames, trackingIds.CorrelationId, trackingIds.TransactionId, _cacheInterval).ConfigureAwait(false);
        }

        private string GetUserPrincipalNamesCacheKey(string securityGroupId) =>
            string.Format(Common.Constants.Caching.UserObjectIdKey, securityGroupId);

        private string GetUserObjectIdsCacheKey(string securityGroupId) =>
            string.Format(Common.Constants.Caching.UserObjectIdKey, securityGroupId);

        private IGraphServiceClient CreateGraphClient(IConfiguration configuration)
        {
            try
            {
                string tenant = configuration["Graph:Tenant"];
                string authority = string.Format(configuration["Graph:Authority"], tenant);
                string[] scopes = new string[] { configuration["Graph:Scope"] };
                string secretLocation = configuration["Graph:ClientSecretLocation"];
                string clientSecret = configuration[secretLocation];

                IConfidentialClientApplication confidentialClient = ConfidentialClientApplicationBuilder
                    .Create(configuration["Graph:ClientId"])
                    .WithAuthority(authority)
                    .WithClientSecret(clientSecret)
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
    }
}
