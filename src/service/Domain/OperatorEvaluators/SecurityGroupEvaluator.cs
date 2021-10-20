using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Group;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using static Microsoft.FeatureFlighting.Common.Constants;

namespace Microsoft.FeatureFlighting.Core.Evaluators
{
    public class SecurityGroupEvaluator
    {
        private readonly IGroupVerificationService _groupVerificationService;
        private readonly IConfiguration _configuration;

        public SecurityGroupEvaluator(IGroupVerificationService graphProvider, IConfiguration configuation)
        {
            _groupVerificationService = graphProvider;
            _configuration = configuation;
        }

        public async Task<EvaluationResult> Evaluate(string configuredValue, string contextValue, string filterType, LoggerTrackingIds trackingIds, Operator op)
        {
            if (string.IsNullOrWhiteSpace(configuredValue))
                return new EvaluationResult(false, "No security groups are configured");

            var securityGroupIds =
                JsonSerializer.Deserialize<SecurityGroup[]>(configuredValue, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })
                .Select(group => group.ObjectId)
                .ToList();

            var isUserPartOfSecurityGroup = false;
            if (filterType == FilterKeys.UserUpn)
            {
                if (!IsValidUpn(contextValue))
                    return new EvaluationResult(false, "The UPN is incorrect. Check the format and allowed domains");

                isUserPartOfSecurityGroup = await _groupVerificationService.IsMember(contextValue, securityGroupIds, trackingIds).ConfigureAwait(false);
            }
            else
            {
                //OBOSOLETE: Check by alias needs to be removed
                isUserPartOfSecurityGroup = await _groupVerificationService.IsUserAliasPartOfSecurityGroup(contextValue, securityGroupIds, trackingIds).ConfigureAwait(false);
            }
            return new EvaluationResult(op == Operator.MemberOfSecurityGroup ? isUserPartOfSecurityGroup : !isUserPartOfSecurityGroup);
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
