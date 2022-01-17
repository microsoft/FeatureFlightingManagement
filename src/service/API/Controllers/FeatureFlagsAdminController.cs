using System;
using System.Linq;
using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Common.Model;
using Microsoft.FeatureFlighting.Core.Queries;
using Microsoft.FeatureFlighting.Core.Commands;
using Microsoft.FeatureFlighting.Common.Model.AzureAppConfig;
using Microsoft.PS.Services.FlightingService.Api.ActionFilters;

namespace Microsoft.FeatureFlighting.API.Controllers
{
    [Route("api/v1/featureflags")]
    public class FeatureFlagsAdminController : BaseController
    {
        private readonly IQueryService _queryService;
        private readonly ICommandBus _commandBus;

        public FeatureFlagsAdminController(IQueryService queryService, ICommandBus commandBus, IConfiguration configuration)
            :base(configuration)
        {
            _queryService = queryService;
            _commandBus = commandBus;
        }

        [HttpGet]
        [Route("{featureName}")]
        [Route("/api/v1/{featureName}")]
        public async Task<IActionResult> GetAzureFeatureFlag([FromRoute] string featureName)
        {
            var (tenant, environment, correlationId, transactionId) = GetHeaders();
            GetAzureFeatureFlagQuery query = new(featureName, tenant, environment, correlationId, transactionId);
            AzureFeatureFlag flag = await _queryService.Query(query);
            if (flag == null)
                return new NotFoundObjectResult($"Feature with name {featureName} doesn't have any flag under {tenant} in {environment}");
            return new OkObjectResult(flag);
        }

        [HttpGet]
        [Route("/api/v1")]
        [Route("")]
        public async Task<IActionResult> GetFeatureFlights()
        {
            var (tenant, environment, correlationId, transactionId) = GetHeaders();
            GetFeatureFlightsQuery query = new(tenant, environment, correlationId, transactionId);
            IEnumerable<FeatureFlightDto> featureFlights = (await _queryService.Query(query)).ToList();
            if (featureFlights == null || !featureFlights.Any())
                return new NotFoundObjectResult($"No feature flights have been registered for {tenant} in {environment}");
            return new OkObjectResult(featureFlights);
        }

        [HttpPut]
        [Route("/api/v1")]
        [Route("")]
        public async Task<IActionResult> CreateFeatureFlag([FromBody] AzureFeatureFlag featureFlag)
        {
            var (tenant, environment, correlationId, transactionId) = GetHeaders();
            featureFlag.Tenant = !string.IsNullOrWhiteSpace(featureFlag.Tenant) ? featureFlag.Tenant : tenant;
            featureFlag.Environment = !string.IsNullOrWhiteSpace(featureFlag.Environment) ? featureFlag.Environment : environment;
            CreateFeatureFlightCommand command = new(featureFlag, correlationId, transactionId);
            await _commandBus.Send(command);
            return new CreatedAtActionResult("GetAzureFeatureFlag", "FeatureFlagsAdmin", new { featureName = featureFlag.Name }, new { });
        }

        [HttpPut]
        [ValidateModel]
        [Route("/api/v1")]
        [Route("")]
        public async Task<IActionResult> UpdateFeatureFlag([FromBody] AzureFeatureFlag featureFlag)
        {
            var (tenant, environment, correlationId, transactionId) = GetHeaders();
            featureFlag.Tenant = !string.IsNullOrWhiteSpace(featureFlag.Tenant) ? featureFlag.Tenant : tenant;
            featureFlag.Environment = !string.IsNullOrWhiteSpace(featureFlag.Environment) ? featureFlag.Environment : environment;
            UpdateFeatureFlightCommand command = new(featureFlag, correlationId, transactionId);
            await _commandBus.Send(command);
            return new NoContentResult();
        }

        [HttpPatch]
        [Route("{featureName}/enable")]
        [Route("/api/v1/{featureName}/enable")]
        public async Task<IActionResult> EnableFeatureFlag([FromRoute] string featureName)
        {
            var (tenant, environment, correlationId, transactionId) = GetHeaders();
            EnableFeatureFlightCommand command = new(featureName, tenant, environment, correlationId, transactionId);
            await _commandBus.Send(command);
            return new NoContentResult();
        }

        [HttpPatch]
        [Route("{featureName}/disable")]
        [Route("/api/v1/{featureName}/disable")]
        public async Task<IActionResult> DisableFeatureFlag([FromRoute] string featureName)
        {
            var (tenant, environment, correlationId, transactionId) = GetHeaders();
            DisableFeatureFlightCommand command = new(featureName, tenant, environment, correlationId, transactionId);
            await _commandBus.Send(command);
            return new NoContentResult();
        }

        [HttpPatch]
        [Route("{featureName}/activatestage/{stageName}")]
        [Route("/api/v1/{featureName}/activatestage/{stageName}")]
        public async Task<IActionResult> ActivateStage([FromRoute] string featureName, [FromRoute] string stageName)
        {
            var (tenant, environment, correlationId, transactionId) = GetHeaders();
            ActivateStageCommand command = new(featureName, tenant, environment, stageName, correlationId, transactionId);
            await _commandBus.Send(command);
            return new NoContentResult();
        }

        [HttpDelete]
        [Route("{featureName}")]
        [Route("/api/v1/{featureName}")]
        public async Task<IActionResult> DeleteFeatureFlag([FromRoute] string featureName)
        {
            var (tenant, environment, correlationId, transactionId) = GetHeaders();
            DeleteFeatureFlightCommand command = new(featureName, tenant, environment, correlationId, transactionId);
            await _commandBus.Send(command);
            return new NoContentResult();
        }
    }
}
