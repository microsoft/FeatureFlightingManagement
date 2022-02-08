using System;
using System.Linq;
using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Core.Queries;
using Microsoft.FeatureFlighting.Core.Commands;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;

namespace Microsoft.FeatureFlighting.API.Controllers
{
    /// <summary>
    /// Feature flight admin controller
    /// </summary>
    [Route("api/v1/featureflags")]
    public class FeatureFlagsAdminController : BaseController
    {
        private readonly IQueryService _queryService;
        private readonly ICommandBus _commandBus;

        /// <summary>
        /// Constructor
        /// </summary>
        public FeatureFlagsAdminController(IQueryService queryService, ICommandBus commandBus, IConfiguration configuration)
            :base(configuration)
        {
            _queryService = queryService;
            _commandBus = commandBus;
        }

        /// <summary>
        /// Gets feature flight configured in Azure
        /// </summary>
        /// <param name="featureName">Feature name to be fetched</param>
        /// <remarks>
        /// Sample Request:
        /// 
        /// GET api/v1/featureflags/feature_1
        /// </remarks>
        /// <response code="200">Azure Feature Flag</response>
        /// <response code="400">Missing/inconsistent information from client</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="404">No flag found for the feature</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json", Type = typeof(AzureFeatureFlag))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Route("{featureName}")]
        [Route("/api/v1/{featureName}")]
        public async Task<IActionResult> GetAzureFeatureFlag([FromRoute] string featureName)
        {
            var (tenant, environment, correlationId, transactionId, _) = GetHeaders();
            GetAzureFeatureFlagQuery query = new(featureName, tenant, environment, correlationId, transactionId);
            AzureFeatureFlag flag = await _queryService.Query(query);
            if (flag == null)
                return new NotFoundObjectResult($"Feature with name {featureName} doesn't have any flag under {tenant} in {environment}");
            return new OkObjectResult(flag);
        }

        /// <summary>
        /// Gets all feature flights configured for a tenant
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        /// GET api/v1/featureflags
        /// </remarks>
        /// <response code="200">Collection of Feature Flags</response>
        /// <response code="400">Missing/inconsistent information from client</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="404">No flag found for the tenant</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json", Type = typeof(IEnumerable<FeatureFlightDto>))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Route("/api/v1")]
        [Route("")]
        public async Task<IActionResult> GetFeatureFlights()
        {
            var (tenant, environment, correlationId, transactionId, _) = GetHeaders();
            GetFeatureFlightsQuery query = new(tenant, environment, correlationId, transactionId);
            IEnumerable<FeatureFlightDto> featureFlights = (await _queryService.Query(query))?.ToList();
            if (featureFlights == null || !featureFlights.Any())
                return new NotFoundObjectResult($"No feature flights have been registered for {tenant} in {environment}");
            return new OkObjectResult(featureFlights);
        }

        /// <summary>
        /// Creates a feature flight
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        /// POST api/v1/featureflags
        /// </remarks>
        /// <response code="201">Succesfull creation</response>
        /// <response code="400">Missing/inconsistent information from client</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="404">No flag found for the tenant</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        [Route("/api/v1")]
        [Route("")]
        public async Task<IActionResult> CreateFeatureFlag([FromBody] AzureFeatureFlag featureFlag)
        {
            var (tenant, environment, correlationId, transactionId, channel) = GetHeaders();
            featureFlag.Tenant = !string.IsNullOrWhiteSpace(featureFlag.Tenant) ? featureFlag.Tenant : tenant;
            featureFlag.Environment = !string.IsNullOrWhiteSpace(featureFlag.Environment) ? featureFlag.Environment : environment;
            CreateFeatureFlightCommand command = new(featureFlag, correlationId, transactionId, channel);
            await _commandBus.Send(command);
            return new CreatedAtActionResult("GetAzureFeatureFlag", "FeatureFlagsAdmin", new { featureName = featureFlag.Name }, new { });
        }

        /// <summary>
        /// Updates an existing feature flight
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        /// PUT api/v1/featureflags
        /// </remarks>
        /// <response code="204">Succesfull updation</response>
        /// <response code="400">Missing/inconsistent information from client</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="404">No flag found for the tenant</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        [Route("/api/v1")]
        [Route("")]
        public async Task<IActionResult> UpdateFeatureFlag([FromBody] AzureFeatureFlag featureFlag)
        {
            var (tenant, environment, correlationId, transactionId, channel) = GetHeaders();
            featureFlag.Tenant = !string.IsNullOrWhiteSpace(featureFlag.Tenant) ? featureFlag.Tenant : tenant;
            featureFlag.Environment = !string.IsNullOrWhiteSpace(featureFlag.Environment) ? featureFlag.Environment : environment;
            UpdateFeatureFlightCommand command = new(featureFlag, correlationId, transactionId, channel);
            await _commandBus.Send(command);
            return new NoContentResult();
        }

