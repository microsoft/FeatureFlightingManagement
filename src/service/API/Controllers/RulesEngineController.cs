using System;
using Newtonsoft.Json;
using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Core.Queries;
using Microsoft.FeatureFlighting.Core.Operators;

namespace Microsoft.FeatureFlighting.API.Controllers
{
    /// <summary>
    /// Rules Engine controller
    /// </summary>
    [Route("api/v1/rulesengines")]
    public class RulesEngineController : BaseController
    {
        private readonly IQueryService _queryService;

        /// <summary>
        /// Constructor
        /// </summary>
        public RulesEngineController(IQueryService queryService, IConfiguration configuration, ILogger logger)
            :base(configuration, logger)
        {
            _queryService = queryService;
        }

        /// <summary>
        /// Verifies the result of a rules engine
        /// </summary>
        /// <param name="workflowName">Name of the workflow</param>
        /// <param name="workflow">JSON body of the workflow</param>
        /// <param name="debug">Indicates if the debug mode is enabled. In debug mode exact error message will be returned</param>
        /// <remarks>
        /// Sample Requests:
        /// 
        /// GET api/v1/featureflags/evaluate?featureNames=Flag1,Flag2,Flag3
        /// </remarks>
        /// <response code="200">Evaluation result of the rule engine</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json", Type = typeof(EvaluationResult))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        [Route("{workflowName}/evaluate")]
        public async Task<IActionResult> Evaluate([FromRoute] string workflowName, [FromBody] RulesEngine.Models.Workflow workflow, [FromQuery] bool debug = true)
        {
            var (tenant, _, correlationId, transactionId, _) = GetHeaders();
            string flightContext = GetHeaderValue("x-flightcontext", string.Empty);
            VerifyRulesEngineQuery verifyRuleEngine = new(tenant, workflowName, JsonConvert.SerializeObject(workflow), flightContext, debug, correlationId, transactionId);
            EvaluationResult evaluationResult = await _queryService.Query(verifyRuleEngine);
            return new OkObjectResult(evaluationResult);
        }

    }
}
