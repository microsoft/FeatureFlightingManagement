using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Microsoft.FeatureFlighting.Api.Controllers
{
    /// <summary>
    /// Controller to check the health of the system
    /// </summary>
    [Route("api/probe")]
    public class ProbeController : ControllerBase
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProbeController(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Simple Ping controller
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("ping")]
        public IActionResult Ping([FromQuery] string echo = "Pong")
        {
            return new OkObjectResult(echo);
        }
    }
}
