using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Group;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using static Microsoft.FeatureFlighting.Common.Constants;

namespace Microsoft.FeatureFlighting.Core.Operators
{
    /// <summary>
    /// Context user is not part of any of the configured security groups
    /// </summary>
    public class NotMemberOfSecurityGroupOperator : BaseOperator
    {
        public override Operator Operator => Operator.NotMemberOfSecurityGroup;
        public override string[] SupportedFilters => new string[] { FilterKeys.Alias, FilterKeys.UserUpn };
        
        private readonly CommonSecurityGroupOperator _securityGroupEvaluator;

        public NotMemberOfSecurityGroupOperator(IGroupVerificationService groupVerificationService, IConfiguration configuation)
        {
            _securityGroupEvaluator = new CommonSecurityGroupOperator(groupVerificationService, configuation);
        }

        protected override Task<EvaluationResult> Process(string configuredValue, string contextValue, string filterType, LoggerTrackingIds trackingIds)
        {
            return _securityGroupEvaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds, Operator);
        }
    }
}
