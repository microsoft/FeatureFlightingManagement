using Azure;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using System.Collections.Generic;
using Azure.Data.AppConfiguration;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Common.Caching;
using Microsoft.FeatureFlighting.Domain.Interfaces;
using Microsoft.FeatureFlighting.Common.AppExcpetions;

namespace Microsoft.FeatureFlighting.Domain.Configuration
{
    public class FeatureFlagManager : IFeatureFlagManager
    {
        private readonly IConfiguration _configuration;
        private readonly IConfigurationClientProvider _configurationClientProvider;
        private readonly ICacheFactory _cacheFactory;
        private readonly ILogger _logger;
        private readonly string _featureFlagContenetType = "application/vnd.microsoft.appconfig.ff+json;charset=utf-8";
        private readonly string _featureFlagPrefix = ".appconfig.featureflag/";
        private readonly JsonSerializerOptions _defaultSerializationOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public FeatureFlagManager(IConfiguration configuration, IConfigurationClientProvider configurationClientProvider, ICacheFactory cacheFactory, ILogger logger)
        {
            _configuration = configuration;
            _configurationClientProvider = configurationClientProvider;
            _cacheFactory = cacheFactory;
            _logger = logger;
        }

        public async Task CreateFeatureFlag(string appName, string envName, FeatureFlag featureFlag, LoggerTrackingIds trackingIds)
        {
            try
            {
                ValidateFeatureFlag(featureFlag, "UNKNOWN", appName, envName);
                featureFlag.Environment = envName;
                featureFlag.Id = Utility.GetFeatureFlagId(appName, envName, featureFlag.Name);
                var value = JsonConvert.SerializeObject(featureFlag);

                // Create a Configuration Setting to be stored in the Configuration Store.
                var setting = new ConfigurationSetting(_featureFlagPrefix + featureFlag.Id, value, _configuration["Env:Label"])
                {
                    ContentType = _featureFlagContenetType
                };

                var client = _configurationClientProvider.GetConfigurationClient();
                await client.SetConfigurationSettingAsync(setting);
                await DeleteCachedFeatureFlags(appName, envName, trackingIds);
            }
            catch (RequestFailedException rex)
            {
                if (rex.Status == 412)
                    throw new DomainException(message:
                        string.Format(Constants.Exception.DomainException.FlagAlreadyExists.Message, featureFlag.Name, appName, envName),
                        Constants.Exception.DomainException.FlagAlreadyExists.ExceptionCode);

                throw new AzureRequestException(
                   message: rex.Message,
                   statusCode: rex.Status,
                   correlationId: trackingIds.CorrelationId,
                   transactionId: trackingIds.TransactionId,
                   failedMethod: "FeatureFlagManger.CreateFeatureFlag",
                   innerException: rex);
            }
        }

        public async Task UpdateFeatureFlag(string appName, string envName, FeatureFlag featureFlag, LoggerTrackingIds trackingIds)
        {
            try
            {
                ValidateFeatureFlag(featureFlag, featureFlag?.Name, appName, envName);
                featureFlag.Environment = envName;
                featureFlag.Id = Utility.GetFeatureFlagId(appName, envName, featureFlag.Name);

                var value = JsonConvert.SerializeObject(featureFlag);

                // Create a Configuration Setting to be stored in the Configuration Store.
                var setting = new ConfigurationSetting(_featureFlagPrefix + featureFlag.Id, value, _configuration["Env:Label"])
                {
                    ContentType = _featureFlagContenetType
                };

                var client = _configurationClientProvider.GetConfigurationClient();
                await client.SetConfigurationSettingAsync(setting);
                await DeleteCachedFeatureFlags(appName, envName, trackingIds);
            }
            catch (RequestFailedException rex)
            {
                throw new AzureRequestException(
                   message: rex.Message,
                   statusCode: rex.Status,
                   correlationId: trackingIds.CorrelationId,
                   transactionId: trackingIds.TransactionId,
                   failedMethod: "FeatureFlagManger.UpdateFeatureFlag",
                   innerException: rex);
            }
        }

        public async Task<FeatureFlag> GetFeatureFlag(string appName, string envName, string name, LoggerTrackingIds trackingIds)
        {
            try
            {
                var key = Utility.GetFeatureFlagId(appName, envName, name);
                var client = _configurationClientProvider.GetConfigurationClient();
                var res = await client.GetConfigurationSettingAsync(_featureFlagPrefix + key, _configuration["Env:Label"]);
                var featureflag = JsonConvert.DeserializeObject<FeatureFlag>(res.Value.Value);

                return featureflag;
            }
            catch (RequestFailedException rex)
            {
                _logger.Log(new ExceptionContext()
                {
                    Exception = rex,
                    CorrelationId = trackingIds.CorrelationId,
                    TransactionId = trackingIds.TransactionId
                });

                return null;
            }
        }

