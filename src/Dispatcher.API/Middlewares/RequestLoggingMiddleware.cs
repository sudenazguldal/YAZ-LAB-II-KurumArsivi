using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

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
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                var route = context.Request.Path.Value ?? "/";
                var method = context.Request.Method;
                var statusCode = context.Response.StatusCode;

                var targetService = context.Items.TryGetValue("TargetService", out var target)
                    ? target?.ToString() ?? "unknown"
                    : "unknown";

                var username = context.Items.TryGetValue("Username", out var user)
                    ? user?.ToString() ?? "anonymous"
                    : "anonymous";

                var durationMs = stopwatch.ElapsedMilliseconds;

                _logger.LogError(
                    ex,
                    "Route={Route} Method={Method} StatusCode={StatusCode} TargetService={TargetService} Username={Username} DurationMs={DurationMs}",
                    route,
                    method,
                    statusCode,
                    targetService,
                    username,
                    durationMs);

                throw;
            }

            stopwatch.Stop();

            var successRoute = context.Request.Path.Value ?? "/";
            var successMethod = context.Request.Method;
            var successStatusCode = context.Response.StatusCode;

            var successTargetService = context.Items.TryGetValue("TargetService", out var successTarget)
                ? successTarget?.ToString() ?? "unknown"
                : "unknown";

            var successUsername = context.Items.TryGetValue("Username", out var successUser)
                ? successUser?.ToString() ?? "anonymous"
                : "anonymous";

            var successDurationMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Route={Route} Method={Method} StatusCode={StatusCode} TargetService={TargetService} Username={Username} DurationMs={DurationMs}",
                successRoute,
                successMethod,
                successStatusCode,
                successTargetService,
                successUsername,
                successDurationMs);
        }
    }
}