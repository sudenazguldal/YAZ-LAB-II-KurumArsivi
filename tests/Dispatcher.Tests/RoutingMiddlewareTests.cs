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
    }
}