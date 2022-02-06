using System;
using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        public ReportsController(ICommandBus commandBus, IConfiguration configuration) : base(configuration)
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
        [HttpPost]
        public async Task<IActionResult> GenerateReport([FromQuery] bool triggerAlert = true)
        {
            var (tenant, environment, correlationId, transactionId) = GetHeaders();
            GenerateReportCommand command = new(tenant, environment, triggerAlert, correlationId, transactionId);
            ReportCommandResult result = await _commandBus.Send(command);
            return new OkObjectResult(result.Report);
        }
    }
}
