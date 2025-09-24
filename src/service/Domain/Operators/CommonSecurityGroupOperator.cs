using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Group;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using static Microsoft.FeatureFlighting.Common.Constants;

namespace Microsoft.FeatureFlighting.Core.Operators
{
    public class CommonSecurityGroupOperator
    {
        private readonly IGroupVerificationService _groupVerificationService;
        private readonly IConfiguration _configuration;

        public CommonSecurityGroupOperator(IGroupVerificationService graphProvider, IConfiguration configuation)
        {
            _groupVerificationService = graphProvider;
            _configuration = configuation;
        }

        public async Task<EvaluationResult> Evaluate(string configuredValue, string contextValue, string filterType, LoggerTrackingIds trackingIds, Operator op)
        {
            if (string.IsNullOrWhiteSpace(configuredValue))
                return EvaluationResult.CreateFaultedResult(false, "No security groups are configured", op, filterType);

            List<string> securityGroupIds = GetConfiguredSecurityGroups(configuredValue).ToList();
            bool isUserPartOfSecurityGroup;
            if (filterType == FilterKeys.UserUpn || filterType == FilterKeys.RulesEngine)
            {
                if (!IsValidUpn(contextValue))
                    return EvaluationResult.CreateFaultedResult(false, "The UPN is incorrect. Check the format and allowed domains", op, filterType);

                isUserPartOfSecurityGroup = await _groupVerificationService.IsMember(contextValue, securityGroupIds, trackingIds).ConfigureAwait(false);
            }
            else
            {
                //OBOSOLETE: Check by alias needs to be removed
                isUserPartOfSecurityGroup = await _groupVerificationService.IsUserAliasPartOfSecurityGroup(contextValue, securityGroupIds, trackingIds).ConfigureAwait(false);
            }
            return new EvaluationResult(op == Operator.MemberOfSecurityGroup ? isUserPartOfSecurityGroup : !isUserPartOfSecurityGroup, op, filterType);
        }

        private IEnumerable<string> GetConfiguredSecurityGroups(string configuredValue)
        {
            try
            {
                return JsonSerializer.Deserialize<SecurityGroup[]>(configuredValue, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })
                ?.Select(group => group.ObjectId)
                .ToList();
            }
            catch (System.Exception)
            {
                return configuredValue.Split(",");
            }
        }

        private bool IsValidUpn(string contextValue)
        {
            var upnParts = contextValue.Split('@');
            if (upnParts.Length < 2)
                return false;
            var upnDomain = upnParts.Last();

            var allowedUpnDomains = _configuration.GetValue<string>("Authentication:AllowedUpnDomains")?.Split(',');
            return allowedUpnDomains == null || 
                !allowedUpnDomains.Any() || 
                allowedUpnDomains.Any(allowedDomain =>
                allowedDomain.ToLowerInvariant() == Flighting.ALL.ToLowerInvariant() ||
                allowedDomain.ToLowerInvariant() == upnDomain.ToLowerInvariant());
        }
    }
}
