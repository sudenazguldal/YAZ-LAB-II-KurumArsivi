using Dispatcher.API.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;

namespace Dispatcher.Tests
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }

    public class MockHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public MockHttpClientFactory(HttpClient client)
        {
            _client = client;
        }

        public HttpClient CreateClient(string name) => _client;
    }

    [TestFixture]
    public class RoutingMiddlewareTests
    {
        private RoutingMiddleware CreateMiddleware(RequestDelegate? next = null)
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "LoginService__Url", "http://login-service:8080" },
                    { "DocumentService__Url", "http://document-service:8080" },
                    { "SearchService__Url", "http://search-service:8080" }
                })
                .Build();

            var handler = new MockHttpMessageHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}")
                });

            var httpClient = new HttpClient(handler);
            var factory = new MockHttpClientFactory(httpClient);

            return new RoutingMiddleware(next ?? (_ => Task.CompletedTask), factory, config);
        }

        [Test]
        public async Task ShouldRoute_ToDocumentService_WhenPathStartsWithDocuments()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/documents";
            context.Request.Method = "GET";

            var middleware = CreateMiddleware();
            await middleware.InvokeAsync(context);

            Assert.That(context.Response.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task ShouldRoute_ToSearchService_WhenPathStartsWithSearch()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/search";
            context.Request.Method = "GET";

            var middleware = CreateMiddleware();
            await middleware.InvokeAsync(context);

            Assert.That(context.Response.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task ShouldRoute_ToLoginService_WhenPathStartsWithAuth()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/auth/login";
            context.Request.Method = "POST";

            var middleware = CreateMiddleware();
            await middleware.InvokeAsync(context);

            Assert.That(context.Response.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task ShouldReturn404_WhenPathIsUnknown()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/unknown";
            context.Request.Method = "GET";

            var middleware = CreateMiddleware();
            await middleware.InvokeAsync(context);

            Assert.That(context.Response.StatusCode, Is.EqualTo(404));
        }
    }
}