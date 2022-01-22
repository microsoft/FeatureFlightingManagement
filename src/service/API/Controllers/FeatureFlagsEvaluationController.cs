using System;
using System.Linq;
using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Core.Queries;

namespace Microsoft.FeatureFlighting.API.Controllers
{
    [Route("api/v1/featureflags")]
    public class FeatureFlagsEvaluationController : BaseController
    {   
        private readonly IFeatureFlagEvaluator _featureFlagEvaluator;
        private readonly IQueryService _queryService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FeatureFlagsEvaluationController(IFeatureFlagEvaluator featureFlagEvaluator, 
            IQueryService queryService, 
            IHttpContextAccessor httpContextAccesor, 
            IConfiguration configuration)
        :base(configuration)
        {   
            _featureFlagEvaluator = featureFlagEvaluator;
            _queryService = queryService;
            _httpContextAccessor = httpContextAccesor;
        }

        [HttpGet]
        [Produces(typeof(Dictionary<string, bool>))]
        [Route("{appName}/{envName}/flighting")]
        [Route("/api/v1/{appName}/{envName}/flighting")]
        public async Task<IActionResult> EvaluateFeatureFlag_Backward([FromRoute] string appName, [FromRoute] string envName, [FromQuery] string featureNames)
        {
            _httpContextAccessor.HttpContext.Request.Headers.AddOrUpdate("x-application", appName);
            _httpContextAccessor.HttpContext.Request.Headers.AddOrUpdate("x-environment", envName);
            return await EvaluateFeatureFlag(featureNames);
        }

        [HttpGet]
        [Produces(typeof(Dictionary<string, bool>))]
        [Route("Evaluate")]
        [Route("/api/v1/Evaluate")]
        public async Task<IActionResult> EvaluateFeatureFlag([FromQuery] string featureNames)
        {
            var (tenant, environment, correlationId, transactionId) = GetHeaders();
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