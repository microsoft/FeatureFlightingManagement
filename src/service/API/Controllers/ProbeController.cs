using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Microsoft.FeatureFlighting.Api.Controllers
{
    /// <summary>
    /// Controller to check the health of the system
    /// </summary>
    [Route("api/probe")]
    public class ProbeController: ControllerBase
    {
        private readonly IConfiguration _config;

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
        public IActionResult Ping([FromQuery]string configKey)
        {
            if (string.IsNullOrWhiteSpace(configKey))
                return new OkObjectResult("Pong");
            
            return Ok(new Dictionary<string, string>()
            {
                { configKey, _config.GetValue<string>(configKey) }
            });
        }
    }
}
