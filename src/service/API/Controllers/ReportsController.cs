using System;
using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using AppInsights.EnterpriseTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Core.Commands;

namespace Microsoft.FeatureFlighting.API.Controllers
{
    /// <summary>
    /// Feature Flight usage report controller
    /// </summary>
    [Route("api/reports")]
    public class ReportsController : BaseController
    {
        private readonly ICommandBus _commandBus;

        /// <summary>
        /// Constructor
        /// </summary>
        public ReportsController(ICommandBus commandBus, IConfiguration configuration, ILogger logger) : base(configuration, logger)
        {
            _commandBus = commandBus;
        }

        /// <summary>
        /// Generates usage report for a tenant
        /// </summary>
        /// <remarks>
        /// 
        /// Sample request:
        /// 
        /// POST api/reports?triggerAlert=true
        /// </remarks>
        /// <response code="200">Report generated</response>
        /// <response code="400">Missing/inconsistent information from client</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json", Type = typeof(UsageReportDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPatch]
        [HttpPost]
        public async Task<IActionResult> GenerateReport([FromQuery] bool triggerAlert = true)
        {
            var (tenant, environment, correlationId, transactionId, _) = GetHeaders();
            GenerateReportCommand command = new(tenant, environment, triggerAlert, correlationId, transactionId);
            ReportCommandResult result = await _commandBus.Send(command);
            return new OkObjectResult(result.Report);
        }
    }
}
