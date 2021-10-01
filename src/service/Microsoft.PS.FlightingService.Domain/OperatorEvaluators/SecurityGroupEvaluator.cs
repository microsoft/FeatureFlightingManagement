using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.PS.FlightingService.Common;
using Microsoft.PS.FlightingService.Services.Interfaces;
using Microsoft.PS.FlightingService.Domain.FeatureFilters;
using static Microsoft.PS.FlightingService.Common.Constants;

namespace Microsoft.PS.FlightingService.Domain.Evaluators
{
    public class SecurityGroupEvaluator
    {
        private readonly IGraphApiAccessProvider _graphProvider;
        private readonly IConfiguration _configuration;

        public SecurityGroupEvaluator(IGraphApiAccessProvider graphProvider, IConfiguration configuation)
        {
            _graphProvider = graphProvider;
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

                isUserPartOfSecurityGroup = await _graphProvider.IsUserUpnPartOfSecurityGroup(contextValue, securityGroupIds, trackingIds).ConfigureAwait(false);
            }
            else
            {
                isUserPartOfSecurityGroup = await _graphProvider.IsUserAliasPartOfSecurityGroup(contextValue, securityGroupIds, trackingIds).ConfigureAwait(false);
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
            return allowedUpnDomains.Any(allowedDomain =>
                allowedDomain.ToLowerInvariant() == Flighting.ALL.ToLowerInvariant() ||
                allowedDomain.ToLowerInvariant() == upnDomain.ToLowerInvariant());
        }
    }
}
