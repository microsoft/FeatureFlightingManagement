using System;
using System.Linq;
using Microsoft.Graph;
using System.Globalization;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using AppInsights.EnterpriseTelemetry;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Services.Interfaces;
using Microsoft.FeatureFlighting.Common.AppExcpetions;

namespace Microsoft.FeatureFlighting.Services
{
    [ExcludeFromCodeCoverage]
    public class GraphApiAccessProvider : IGraphApiAccessProvider
    {
        private readonly ILogger _logger;
        private readonly ICacheFactory _cacheFactory;
        private readonly bool _isCachingEnabled;
        private readonly int _cacheInterval;
        private readonly IGraphServiceClient graphServiceClient;

        public GraphApiAccessProvider(IConfiguration configuration, ICacheFactory cacheFactory, ILogger logger)
        {
            IConfiguration _configuration = configuration;
            _cacheFactory = cacheFactory;
            _logger = logger;
            var tenant = _configuration.GetValue<string>("Graph:Tenant");
            var authority = string.Format(_configuration.GetValue<string>("Graph:Authority"), tenant);
            var scopes = new string[] { _configuration.GetValue<string>("Graph:Scope") };
            _cacheInterval = _configuration.GetValue<int>("Graph:CacheExpiration");
            _isCachingEnabled = cacheFactory != null && _cacheInterval > 0 && _configuration.GetValue<bool>("Graph:CachingEnabled");

            try
            {
                IConfidentialClientApplication confidentialClient = ConfidentialClientApplicationBuilder
                    .Create(_configuration.GetValue<string>("Graph:ClientId"))
                    .WithAuthority(authority)
                    .WithClientSecret(_configuration.GetValue<string>("GraphClientSecret"))
                    .Build();

                graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
                    {
                        var authResult = await confidentialClient
                            .AcquireTokenForClient(scopes)
                            .ExecuteAsync();

                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                    }));
            }
            catch (Exception ex)
            {
                HandleGraphError(ex, null);
            }
        }

        public async Task<bool> IsUserAliasPartOfSecurityGroup(string userAlias, List<string> securityGroupIds, LoggerTrackingIds trackingIds)
        {
            var userUpn = userAlias;
            if (!userAlias.Contains("@microsoft.com"))
                userUpn += "@microsoft.com";

            return await IsUserUpnPartOfSecurityGroup(userUpn, securityGroupIds, trackingIds);
        }

        public async Task<bool> IsUserUpnPartOfSecurityGroup(string userUpn, List<string> groupOids, LoggerTrackingIds trackingIds)
        {
            try
            {
                var cachedUsers = await GetCachedUserPrincipalNames(securityGroupIds: groupOids, trackingIds);
                if (cachedUsers != null && cachedUsers.Any())
                    return cachedUsers.Any(user => userUpn.ToLowerInvariant() == user.ToLowerInvariant());

                var memberGroups = await graphServiceClient.Users[userUpn]
                    .CheckMemberGroups(groupOids)
                    .Request()
                    .PostAsync();

                await CacheUsers(securityGroupIds: groupOids, trackingIds);
                if (memberGroups.Any())
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                HandleGraphError(ex, trackingIds);
                return false;
            }
        }

        private async Task<List<string>> GetCachedUserPrincipalNames(List<string> securityGroupIds, LoggerTrackingIds trackingIds)
        {
            try
            {
                if (!_isCachingEnabled)
                    return null;

                var cache = _cacheFactory.Create("Default", "Graph", trackingIds.CorrelationId, trackingIds.TransactionId);
                var cachingTasks = new List<Task<List<string>>>();
                foreach (var securityGroupId in securityGroupIds)
                {
                    var cacheKey = GetUserPrincipalNamesCacheKey(securityGroupId);
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

            List<string> cachedObjectIds = new List<string>();
            List<string> cachedUserPrincipalNames = new List<string>();

            var cache = _cacheFactory.Create("Default", "Graph", trackingIds.CorrelationId, trackingIds.TransactionId);
            var transitiveMembers = (await graphServiceClient.Groups[securityGroupId]
                    .TransitiveMembers
                    .Request()
                    .GetAsync()
                    .ConfigureAwait(false));
            var groupMembers = transitiveMembers.ToList();

            while (transitiveMembers != null && transitiveMembers.NextPageRequest != null)
            {
                transitiveMembers = await transitiveMembers.NextPageRequest.GetAsync();
                if (transitiveMembers != null)
                    groupMembers.AddRange(transitiveMembers.ToList());
            }

            foreach (var member in groupMembers)
            {
                if (member is User)
                {
                    cachedObjectIds.Add(member.Id);
                    cachedUserPrincipalNames.Add((member as User).UserPrincipalName);
                }
            }

            await cache.SetList(GetUserObjectIdsCacheKey(securityGroupId), cachedObjectIds, trackingIds.CorrelationId, trackingIds.TransactionId, _cacheInterval).ConfigureAwait(false);
            await cache.SetList(GetUserPrincipalNamesCacheKey(securityGroupId), cachedUserPrincipalNames, trackingIds.CorrelationId, trackingIds.TransactionId, _cacheInterval).ConfigureAwait(false);
        }

        private string GetUserPrincipalNamesCacheKey(string securityGroupId) =>
            string.Format(Common.Constants.Caching.UserObjectIdKey, securityGroupId);

        private string GetUserObjectIdsCacheKey(string securityGroupId) =>
            string.Format(Common.Constants.Caching.UserObjectIdKey, securityGroupId);


        public void HandleGraphError(Exception error, LoggerTrackingIds trackingIds)
        {
            var graphException = new GraphException(
                message: error.Message,
                exceptionCode: "",
                correlationId: trackingIds.CorrelationId,
                transactionId: trackingIds.TransactionId,
                failedMethod: "GraphApiAccessProvider.IsMemberOfSecurityGroup",
                innerException: error);
            _logger.Log(new ExceptionContext()
            {
                Exception = graphException,
                CorrelationId = trackingIds.CorrelationId,
                TransactionId = trackingIds.TransactionId
            });
        }
    }
}
