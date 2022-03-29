using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Core.Spec;
using Microsoft.FeatureFlighting.API.Controllers;
using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Api.Controllers
{
    /// <summary>
    /// Configuration Controller
    /// </summary>
    [Route("api/v1/[controller]")]
    public class ConfigurationController : BaseController
    {
        private readonly IOperatorStrategy _operatorEvaluatorStrategy;

        /// <summary>
        /// Constructor
        /// </summary>
        public ConfigurationController(IOperatorStrategy operatorEvaluatorStrategy, IConfiguration configuration, ILogger logger)
            : base(configuration, logger)
        {
            _operatorEvaluatorStrategy = operatorEvaluatorStrategy;
        }

        /// <summary>
        /// Get collection of all the operators used for creating flighting rules
        /// </summary>
        /// <remarks>
        /// 
        /// Sample Request: 
        /// 
        /// GET api/operators
        /// </remarks>
        /// <response code="200">Collection of operators</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json", Type = typeof(IEnumerable<string[]>))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Route("operators")]
        public IActionResult GetOperators()
        {
            string[] operators = Enum.GetNames(typeof(Operator));
            return Ok(operators);
        }

        /// <summary>
        /// Get collection of all filters used for flighting
        /// </summary>
        /// <remarks>
        /// 
        /// Sample Request: 
        /// 
        /// GET api/filters
        /// </remarks>
        /// <response code="200">Collection of filters</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json", Type = typeof(IEnumerable<string[]>))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Route("filters")]
        public async Task<IActionResult> GetFilters()
        {
            var (tenant, _, correlationId, transactionId, _) = GetHeaders(validateHeaders: false);
            IDictionary<string, List<string>> map = await _operatorEvaluatorStrategy.GetFilterOperatorMapping(tenant, correlationId, transactionId);
            return Ok(map.Keys);
        }

        /// <summary>
        /// Gets all the filters with the operators that can operated on the filter
        /// </summary>
        /// <remarks>
        /// 
        /// Sample Request: 
        /// 
        /// GET api/filters/operators/map
        /// </remarks>
        /// <response code="200">Collection of filters and the operators</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(typeof(Dictionary<string, List<string>>))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Route("filters/operators/map")]
        public async Task<IActionResult> GetFilterOperatorMapping()
        {
            var (tenant, _, correlationId, transactionId, _) = GetHeaders(validateHeaders: false);
            IDictionary<string, List<string>> map = await _operatorEvaluatorStrategy.GetFilterOperatorMapping(tenant, correlationId, transactionId);
            return Ok(map);
        }
    }
}
