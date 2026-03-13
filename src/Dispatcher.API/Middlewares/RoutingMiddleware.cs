using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Dispatcher.API.Middlewares
{
    public class RoutingMiddleware
    {
        private readonly RequestDelegate _next;

        public RoutingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/api/documents"))
            {
                context.Response.Headers["X-Routed-To"] = "document-service";
                await _next(context);
                return;
            }

             if (context.Request.Path.StartsWithSegments("/api/search"))
            {
                context.Response.Headers["X-Routed-To"] = "search-service";
                await _next(context);
                return;
            }

            //hiçbir route eşleşmezse 404 dön
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync("Route not found.");



        }


    }
}