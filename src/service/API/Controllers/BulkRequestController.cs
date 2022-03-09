using System;
using System.Linq;
using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Core.Commands;

namespace Microsoft.FeatureFlighting.API.Controllers
{
    /// <summary>
    /// Bulk request controller
    /// </summary>
    [Route("api/featureflags/bulk")]
    public class BulkRequestController : BaseController
    {
        private readonly ICommandBus _commandBus;

        /// <summary>
        /// Constructor
        /// </summary>
        public BulkRequestController(IConfiguration configuration, ICommandBus commandBus) : base(configuration)
        {
            _commandBus = commandBus;
        }

        /// <summary>
        /// Deletes feature flights in bulk
        /// </summary>
        /// <param name="featureFlightSelection">Selection of feature flights to be deleted</param>
        /// <remarks>
        /// 
        /// Sample request:
        /// 
        /// DELETE api/featureflags/bulk
        /// </remarks>
        /// <response code="200">Bulk operation meessage</response>
        /// <response code="400">Missing/inconsistent information from client</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json", Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete]
        public async Task<IActionResult> BulkDelete([FromBody] Dictionary<string, string> featureFlightSelection)
        {
            IEnumerable<string> selectedFeatureNames = GetSelectedFeatureNames(featureFlightSelection);
            if (selectedFeatureNames == null || !selectedFeatureNames.Any())
                return new BadRequestObjectResult("No flights selected for deletion");

            await PerformBulkOperation(selectedFeatureNames, "DELETE");
            return new OkObjectResult($"The flights for the following features has been deleted: {string.Join(',', selectedFeatureNames)}");
        }

        /// <summary>
        /// Disables feature flights in bulk
        /// </summary>
        /// <param name="featureFlightSelection">Selection of feature flights to be disabled</param>
        /// <remarks>
        /// 
        /// Sample request:
        /// 
        /// PUT api/featureflags/bulk/disable
        /// </remarks>
        /// <response code="200">Bulk operation meessage</response>
        /// <response code="400">Missing/inconsistent information from client</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json", Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("disable")]
        [HttpPut]
        public async Task<IActionResult> BulkDisable([FromBody] Dictionary<string, string> featureFlightSelection)
        {
            IEnumerable<string> selectedFeatureNames = GetSelectedFeatureNames(featureFlightSelection);
            if (selectedFeatureNames == null || !selectedFeatureNames.Any())
                return new BadRequestObjectResult("No flights selected for disablement");

            await PerformBulkOperation(selectedFeatureNames, "DISABLE");
            return new OkObjectResult($"The flights for the following features has been disabled: {string.Join(',', selectedFeatureNames)}");
        }

        /// <summary>
        /// Unsubcribe alerts feature flights in bulk
        /// </summary>
        /// <param name="featureFlightSelection">Selection of feature flights to be unsubscribed</param>
        /// <remarks>
        /// 
        /// Sample request:
        /// 
        /// PUT api/featureflags/bulk/unsubscribe
        /// </remarks>
        /// <response code="200">Bulk operation meessage</response>
        /// <response code="400">Missing/inconsistent information from client</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json", Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("unsubscribe")]
        [HttpPut]
        public async Task<IActionResult> BulkUnsubscribe([FromBody] Dictionary<string, string> featureFlightSelection)
        {
            IEnumerable<string> selectedFeatureNames = GetSelectedFeatureNames(featureFlightSelection);
            if (selectedFeatureNames == null || !selectedFeatureNames.Any())
                return new BadRequestObjectResult("No flights selected for disablement");

            await PerformBulkOperation(selectedFeatureNames, "UNSUBSCRIVE");
            return new OkObjectResult($"You won't receive alers for the following flights: {string.Join(',', selectedFeatureNames)}");
        }

        private async Task PerformBulkOperation(IEnumerable<string> selectedFeatureNames, string operationType)
        {   
            var featureGroups = selectedFeatureNames.Select((feature, index) => new
            {
                Index = index,
                Feature = feature
            }).GroupBy((indexed) => indexed.Index / 10);

            foreach (var featureGroup in featureGroups)
            {
                List<Task> bulkTasks = new();
                foreach (var featureIndex in featureGroup.ToList())
                {
                    Command<IdCommandResult> command = CreateCommand(operationType, featureIndex.Feature);
                    bulkTasks.Add(_commandBus.Send(command));
                }
                await Task.WhenAll(bulkTasks);
            }
        }

        private Command<IdCommandResult> CreateCommand(string commandType, string featureName)
        {
            var (tenant, environment, correlationId, transactionId, channel) = GetHeaders();
            return commandType switch
            {
                "DELETE" => new DeleteFeatureFlightCommand(featureName, tenant, environment, correlationId, transactionId, channel),
                "DISABLE" => new DisableFeatureFlightCommand(featureName, tenant, environment, correlationId, transactionId, channel),
                _ => new UnsubscribeAlertsCommand(featureName, tenant, environment, correlationId, transactionId, channel),
            };
        }

        private static IEnumerable<string> GetSelectedFeatureNames(Dictionary<string, string> featureFlightSelection)
        {
            if (featureFlightSelection == null || !featureFlightSelection.Any())
                return null;

            IEnumerable<string> selectedFeatureNames = featureFlightSelection
                .Where(selection => bool.Parse(selection.Value))
                .Select(map => map.Key)
                .ToList();

            return selectedFeatureNames;
        }
    }
}
