using System.Linq;
using CQRS.Mediatr.Lite;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureFlighting.Core.Queries;
using Microsoft.FeatureFlighting.Common.Config;

namespace Microsoft.FeatureFlighting.API.Controllers
{
    /// <summary>
    /// Tenants controller
    /// </summary>
    [Route("api/tenants")]
    public class TenantsController : BaseController
    {
        private readonly IQueryService _queryService;

        /// <summary>
        /// Constructor
        /// </summary>
        public TenantsController(IQueryService queryService, IConfiguration configuration) : base(configuration)
        {
            _queryService = queryService;
        }

        /// <summary>
        /// Gets a list of tenants
        /// </summary>
        /// <remarks>
        /// 
        /// Sample Request: 
        /// 
        /// GET api/tenants
        /// </remarks>
        /// <response code="200">List of registered tenants</response>
        /// <response code="400">Missing information from client</response>
        /// <response code="401">Unauthorized caller</response>
        /// <response code="404">No tenants are found</response>
        /// <response code="500">Unhandled exception</response>
        [Produces(contentType: "application/json", Type =typeof(IEnumerable<TenantConfiguration>))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetTenants()
        {
            GetRegisteredTenantsQuery query = new();
            IEnumerable<TenantConfiguration> tenants = await _queryService.Query(query);
            if (tenants == null || !tenants.Any())
                return new NotFoundObjectResult("No registered tenants found");
            return new OkObjectResult(tenants);
        }
    }
}
