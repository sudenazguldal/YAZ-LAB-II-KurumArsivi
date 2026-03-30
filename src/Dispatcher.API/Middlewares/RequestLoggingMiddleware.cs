using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dispatcher.API.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            var route = context.Request.Path.Value ?? "/";
            var method = context.Request.Method;
            var statusCode = context.Response.StatusCode;

            var targetService = context.Items.TryGetValue("TargetService", out var target)
                ? target?.ToString() ?? "unknown"
                : "unknown";

            _logger.LogInformation(
                "Route={Route} Method={Method} StatusCode={StatusCode} TargetService={TargetService}",
                route,
                method,
                statusCode,
                targetService);
        }
    }
}