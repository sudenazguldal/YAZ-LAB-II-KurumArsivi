using System.Diagnostics;
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
            var stopwatch = Stopwatch.StartNew();
            Exception? exception = null;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
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

                if (exception == null)
                {
                    _logger.LogInformation(
                        "Route={Route} Method={Method} StatusCode={StatusCode} TargetService={TargetService} Username={Username} DurationMs={DurationMs}",
                        route,
                        method,
                        statusCode,
                        targetService,
                        username,
                        durationMs);
                }
                else
                {
                    _logger.LogError(
                        exception,
                        "Route={Route} Method={Method} StatusCode={StatusCode} TargetService={TargetService} Username={Username} DurationMs={DurationMs}",
                        route,
                        method,
                        statusCode,
                        targetService,
                        username,
                        durationMs);
                }
            }
        }
    }
}