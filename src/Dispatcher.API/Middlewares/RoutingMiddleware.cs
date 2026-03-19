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

            Console.WriteLine($"Constructor - LoginService__Url: '{configuration["LoginService__Url"]}'");
            Console.WriteLine($"Constructor - ENV: '{Environment.GetEnvironmentVariable("LoginService__Url")}'");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";

            string? targetBaseUrl = ResolveTargetService(path);

            if (targetBaseUrl == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Route not found.");
                return;
            }

            await ForwardRequestAsync(context, targetBaseUrl + path);
        }

        private string? ResolveTargetService(string path)
        {
            Console.WriteLine($"PATH RECEIVED: '{path}'");
        
            string? url = null;

            if (path.StartsWith("/api/auth"))
                return Environment.GetEnvironmentVariable("LoginService__Url");

            if (path.StartsWith("/api/documents"))
                return Environment.GetEnvironmentVariable("DocumentService__Url");

            if (path.StartsWith("/api/search"))
                return Environment.GetEnvironmentVariable("SearchService__Url");

            Console.WriteLine($"LoginService__Url: '{_configuration["LoginService__Url"]}'");

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
                    requestMessage.Content.Headers.ContentType =
                        MediaTypeHeaderValue.Parse(context.Request.ContentType);
            }

            // UserRole'ü Items'dan al ve header olarak ekle
            if (context.Items.TryGetValue("UserRole", out var userRole) && userRole != null)
            {
                requestMessage.Headers.TryAddWithoutValidation("X-User-Role", userRole.ToString());
            }

            // Header'ları kopyala
            foreach (var header in context.Request.Headers)
            {
                // Content- ile başlayanları ve Host'u atlıyoruz
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
                context.Response.ContentType = "application/json";
                var responseBody = await response.Content.ReadAsStringAsync();
                await context.Response.WriteAsync(responseBody);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HttpRequestException: {ex.Message}");
                Console.WriteLine($"Inner: {ex.InnerException?.Message}");
                context.Response.StatusCode = 503;
                await context.Response.WriteAsync("Service unavailable.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal error.");
            }
        }
    }
}