        public async Task UpdateFeatureFlagStatus(string appName, string envName, FeatureFlag featureFlag, bool status, LoggerTrackingIds trackingIds)
        {
            var flagName = featureFlag?.Name;
            featureFlag = await GetFeatureFlag(appName, envName, flagName, trackingIds);
            ValidateFeatureFlag(featureFlag, flagName, appName, envName);
            featureFlag.Enabled = status;
            await UpdateFeatureFlag(appName, envName, featureFlag, trackingIds);
            await DeleteCachedFeatureFlags(appName, envName, trackingIds);
        }

        public async Task<IList<FeatureFlagDto>> GetFeatureFlags(string appName, string envName, LoggerTrackingIds trackingIds)
        {
            var cachedFeatureFlags = await GetCachedFeatureFlags(appName, envName, trackingIds);
            if (cachedFeatureFlags != null && cachedFeatureFlags.Any())
                return cachedFeatureFlags;

            var selector = new SettingSelector()
            {
                KeyFilter = _featureFlagPrefix + appName.ToLowerInvariant() + "_" + envName.ToLowerInvariant() + "*",
                LabelFilter = _configuration["Env:Label"]
            };

            var client = _configurationClientProvider.GetConfigurationClient();

            IList<FeatureFlagDto> featureFlagsDTO = new List<FeatureFlagDto>();

            await foreach (ConfigurationSetting setting in client.GetConfigurationSettingsAsync(selector))
            {
                var featureFlag = JsonConvert.DeserializeObject<FeatureFlag>(setting.Value);
                var dto = new FeatureFlagDto()
                {
                    Id = featureFlag.Id,
                    Environment = featureFlag.Environment,
                    Enabled = featureFlag.Enabled,
                    Name = featureFlag.Name,
                };

                if (featureFlag.Conditions != null && featureFlag.Conditions.Client_Filters != null && featureFlag.Conditions.Client_Filters.Any())
                {
                    dto.Stages = new List<Stage>();
                    IList<Filter> filters = new List<Filter>(featureFlag.Conditions.Client_Filters);

                    if (filters.Count > 1)
                    {
                        var distinct = filters.Distinct(new FilterComparer());
                        var sortedList = distinct.OrderBy(o => o.Parameters.StageId);

                        var i = 1;
                        foreach (Filter filter in sortedList)
                        {
                            if (filter is null)
                                continue;

                            var stage = new Stage
                            {
                                IsActive = Convert.ToBoolean(filter.Parameters.IsActive),
                                StageId = Convert.ToInt32(filter.Parameters.StageId),
                                StageName = filter.Parameters.StageName,
                                IsFirstStage = i == 1,
                                IsLastStage = i == sortedList.Count()
                            };
                            dto.Stages.Add(stage);
                            i++;
                        }
                    }
                    else
                    {
                        if (filters[0] is null)
                            continue;

                        var stage = new Stage
                        {
                            IsActive = Convert.ToBoolean(filters[0].Parameters.IsActive),
                            StageId = Convert.ToInt32(filters[0].Parameters.StageId),
                            StageName = filters[0].Parameters.StageName,
                            IsFirstStage = true,
                            IsLastStage = true
                        };
                        dto.Stages.Add(stage);
                    }
                }

                featureFlagsDTO.Add(dto);
            }

            await AddCachedFeatureFlags(appName, envName, featureFlagsDTO, trackingIds);
            return featureFlagsDTO;
        }

        public async Task<IList<string>> GetFeatures(string appName, string envName, LoggerTrackingIds trackingIds, bool burstCache = false)
        {
            if (!burstCache)
            {
                var cachedFeatureFlags = await GetCachedFeatureFlagNames(appName, envName, trackingIds);
                if (cachedFeatureFlags != null && cachedFeatureFlags.Any())
                    return cachedFeatureFlags;
            }
            else
            {
                await DeleteCachedFeatureFlags(appName, envName, trackingIds);
            }

            var featureFlags = await GetFeatureFlags(appName, envName, trackingIds);
            if (featureFlags != null)
                return featureFlags.Select(flag => flag.Name).ToList();
            return null;
        }

        public async Task ActivateStage(string appName, string envName, FeatureFlag featureFlag, string stage, LoggerTrackingIds trackingIds)
        {
            var flagName = featureFlag?.Name;
            featureFlag = await GetFeatureFlag(appName, envName, flagName, trackingIds);
            ValidateFeatureFlag(featureFlag, flagName, appName, envName);
            foreach (var filter in featureFlag.Conditions.Client_Filters)
            {
                filter.Parameters.IsActive = Convert.ToString(filter.Parameters.StageName == stage);
            }
            await UpdateFeatureFlag(appName, envName, featureFlag, trackingIds);
            await DeleteCachedFeatureFlags(appName, envName, trackingIds);
        }

        public async Task DeleteFeatureFlag(string appName, string envName, string featureName, LoggerTrackingIds trackingIds)
        {
            try
            {
                var featureFlag = await GetFeatureFlag(appName, envName, featureName, trackingIds);
                ValidateFeatureFlag(featureFlag, featureName, appName, envName);
                var featureId = featureFlag.Id;
                var client = _configurationClientProvider.GetConfigurationClient();
                await client.DeleteConfigurationSettingAsync(_featureFlagPrefix + featureId, _configuration["Env:Label"]);
                await DeleteCachedFeatureFlags(appName, envName, trackingIds);
            }
            catch (RequestFailedException rex)
            {
                throw new AzureRequestException(
                   message: rex.Message,
                   statusCode: rex.Status,
                   correlationId: trackingIds.CorrelationId,
                   transactionId: trackingIds.TransactionId,
                   failedMethod: "FeatureFlagManger.DeleteFeatureFlag",
                   innerException: rex);
            }
        }

