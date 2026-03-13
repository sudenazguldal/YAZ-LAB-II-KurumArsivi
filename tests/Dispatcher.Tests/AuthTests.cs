using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using System.Threading.Tasks;
using Dispatcher.API.Middlewares;

namespace Dispatcher.Tests
{
    [TestFixture]
    public class AuthTests
    {
        [Test]
        public async Task Routing_ShouldReturn401Unauthorized_WhenTokenIsMissing()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var middleware = new AuthMiddleware(innerHttpContext => Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(context);

            // Assert 
            Assert.That(context.Response.StatusCode, Is.EqualTo(401));

        }


        [Test]
        public async Task ShouldReturn401_WhenTokenIsInvalid()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Bearer gecersiz_token";
            var middleware = new AuthMiddleware(innerHttpContext => Task.CompletedTask);

            await middleware.InvokeAsync(context);

        
            Assert.That(context.Response.StatusCode, Is.EqualTo(401));
        }

        [Test]
        public async Task ShouldAllow_LoginEndpoint_WithoutToken()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/login";
       

            var middleware = new AuthMiddleware(innerHttpContext => Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(context);

            // Assert — 401 OLMAMALI
            Assert.That(context.Response.StatusCode, Is.Not.EqualTo(401));
        }



        [Test]
        public async Task ShouldReturn401_WhenRequestIsForDocumentsPath_AndTokenIsMissing()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/documents";

            var middleware = new AuthMiddleware(_ => Task.CompletedTask);

            await middleware.InvokeAsync(context);

            Assert.That(context.Response.StatusCode, Is.EqualTo(401));
        }

        [Test]
        public async Task InvokeAsync_ShouldReturn401_WhenAuthorizationHeaderDoesNotStartWithBearer()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Basic sometoken";
            context.Response.Body = new MemoryStream();

            var middleware = new AuthMiddleware(_ => Task.CompletedTask);

            await middleware.InvokeAsync(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var body = new StreamReader(context.Response.Body).ReadToEnd();

            Assert.That(context.Response.StatusCode, Is.EqualTo(401));
            Assert.That(body, Is.EqualTo("Invalid token format.")); //  Bearer kapalıyken bu mesaj gelmez
        }

        [Test]
        public async Task InvokeAsync_ShouldReturn401_WhenBearerTokenIsEmpty()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Bearer ";
            context.Response.Body = new MemoryStream();

            var middleware = new AuthMiddleware(_ => Task.CompletedTask);

            await middleware.InvokeAsync(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var body = new StreamReader(context.Response.Body).ReadToEnd();

            Assert.That(context.Response.StatusCode, Is.EqualTo(401));
            Assert.That(body, Is.EqualTo("Bearer token is empty."));
        }
    }
}