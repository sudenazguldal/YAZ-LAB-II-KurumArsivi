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

            // Şu an kod bunu GEÇMEYECEK çünkü 
            // AuthMiddleware token'ı doğrulamıyor var mı yok mu bakıyor
            Assert.That(context.Response.StatusCode, Is.EqualTo(401));
        }

        [Test]
        public async Task ShouldAllow_LoginEndpoint_WithoutToken()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/login";
            // Token YOK ama /login geçmeli

            var middleware = new AuthMiddleware(innerHttpContext => Task.CompletedTask);

            // Act
            await middleware.InvokeAsync(context);

            // Assert — 401 OLMAMALI
            Assert.That(context.Response.StatusCode, Is.Not.EqualTo(401));
        }

        [Test]
        public async Task ShouldRoute_ToDocumentService_WhenPathStartsWithDocuments()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/documents";
            // Token YOK — whitelist'e alacağız

            var middleware = new AuthMiddleware(innerHttpContext => Task.CompletedTask);
            await middleware.InvokeAsync(context);

            Assert.That(context.Response.Headers.ContainsKey("X-Routed-To"), Is.True);
        }
    }
}