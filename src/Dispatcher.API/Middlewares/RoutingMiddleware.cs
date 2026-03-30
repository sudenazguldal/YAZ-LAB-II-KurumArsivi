using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace Dispatcher.API.Middlewares
{
    public class RoutingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public RoutingMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _next = next;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";

            string targetService = ResolveTargetServiceName(path);
            context.Items["TargetService"] = targetService;

            string? targetBaseUrl = ResolveTargetService(path);

            if (targetBaseUrl == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Route not found.");
                return;
            }

            var targetUrl = targetBaseUrl + path;

            if (context.Request.QueryString.HasValue)
                targetUrl += context.Request.QueryString.Value;

            await ForwardRequestAsync(context, targetUrl);
        }

        private string ResolveTargetServiceName(string path)
        {
            if (path.StartsWith("/api/auth"))
                return "login-service";

            if (path.StartsWith("/api/documents"))
                return "document-service";

            if (path.StartsWith("/api/search"))
                return "search-service";

            return "unknown";
        }

        private string? ResolveTargetService(string path)
        {
            if (path.StartsWith("/api/auth"))
                return Environment.GetEnvironmentVariable("LoginService__Url");

            if (path.StartsWith("/api/documents"))
                return Environment.GetEnvironmentVariable("DocumentService__Url");

            if (path.StartsWith("/api/search"))
                return Environment.GetEnvironmentVariable("SearchService__Url");

            return null;
        }

        private async Task ForwardRequestAsync(HttpContext context, string targetUrl)
        {
            var client = _httpClientFactory.CreateClient();

            var requestMessage = new HttpRequestMessage
            {
                Method = new HttpMethod(context.Request.Method),
                RequestUri = new Uri(targetUrl)
            };

            // Body kopyala
            if (context.Request.ContentLength > 0 || context.Request.ContentType != null)
            {
                var memoryStream = new MemoryStream();
                await context.Request.Body.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                requestMessage.Content = new StreamContent(memoryStream);

                if (context.Request.ContentType != null)
                {
                    requestMessage.Content.Headers.ContentType =
                        MediaTypeHeaderValue.Parse(context.Request.ContentType);
                }
            }

            // UserRole header ekle
            if (context.Items.TryGetValue("UserRole", out var userRole) && userRole != null)
            {
                requestMessage.Headers.TryAddWithoutValidation("X-User-Role", userRole.ToString());
            }

            // Header'ları kopyala
            foreach (var header in context.Request.Headers)
            {
                if (header.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                    continue;

                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            try
            {
                var response = await client.SendAsync(requestMessage);

                context.Response.StatusCode = (int)response.StatusCode;

                // 204 No Content ise body yazmadan çık
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return;
                }

                // Content-Type varsa taşı
                if (response.Content.Headers.ContentType != null)
                {
                    context.Response.ContentType = response.Content.Headers.ContentType.ToString();
                }

                var responseBody = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    await context.Response.WriteAsync(responseBody);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HttpRequestException: {ex.Message}");
                Console.WriteLine($"Inner: {ex.InnerException?.Message}");

                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 503;
                    await context.Response.WriteAsync("Service unavailable.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");

                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Internal error.");
                }
            }
        }
    }
}