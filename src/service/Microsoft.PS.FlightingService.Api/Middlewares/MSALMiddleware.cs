using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Microsoft.PS.FlightingService.Api.Middlewares
{
    public class MSALMiddleware
    {
        private readonly RequestDelegate _next;

        public MSALMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var result = await httpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                result = await httpContext.AuthenticateAsync("MSAL");
            }

            httpContext.User = result.Principal;
            await _next.Invoke(httpContext);
        }
    }
}
