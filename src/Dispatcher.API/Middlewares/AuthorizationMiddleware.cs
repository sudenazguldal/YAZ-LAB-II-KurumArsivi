using Microsoft.AspNetCore.Http;

namespace Dispatcher.API.Middlewares
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var method = context.Request.Method.ToUpperInvariant();

            // Login herkese açık
            if (path.StartsWith("/api/auth/login", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // AuthMiddleware burada role'ü doldurmuş olacak
            var userRole = context.Items["UserRole"]?.ToString() ?? string.Empty;

            if (!IsAuthorized(path, method, userRole))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Forbidden.");
                return;
            }

            await _next(context);
        }

        private static bool IsAuthorized(string path, string method, string role)
        {
            // Login servisindeki admin endpointleri
            if (path.Equals("/api/auth/register", StringComparison.OrdinalIgnoreCase) && method == "POST")
                return role == "admin";

            if (path.Equals("/api/auth/users", StringComparison.OrdinalIgnoreCase) && method == "GET")
                return role == "admin";

            if (path.StartsWith("/api/auth/users/", StringComparison.OrdinalIgnoreCase) && method == "DELETE")
                return role == "admin";

            if (path.StartsWith("/api/auth/users/", StringComparison.OrdinalIgnoreCase)
                && path.EndsWith("/role", StringComparison.OrdinalIgnoreCase)
                && method == "PUT")
                return role == "admin";

            // Document endpointleri
            if (path.StartsWith("/api/documents", StringComparison.OrdinalIgnoreCase))
            {
                if (method == "GET") return role == "admin" || role == "user";
                if (method == "POST") return role == "admin" || role == "user";
                if (method == "PUT") return role == "admin" || role == "user";
                if (method == "DELETE") return role == "admin";
            }

            // Search endpointleri
            if (path.StartsWith("/api/search", StringComparison.OrdinalIgnoreCase))
            {
                if (method == "GET") return role == "admin" || role == "user";
            }

            return false;
        }
    }
}