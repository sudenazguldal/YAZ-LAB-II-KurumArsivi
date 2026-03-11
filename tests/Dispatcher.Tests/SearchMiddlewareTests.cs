using Dispatcher.API.Middlewares;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dispatcher.Tests
{
    internal class SearchMiddlewareTests
    {
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
    }
}
