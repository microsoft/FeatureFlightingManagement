using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.PS.FlightingService.Domain.Interfaces;
using Microsoft.PS.FlightingService.Domain.FeatureFilters;

namespace Microsoft.PS.FlightingService.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [AspNetCore.Authorization.Authorize]
    public class ConfigurationController : ControllerBase
    {
        private readonly IOperatorEvaluatorStrategy _operatorEvaluatorStrategy;

        public ConfigurationController(IConfiguration config, IOperatorEvaluatorStrategy operatorEvaluatorStrategy)
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
        public IActionResult GetFilters()
        {
            string[] filters = Enum.GetNames(typeof(Filters));
            return Ok(filters);
        }

        [HttpGet]
        [Produces(typeof(Dictionary<string, List<string>>))]
        [Route("filters/operators/map")]
        public IActionResult GetFilterOperatorMapping()
        {
            return Ok(_operatorEvaluatorStrategy.GetFilterOperatorMapping());
        }
    }
}
