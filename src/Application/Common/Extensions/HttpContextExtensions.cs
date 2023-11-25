using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Application.Common.Extensions
{
    public static class HttpContextExtensions
    {
        public static string? GetUsername(this HttpContext httpContext)
        {
            return httpContext.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Name)?.Value;
        }
    }
}
