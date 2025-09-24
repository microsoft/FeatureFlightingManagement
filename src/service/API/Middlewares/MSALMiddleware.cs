using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.IdentityModel.S2S.Extensions.AspNetCore;

namespace Microsoft.FeatureFlighting.Api.Middlewares
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
            var result = await httpContext.AuthenticateAsync(S2SAuthenticationDefaults.AuthenticationScheme);
            if (result.Succeeded || httpContext.Request.Path.Value == "/api/probe/ping")
            {
                httpContext.User = result.Principal;
                await _next.Invoke(httpContext);
            }
            else
            {
                throw new System.Exception("Authentication Failed");
            }
        }
    }
}
