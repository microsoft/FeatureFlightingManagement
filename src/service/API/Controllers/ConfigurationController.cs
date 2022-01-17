using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.Core.FeatureFilters;
using Microsoft.FeatureFlighting.API.Controllers;

namespace Microsoft.FeatureFlighting.Api.Controllers
{
    [Route("api/v1/[controller]")]
    public class ConfigurationController : BaseController
    {
        private readonly IOperatorStrategy _operatorEvaluatorStrategy;

        public ConfigurationController(IOperatorStrategy operatorEvaluatorStrategy, IConfiguration configuration)
            : base(configuration)
        {
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
            var (tenant, _, correlationId, transactionId) = GetHeaders(validateHeaders: false);
            IDictionary<string, List<string>> map = await _operatorEvaluatorStrategy.GetFilterOperatorMapping(tenant, correlationId, transactionId);
            return Ok(map.Keys);
        }

        [HttpGet]
        [Produces(typeof(Dictionary<string, List<string>>))]
        [Route("filters/operators/map")]
        public async Task<IActionResult> GetFilterOperatorMapping()
        {
            var (tenant, _, correlationId, transactionId) = GetHeaders(validateHeaders: false);
            IDictionary<string, List<string>> map = await _operatorEvaluatorStrategy.GetFilterOperatorMapping(tenant, correlationId, transactionId);
            return Ok(map);
        }
    }
}
