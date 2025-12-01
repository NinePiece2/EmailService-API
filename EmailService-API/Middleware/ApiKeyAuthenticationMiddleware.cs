using EmailService_API.Services;
using System.Security.Claims;

namespace EmailService_API.Middleware
{
    public class ApiKeyAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ApiKeyHeaderName = "X-API-Key";

        public ApiKeyAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService)
        {
            // Skip authentication for certain endpoints
            if (ShouldSkipAuthentication(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Check if user is already authenticated (JWT authentication ran before this middleware)
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                // JWT authentication already handled this request
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Authentication required. Provide either X-API-Key header or Authorization bearer token.");
                return;
            }

            var isValid = await apiKeyService.ValidateApiKeyAsync(extractedApiKey!);
            if (!isValid)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid API Key");
                return;
            }

            // Create a claims identity for API key authentication
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "ApiKeyUser"),
                new Claim("ApiKey", extractedApiKey!.ToString()!.Substring(0, Math.Min(10, extractedApiKey!.ToString()!.Length)))
            };
            var identity = new ClaimsIdentity(claims, "ApiKey");
            context.User = new ClaimsPrincipal(identity);

            await _next(context);
        }

        private bool ShouldSkipAuthentication(PathString path)
        {
            // Skip authentication for Swagger, token generation, and health check endpoints
            var pathValue = path.Value?.ToLower() ?? "";
            return pathValue.StartsWith("/swagger") ||
                   pathValue.StartsWith("/apikey") ||
                   pathValue.StartsWith("/token") ||
                   pathValue == "/" ||
                   pathValue == "";
        }
    }

    // Extension method to add the middleware to the pipeline
    public static class ApiKeyAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiKeyAuthenticationMiddleware>();
        }
    }
}