        #region Private::Cache
        private async Task<IList<string>> GetCachedFeatureFlagNames(string appName, string environment, LoggerTrackingIds trackingIds)
        {
            var secondaryCache = _cacheFactory.Create(appName, operation: "FeatureFlagNames", trackingIds.CorrelationId, trackingIds.TransactionId);
            if (secondaryCache == null)
                return null;

            var cacheKey = GetFeatureFlagsCacheKey(appName, environment);
            var cachedFlags = await secondaryCache.GetList(cacheKey, trackingIds.CorrelationId, trackingIds.TransactionId);
            if (cachedFlags == null || !cachedFlags.Any())
                return null;
            return cachedFlags;
        }

        private async Task<IList<FeatureFlagDto>> GetCachedFeatureFlags(string appName, string environment, LoggerTrackingIds trackingIds)
        {
            var primaryCache = _cacheFactory.Create(appName, operation: "FeatureFlags", trackingIds.CorrelationId, trackingIds.TransactionId);
            if (primaryCache == null)
                return null;

            var cacheKey = GetFeatureFlagsCacheKey(appName, environment);
            var cachedFlags = await primaryCache.GetList(cacheKey, trackingIds.CorrelationId, trackingIds.TransactionId);
            if (cachedFlags == null || !cachedFlags.Any())
                return null;

            return cachedFlags.Select(cachedFlag =>
                System.Text.Json.JsonSerializer.Deserialize<FeatureFlagDto>(cachedFlag, _defaultSerializationOptions))
                .ToList();
        }

        private async Task AddCachedFeatureFlags(string appName, string environment, IList<FeatureFlagDto> flags, LoggerTrackingIds trackingIds)
        {
            if (flags == null || !flags.Any())
                return;

            var primaryCache = _cacheFactory.Create(appName, operation: "FeatureFlags", trackingIds.CorrelationId, trackingIds.TransactionId);
            var secondaryCache = _cacheFactory.Create(appName, operation: "FeatureFlagNames", trackingIds.CorrelationId, trackingIds.TransactionId);
            var cacheKey = GetFeatureFlagsCacheKey(appName, environment);
            var cachedValues = flags.Select(flag =>
                System.Text.Json.JsonSerializer.Serialize(flag, _defaultSerializationOptions))
                .ToList();

            if (primaryCache != null)
                await primaryCache.SetList(cacheKey, cachedValues, trackingIds.CorrelationId, trackingIds.TransactionId);

            if (secondaryCache != null)
                await secondaryCache.SetList(cacheKey, flags.Select(flag => flag.Name).ToList(), trackingIds.CorrelationId, trackingIds.TransactionId, 30);
        }

        private async Task DeleteCachedFeatureFlags(string appName, string environment, LoggerTrackingIds trackingIds)
        {
            var primaryCache = _cacheFactory.Create(appName, operation: "FeatureFlags", trackingIds.CorrelationId, trackingIds.TransactionId);
            var secondaryCache = _cacheFactory.Create(appName, operation: "FeatureFlagNames", trackingIds.CorrelationId, trackingIds.TransactionId);
            var cacheKey = GetFeatureFlagsCacheKey(appName, environment);

            if (primaryCache != null)
                await primaryCache.Delete(cacheKey, trackingIds.CorrelationId, trackingIds.TransactionId);

            if (secondaryCache != null)
                await secondaryCache.Delete(cacheKey, trackingIds.CorrelationId, trackingIds.TransactionId);
        }

        private string GetFeatureFlagsCacheKey(string appName, string environment) => $"Flags:{Utility.GetFormattedTenantName(appName)}:{environment.ToUpperInvariant()}";
        #endregion Private::Cache

        private void ValidateFeatureFlag(FeatureFlag featureFlag, string flagName, string appName, string envName)
        {
            if (featureFlag == null)
            {
                var message = string.Format(Constants.Exception.DomainException.FlagDoesntExist.Message, flagName, appName, envName);
                throw new DomainException(message, Constants.Exception.DomainException.FlagDoesntExist.ExceptionCode);
            }
        }
    }

    public class FilterComparer : IEqualityComparer<Filter>
    {
        public bool Equals(Filter x, Filter y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (x is null || y is null)
                return false;

            return x.Parameters.StageId == y.Parameters.StageId && x.Parameters.StageName == y.Parameters.StageName;
        }

        public int GetHashCode(Filter obj)
        {
            if (obj is null) return 0;

            int hashId = obj.Parameters.StageId == null ? 0 : obj.Parameters.StageId.GetHashCode();
            int hashName = obj.Parameters.StageName == null ? 0 : obj.Parameters.StageName.GetHashCode();

            return hashId ^ hashName;
        }
    }
}
