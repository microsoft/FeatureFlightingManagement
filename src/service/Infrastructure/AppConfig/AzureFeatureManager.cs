using Azure;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Azure.Data.AppConfiguration;
using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Common;
using Microsoft.Extensions.Configuration;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.FeatureFlighting.Common.AppConfig;
using Microsoft.FeatureFlighting.Common.AppExceptions;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.Infrastructure.AppConfig
{
    public class AzureFeatureManager: IAzureFeatureManager
    {
        private readonly ConfigurationClient _configurationClient;
        private readonly ILogger _logger;
        private readonly string _featureFlagContentType = "application/vnd.microsoft.appconfig.ff+json;charset=utf-8";
        private readonly string _featureFlagPrefix = ".appconfig.featureflag/";
        private readonly string _envLabel;

        public AzureFeatureManager(IConfiguration configuration, IAzureConfigurationClientProvider configurationClientProvider, ILogger logger)
        {
            _envLabel = configuration.GetValue<string>("Env:Label");
            _configurationClient = configurationClientProvider.GetConfigurationClient();
            _logger = logger;
        }

        public async Task Create(AzureFeatureFlag featureFlag, LoggerTrackingIds trackingIds)
        {
            try
            {
                string featureFlagConfigValue = JsonConvert.SerializeObject(featureFlag);
                string featureFlagConfigKey = new StringBuilder().Append(_featureFlagPrefix).Append(featureFlag.Id).ToString();
                ConfigurationSetting config = new(featureFlagConfigKey, featureFlagConfigValue, _envLabel)
                {
                    ContentType = _featureFlagContentType
                };
                await _configurationClient.SetConfigurationSettingAsync(config);
            }
            catch (RequestFailedException rex)
            {
                if (rex.Status == 412)
                    throw new DomainException(message:
                        string.Format(Constants.Exception.DomainException.FlagAlreadyExists.Message, featureFlag.Name, featureFlag.Tenant, featureFlag.Environment),
                        Constants.Exception.DomainException.FlagAlreadyExists.ExceptionCode);

                throw new AzureRequestException(
                   message: rex.Message,
                   statusCode: rex.Status,
                   correlationId: trackingIds.CorrelationId,
                   transactionId: trackingIds.TransactionId,
                   source: "AzureFeatureManager.Create",
                   innerException: rex);
            }
        }

        public async Task Update(AzureFeatureFlag featureFlag, LoggerTrackingIds trackingIds)
        {
            try
            {
                string featureFlagConfigValue = JsonConvert.SerializeObject(featureFlag);
                string featureFlagConfigKey = new StringBuilder().Append(_featureFlagPrefix).Append(featureFlag.Id).ToString();
                ConfigurationSetting config = new(featureFlagConfigKey, featureFlagConfigValue, _envLabel)
                {
                    ContentType = _featureFlagContentType
                };
                await _configurationClient.SetConfigurationSettingAsync(config);
            }
            catch (RequestFailedException rex)
            {
                throw new AzureRequestException(
                   message: rex.Message,
                   statusCode: rex.Status,
                   correlationId: trackingIds.CorrelationId,
                   transactionId: trackingIds.TransactionId,
                   source: "AzureFeatureManager.Update",
                   innerException: rex);
            }
        }

        public async Task ChangeStatus(string name, string tenant, string environment, bool newStatus, LoggerTrackingIds trackingIds)
        {
            AzureFeatureFlag? flag = await Get(name, tenant, environment, trackingIds);
            if (flag == null)
                throw new DomainException($"Flag for feature {name} under {tenant} in {environment} doesn't exist in Azure", "AZ_FLAG_001",
                    trackingIds.CorrelationId, trackingIds.TransactionId, "AzureFeautreManager:ChangeStatus");
            flag.Enabled = newStatus;
            await Update(flag, trackingIds);
        }

        public async Task<AzureFeatureFlag?> Get(string name, string tenant, string environment, LoggerTrackingIds trackingIds)
        {
            try
            {
                string flagId = FlagUtilities.GetFeatureFlagId(tenant, environment, name);
                var configurationResponse = await _configurationClient.GetConfigurationSettingAsync(_featureFlagPrefix + flagId, _envLabel);
                string? configurationValue = configurationResponse?.GetRawResponse()?.Content?.ToString();
                if (string.IsNullOrWhiteSpace(configurationValue))
                    return null;
                ConfigurationSetting rawConfigurationSetting = JsonConvert.DeserializeObject<ConfigurationSetting>(configurationValue);
                if (rawConfigurationSetting == null)
                    return null;

                AzureFeatureFlag featureflag = ConvertFromConfigurationSettings(rawConfigurationSetting, tenant, environment);
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

        public async Task<IEnumerable<AzureFeatureFlag>> Get(string tenant, string environment)
        {
            SettingSelector featureFlagSelector = new()
            {
                KeyFilter = $"{_featureFlagPrefix}{tenant.ToLowerInvariant()}_{environment.ToLowerInvariant()}*",
                LabelFilter = _envLabel
            };
            List<AzureFeatureFlag> flags = new();

            await foreach(ConfigurationSetting configurationSetting in _configurationClient.GetConfigurationSettingsAsync(featureFlagSelector)) 
            {
                flags.Add(ConvertFromConfigurationSettings(configurationSetting, tenant, environment));
            }
            return flags;
        }

        public async Task Delete(string name, string tenant, string environment, LoggerTrackingIds trackingIds)
        {
            try
            {
                string flagId = FlagUtilities.GetFeatureFlagId(tenant, environment, name);
                await _configurationClient.DeleteConfigurationSettingAsync(_featureFlagPrefix + flagId, _envLabel);
            }
            catch (RequestFailedException rex)
            {
                throw new AzureRequestException(
                   message: rex.Message,
                   statusCode: rex.Status,
                   correlationId: trackingIds.CorrelationId,
                   transactionId: trackingIds.TransactionId,
                   source: "FeatureFlagManger.DeleteFeatureFlag",
                   innerException: rex);
            }
        }

        private AzureFeatureFlag ConvertFromConfigurationSettings(ConfigurationSetting configurationSetting, string tenant, string environment)
        {
            AzureFeatureFlag featureflag = JsonConvert.DeserializeObject<AzureFeatureFlag>(configurationSetting.Value);
            featureflag.Name = !string.IsNullOrWhiteSpace(featureflag.Name) ? featureflag.Name : FlagUtilities.GetFeatureFlagName(tenant, environment, featureflag.Id);
            featureflag.Environment = !string.IsNullOrWhiteSpace(featureflag.Environment) ? featureflag.Environment : environment.ToLowerInvariant();
            return featureflag;
        }
    }
}
