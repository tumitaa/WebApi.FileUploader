using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Security.Claims;

namespace WebApi.FileUploader.Infrastructure.Authentication
{
    public class AKeyAuthAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public AKeyAuthAttribute(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var aKey = httpContext.Request.Query["AKey"].ToString();
            if (string.IsNullOrEmpty(aKey))
            {
                SetUnauthorizedResult(context, "AKey is missing.");
                return;
            }

            var redisKey = $"authorizationCache:{aKey}";
            var db = _connectionMultiplexer.GetDatabase();
            var cachedData = await db.StringGetAsync(redisKey);

            if (string.IsNullOrEmpty(cachedData))
            {
                SetUnauthorizedResult(context, "Invalid or expired AKey.");
                return;
            }

            var authData = JsonConvert.DeserializeObject<AuthData>(cachedData);
            if (authData == null || string.IsNullOrEmpty(authData.Id))
            {
                SetUnauthorizedResult(context, "Invalid authorization data.");
                return;
            }

            var hash = HashGenerator.GenerateHash(authData.Id);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, authData.Id),
                new Claim("Projeto", authData.Projeto),
                new Claim("Hash", hash)
            };

            var identity = new ClaimsIdentity(claims, "CustomAuth");
            var principal = new ClaimsPrincipal(identity);

            httpContext.User = principal;
        }

        private void SetUnauthorizedResult(AuthorizationFilterContext context, string message)
        {
            context.Result = new JsonResult(new { error = message })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }

        private class AuthData
        {
            public string Id { get; set; }
            public string Projeto { get; set; }
        }
    }
}