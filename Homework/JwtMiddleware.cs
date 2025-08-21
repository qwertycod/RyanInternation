using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Homework
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;
        private readonly JwtHelper _jwtHelper;
        public JwtMiddleware(RequestDelegate requestDelegate, IConfiguration config, JwtHelper jwtHelper)
        {
            _next = requestDelegate;
            _config = config;
            _jwtHelper = jwtHelper;
        }

        public async Task Invoke(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null;

            if (!allowAnonymous)
            {
                var token = context.Request.Cookies["jwt-token-value"];

                if (!string.IsNullOrEmpty(token))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var key = Encoding.UTF8.GetBytes(_config["SecretKey"]);

                    try
                    {
                        var principal = _jwtHelper.ValidateRefreshToken(token);
                        if (principal != null)
                        {
                            context.User = principal;   // it makes the context authorised
                        }
                    }
                    catch (Exception ex)
                    {
                        // Invalid token — clear user
                        context.User = null;// new ClaimsPrincipal(new ClaimsIdentity());
                    }
                }
                else
                {
                    // means context.User is null, so [Authorize] will block them.
                }

            }

            // it will let all(unauthenticated request also) if method/controller doesn't have [Authorize] attribute over it.
            await _next(context);
        }
    }
}
