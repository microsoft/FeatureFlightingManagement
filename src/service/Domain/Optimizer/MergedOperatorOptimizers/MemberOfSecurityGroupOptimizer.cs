using AppInsights.EnterpriseTelemetry;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.FeatureFlighting.Core.Optimizer
{
    internal class MemberOfSecurityGroupOptimizer : MergedOperatorOptimizer
    {
        public override string RuleName => nameof(MemberOfSecurityGroupOptimizer);

        protected override Operator DuplicateOperator => Operator.MemberOfSecurityGroup;

        protected override Operator OptimizedOperator => Operator.MemberOfSecurityGroup;

        protected override string EventName => "FeatureFlagOptmized:MemberOfSecurityGroupOperatorMerged";

        public MemberOfSecurityGroupOptimizer(ILogger logger) : base(logger) { }

        protected override string JoinDuplicateValues(IGrouping<string, AzureFilterGroup> duplicateFilters)
        {
            List<SecurityGroup> groups = new();
            foreach(AzureFilterGroup azureFilterGroup in duplicateFilters)
            {
                try
                {
                    List<SecurityGroup> filterGroups = JsonConvert.DeserializeObject<List<SecurityGroup>>(azureFilterGroup.Filter.Parameters.Value);
                    groups.AddRange(filterGroups);
                }
                catch (Exception exception) 
                {
                    _logger.Log("Security group format is incorrect, error occured while joining groups");
                    _logger.Log(exception);
                }
            }
            return JsonConvert.SerializeObject(groups);
        }
    }
}