        /// <summary>
        /// Enables a disabled feature flight
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        /// PATCH api/v1/featureflags/feature_1/enable
        /// </remarks>
        /// <response code="204">Succesfull updation</response>
        /// <response code="400">Missing/inconsistent information from client</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="404">No flag found for the tenant</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPatch]
        [Route("{featureName}/enable")]
        [Route("/api/v1/{featureName}/enable")]
        public async Task<IActionResult> EnableFeatureFlag([FromRoute] string featureName)
        {
            var (tenant, environment, correlationId, transactionId, channel) = GetHeaders();
            EnableFeatureFlightCommand command = new(featureName, tenant, environment, correlationId, transactionId, channel);
            await _commandBus.Send(command);
            return new NoContentResult();
        }

        /// <summary>
        /// Disabled an enabled feature flight
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        /// PATCH api/v1/featureflags/feature_1/disable
        /// </remarks>
        /// <response code="204">Succesfull updation</response>
        /// <response code="400">Missing/inconsistent information from client</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="404">No flag found for the tenant</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPatch]
        [Route("{featureName}/disable")]
        [Route("/api/v1/{featureName}/disable")]
        public async Task<IActionResult> DisableFeatureFlag([FromRoute] string featureName)
        {
            var (tenant, environment, correlationId, transactionId, channel) = GetHeaders();
            DisableFeatureFlightCommand command = new(featureName, tenant, environment, correlationId, transactionId, channel);
            await _commandBus.Send(command);
            return new NoContentResult();
        }

        /// <summary>
        /// Activates a stage in the feature flight
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        /// PATCH api/v1/featureflags/feature_1/activestage/ring_2
        /// </remarks>
        /// <response code="204">Succesfull updation</response>
        /// <response code="400">Missing/inconsistent information from client</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="404">No flag found for the tenant</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPatch]
        [Route("{featureName}/activatestage/{stageName}")]
        [Route("/api/v1/{featureName}/activatestage/{stageName}")]
        public async Task<IActionResult> ActivateStage([FromRoute] string featureName, [FromRoute] string stageName)
        {
            var (tenant, environment, correlationId, transactionId, channel) = GetHeaders();
            ActivateStageCommand command = new(featureName, tenant, environment, stageName, correlationId, transactionId, channel);
            await _commandBus.Send(command);
            return new NoContentResult();
        }

        /// <summary>
        /// Deletes a feature flight
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        /// DELETE api/v1/featureflags/feature_1
        /// </remarks>
        /// <response code="204">Succesfull deletion</response>
        /// <response code="400">Missing/inconsistent information from client</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="404">No flag found for the tenant</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete]
        [Route("{featureName}")]
        [Route("/api/v1/{featureName}")]
        public async Task<IActionResult> DeleteFeatureFlag([FromRoute] string featureName)
        {
            var (tenant, environment, correlationId, transactionId, channel) = GetHeaders();
            DeleteFeatureFlightCommand command = new(featureName, tenant, environment, correlationId, transactionId, channel);
            await _commandBus.Send(command);
            return new NoContentResult();
        }

        /// <summary>
        /// Applies all optimization rules and re-builds the feature flag
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        /// PUT api/v1/featureflags/rebuild?featurename=feature_1&amp;feature_name=feature_2
        /// </remarks>
        /// <response code="200">Succesfull rebuild</response>
        /// <response code="400">Missing/inconsistent information from client</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="404">No flag found for the tenant</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        [Route("rebuild")]
        public async Task<IActionResult> RebuildFlags([FromQuery] string reason, [FromQuery] string[] featureName = null)
        {
            var (tenant, environment, correlationId, transactionId, channel) = GetHeaders();
            RebuildFlightsCommand command = new(featureName.ToList(), tenant, environment, reason, correlationId, transactionId, channel);
            RebuildCommandResult result = await _commandBus.Send(command);
            return new OkObjectResult(result);
        }

        /// <summary>
        /// Subscribes to email alerts for a feature flight
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        /// PATCH api/v1/featureflags/feature_1/subscribe
        /// </remarks>
        /// <response code="204">Succesfull subscription</response>
        /// <response code="400">Missing/inconsistent information from client</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="404">No flag found for the tenant</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPatch]
        [Route("{featureName}/subscribe")]
        public async Task<IActionResult> SubscribeToAlerts([FromRoute] string featureName)
        {
            var (tenant, environment, correlationId, transactionId, channel) = GetHeaders();
            SubscribeAlertsCommand command = new(featureName, tenant, environment, correlationId, transactionId, channel);
            await _commandBus.Send(command);
            return new NoContentResult();
        }

        /// <summary>
        /// Unsubscribes from email alerts for a feature flight
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// 
        /// PATCH api/v1/featureflags/feature_1/unsubscribe
        /// </remarks>
        /// <response code="204">Succesfull subscription</response>
        /// <response code="400">Missing/inconsistent information from client</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="404">No flag found for the tenant</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPatch]
        [Route("{featureName}/unsubscribe")]
        public async Task<IActionResult> UnsubscribeFromAlerts([FromRoute] string featureName)
        {
            var (tenant, environment, correlationId, transactionId, channel) = GetHeaders();
            UnsubscribeAlertsCommand command = new(featureName, tenant, environment, correlationId, transactionId, channel);
            await _commandBus.Send(command);
            return new NoContentResult();
        }
    }
}
