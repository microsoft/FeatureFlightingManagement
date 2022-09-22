using System;
using Newtonsoft.Json;
using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Core.Operators;

namespace Microsoft.FeatureFlighting.Core.Queries
{
    /// <summary>
    /// Handles <see cref="VerifyRulesEngineQuery"/>
    /// </summary>
    public class VerifyRulesEngineQueryHandler : QueryHandler<VerifyRulesEngineQuery, EvaluationResult>
    {
        private readonly IRulesEngineManager _rulesEngineManager;

        public VerifyRulesEngineQueryHandler(IRulesEngineManager rulesEngineManager)
        {
            _rulesEngineManager = rulesEngineManager;
        }

        protected override async Task<EvaluationResult> ProcessRequest(VerifyRulesEngineQuery query)
        {
            try
            {
                IRulesEngineEvaluator evaluator = await _rulesEngineManager.Build(query.Tenant, query.WorkflowName, query.WorkflowPayload);
                Dictionary<string, object> flightContext = JsonConvert.DeserializeObject<Dictionary<string, object>>(query.FlightContext);
                return await evaluator.Evaluate(flightContext, query.TrackingIds);
            }
            catch(Exception exception)
            {
                if (!query.Debug)
                    throw;

                return new EvaluationResult(false, exception.ToString());
            }
        }
    }
}
