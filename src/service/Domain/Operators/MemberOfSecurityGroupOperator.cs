using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Group;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using static Microsoft.FeatureFlighting.Common.Constants;

namespace Microsoft.FeatureFlighting.Core.Operators
{
    public class MemberOfSecurityGroupOperator : BaseOperator
    {
        public override Operator Operator => Operator.MemberOfSecurityGroup;
        public override string[] SupportedFilters => new string[] { FilterKeys.Alias, FilterKeys.UserUpn, FilterKeys.RulesEngine };
        
        private readonly CommonSecurityGroupOperator _securityGroupOperator;

        public MemberOfSecurityGroupOperator(IGroupVerificationService groupVerificationService, IConfiguration configuation)
        {
            _securityGroupOperator = new CommonSecurityGroupOperator(groupVerificationService, configuation);
        }

        protected override Task<EvaluationResult> Process(string configuredValue, string contextValue, string filterType, LoggerTrackingIds trackingIds)
        {
            return _securityGroupOperator.Evaluate(configuredValue, contextValue, filterType, trackingIds, Operator);
        }
    }
}
