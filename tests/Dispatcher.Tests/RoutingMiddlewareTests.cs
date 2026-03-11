using Dispatcher.API.Middlewares;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Dispatcher.Tests
{
    [TestFixture]
    public class RoutingMiddlewareTests
    {
        [Test]
        public async Task ShouldRoute_ToDocumentService_WhenPathStartsWithDocuments()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/documents";

            var middleware = new RoutingMiddleware(_ => Task.CompletedTask);

            await middleware.InvokeAsync(context);

            Assert.That(context.Response.Headers.ContainsKey("X-Routed-To"), Is.True);
            Assert.That(context.Response.Headers["X-Routed-To"].ToString(), Is.EqualTo("document-service"));
        }

        [Test]
        public async Task ShouldRoute_ToSearchService_WhenPathStartsWithSearch()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/search";

            var middleware = new RoutingMiddleware(_ => Task.CompletedTask);

            await middleware.InvokeAsync(context);

            Assert.That(context.Response.Headers.ContainsKey("X-Routed-To"), Is.True);
            Assert.That(context.Response.Headers["X-Routed-To"].ToString(), Is.EqualTo("search-service"));
        }

        [Test]

        public async Task ShouldReturn404_WhenPathIsUnknown()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/unknown";

            var middleware = new RoutingMiddleware(_ => Task.CompletedTask);

            await middleware.InvokeAsync(context);

            Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));

        }


    }
}