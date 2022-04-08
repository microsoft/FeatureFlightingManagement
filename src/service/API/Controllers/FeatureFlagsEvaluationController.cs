using System;
using System.Linq;
using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Core.Queries;

namespace Microsoft.FeatureFlighting.API.Controllers
{
    /// <summary>
    /// Feature flight evaluation controller
    /// </summary>
    [Route("api/v1/featureflags")]
    public class FeatureFlagsEvaluationController : BaseController
    {   
        private readonly IFeatureFlagEvaluator _featureFlagEvaluator;
        private readonly IQueryService _queryService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Constructor
        /// </summary>
        public FeatureFlagsEvaluationController(IFeatureFlagEvaluator featureFlagEvaluator, 
            IQueryService queryService, 
            IHttpContextAccessor httpContextAccesor, 
            IConfiguration configuration,
            ILogger logger)
        :base(configuration, logger)
        {   
            _featureFlagEvaluator = featureFlagEvaluator;
            _queryService = queryService;
            _httpContextAccessor = httpContextAccesor;
        }

        /// <summary>
        /// Evaluates feature flights (Legacy method)
        /// </summary>
        /// <param name="appName">Tenant name</param>
        /// <param name="envName">Environment</param>
        /// <param name="featureNames">Comma separated collection of feature names</param>
        /// <remarks>
        /// Deprecation Warning: This is a legacy method, consider moving away from this method
        /// Sample Requests:
        /// 
        /// GET api/v1/featureflags/Tenant/Prod/flighting
        /// </remarks>
        /// <response code="200">Evaluation result</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json", Type = typeof(Dictionary<string, bool>))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Route("{appName}/{envName}/flighting")]
        [Route("/api/v1/{appName}/{envName}/flighting")]
        public async Task<IActionResult> EvaluateFeatureFlag_Backward([FromRoute] string appName, [FromRoute] string envName, [FromQuery] string featureNames)
        {
            _httpContextAccessor.HttpContext.Request.Headers.AddOrUpdate("x-application", appName);
            _httpContextAccessor.HttpContext.Request.Headers.AddOrUpdate("x-environment", envName);
            return await EvaluateFeatureFlag(featureNames);
        }

        /// <summary>
        /// Evaluates feature flags
        /// </summary>
        /// <param name="featureNames">Comma separated collection of feature names. Empty feature names will result in the evaluation of all feature flags in the tenant.</param>
        /// <remarks>
        /// Sample Requests:
        /// 
        /// GET api/v1/featureflags/evaluate?featureNames=Flag1,Flag2,Flag3
        /// </remarks>
        /// <response code="200">Evaluation result</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json", Type = typeof(Dictionary<string, bool>))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Route("Evaluate")]
        [Route("/api/v1/Evaluate")]
        public async Task<IActionResult> EvaluateFeatureFlag([FromQuery] string featureNames)
        {
            var (tenant, environment, correlationId, transactionId, _) = GetHeaders();
            IList<string> featureList;
            if (string.IsNullOrWhiteSpace(featureNames))
            {
                GetFeatureNamesQuery query = new(tenant, environment, correlationId, transactionId);
                featureList = (await _queryService.Query(query)).ToList();
                if (featureList == null || !featureList.Any())
                {
                    return Ok(new Dictionary<string, bool>());
                }
            }
            else
            {
                featureList = featureNames.Split(',').ToList();
            }

            IDictionary<string, bool> evaluationResult = await _featureFlagEvaluator.Evaluate(tenant, environment, featureList.ToList());
            return Ok(evaluationResult);
        }
    }
}