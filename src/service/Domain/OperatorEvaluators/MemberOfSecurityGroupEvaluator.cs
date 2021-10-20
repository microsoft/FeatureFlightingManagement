using System.Threading.Tasks;
using Microsoft.FeatureFlighting.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Group;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using static Microsoft.FeatureFlighting.Common.Constants;

namespace Microsoft.FeatureFlighting.Core.Evaluators
{
    public class MemberOfSecurityGroupEvaluator : BaseOperatorEvaluator
    {
        public override Operator Operator => Operator.MemberOfSecurityGroup;
        public override string[] SupportedFilters => new string[] { FilterKeys.Alias, FilterKeys.UserUpn };
        
        private readonly SecurityGroupEvaluator _securityGroupEvaluator;

        public MemberOfSecurityGroupEvaluator(IGroupVerificationService groupVerificationService, IConfiguration configuation)
        {
            _securityGroupEvaluator = new SecurityGroupEvaluator(groupVerificationService, configuation);
        }

        protected override Task<EvaluationResult> Process(string configuredValue, string contextValue, string filterType, LoggerTrackingIds trackingIds)
        {
            return _securityGroupEvaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds, Operator);
        }
    }
}
