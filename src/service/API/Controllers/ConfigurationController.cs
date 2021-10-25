using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.FeatureFlighting.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [AspNetCore.Authorization.Authorize]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IOperatorStrategy _operatorEvaluatorStrategy;

        public ConfigurationController(IConfiguration configuration, IOperatorStrategy operatorEvaluatorStrategy)
        {
            _configuration = configuration;
            _operatorEvaluatorStrategy = operatorEvaluatorStrategy;
        }

        [HttpGet]
        [Produces(typeof(string[]))]
        [Route("operators")]
        public IActionResult GetOperators()
        {
            string[] operators = Enum.GetNames(typeof(Operator));
            return Ok(operators);
        }

        [HttpGet]
        [Produces(typeof(string[]))]
        [Route("filters")]
        public async Task<IActionResult> GetFilters()
        {
            string tenant = Request?.Headers.GetOrDefault(Constants.Flighting.APP_HEADER, "Default").ToString() ?? "Default";
            string correlationId = Request?.Headers.GetOrDefault("x-CorrelationId", Guid.NewGuid().ToString()).ToString();
            string transactionId = Request?.Headers.GetOrDefault("x-MessageId", Guid.NewGuid().ToString()).ToString();
            IDictionary<string, List<string>> map = await _operatorEvaluatorStrategy.GetFilterOperatorMapping(tenant, correlationId, transactionId);
            return Ok(map.Keys);
        }

        [HttpGet]
        [Produces(typeof(Dictionary<string, List<string>>))]
        [Route("filters/operators/map")]
        public async Task<IActionResult> GetFilterOperatorMapping()
        {
            string tenant = Request?.Headers.GetOrDefault(Constants.Flighting.APP_HEADER, "Default").ToString() ?? "Default";
            string correlationId = Request?.Headers.GetOrDefault("x-CorrelationId", Guid.NewGuid().ToString()).ToString();
            string transactionId = Request?.Headers.GetOrDefault("x-MessageId", Guid.NewGuid().ToString()).ToString();
            IDictionary<string, List<string>> map = await _operatorEvaluatorStrategy.GetFilterOperatorMapping(tenant, correlationId, transactionId);
            return Ok(map);
        }
    }
}
