using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common;
using Microsoft.FeatureFlighting.Services.Interfaces;
using Microsoft.FeatureFlighting.Domain.FeatureFilters;
using static Microsoft.FeatureFlighting.Common.Constants;

namespace Microsoft.FeatureFlighting.Domain.Evaluators
{
    public class NotMemberOfSecurityGroupEvaluator : BaseOperatorEvaluator
    {
        public override Operator Operator => Operator.NotMemberOfSecurityGroup;
        public override string[] SupportedFilters => new string[] { FilterKeys.Alias, FilterKeys.UserUpn };
        private readonly SecurityGroupEvaluator _securityGroupEvaluator;

        public NotMemberOfSecurityGroupEvaluator(IGraphApiAccessProvider graphProvider, IConfiguration configuation)
        {
            _securityGroupEvaluator = new SecurityGroupEvaluator(graphProvider, configuation);
        }

        protected override Task<EvaluationResult> Process(string configuredValue, string contextValue, string filterType, LoggerTrackingIds trackingIds)
        {
            return _securityGroupEvaluator.Evaluate(configuredValue, contextValue, filterType, trackingIds, Operator);
        }
    }
}
