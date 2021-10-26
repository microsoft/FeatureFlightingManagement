using System;
using AppInsights.EnterpriseTelemetry.Exceptions;

namespace Microsoft.FeatureFlighting.Common.AppExceptions
{
    /// <summary>
    /// Exception when rules engine fail
    /// </summary>
    [Serializable]
    public class RuleEngineException : BaseAppException
    {
        public override string Type => Constants.Exception.Types.BRE;

        public string RuleEngine { get; set; }
        public string Tenant { get; set; }

        public RuleEngineException(string ruleEngine, string tenant, string message, string source, string correlationId, string transactionId)
            : base(message: message,
                  exceptionCode: Constants.Exception.RulesEngineException.GeneralCode,
                  source: source,
                  correlationId: correlationId,
                  transactionId: transactionId)
        {
            RuleEngine = ruleEngine;
            Tenant = tenant;
        }

        public RuleEngineException(string ruleEngine, string tenant, Exception exception, string source, string correlationId, string transactionId)
            : base(message: string.Format(Constants.Exception.RulesEngineException.Message, ruleEngine, tenant),
                  exceptionCode: Constants.Exception.RulesEngineException.EvaluationFailureCode,
                  innerException: exception,
                  source: source,
                  correlationId: correlationId,
                  transactionId: transactionId)
        {
            RuleEngine = ruleEngine;
            Tenant = tenant;
        }

        protected override string CreateDisplayMessage()
            => string.Format(Constants.Exception.RulesEngineException.DisplayMessage, CorrelationId);
    }
}
