using Dispatcher.API.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;

namespace Dispatcher.Tests
{
    [TestFixture]
    public class AuthTests
    {

        private AuthMiddleware CreateMiddleware(RequestDelegate? next = null)
        {
            Environment.SetEnvironmentVariable("Jwt__Secret",
                "bu-cok-gizli-bir-anahtar-en-az-32-karakter");

            var config = new ConfigurationBuilder().Build();
            return new AuthMiddleware(next ?? (_ => Task.CompletedTask), config);
        }


        [Test]
        public async Task Routing_ShouldReturn401Unauthorized_WhenTokenIsMissing()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var middleware = CreateMiddleware();

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
            var middleware = CreateMiddleware();

            await middleware.InvokeAsync(context);

        
            Assert.That(context.Response.StatusCode, Is.EqualTo(401));
        }

        [Test]
        public async Task ShouldAllow_LoginEndpoint_WithoutToken()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/auth/login";
       

            var middleware = CreateMiddleware();

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

                        var middleware = CreateMiddleware();


            await middleware.InvokeAsync(context);

            Assert.That(context.Response.StatusCode, Is.EqualTo(401));
        }

        [Test]
        public async Task InvokeAsync_ShouldReturn401_WhenAuthorizationHeaderDoesNotStartWithBearer()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Basic sometoken";
            context.Response.Body = new MemoryStream();

            var middleware = CreateMiddleware();


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

            var middleware = CreateMiddleware();


            await middleware.InvokeAsync(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var body = new StreamReader(context.Response.Body).ReadToEnd();

            Assert.That(context.Response.StatusCode, Is.EqualTo(401));
            Assert.That(body, Is.EqualTo("Bearer token is empty."));
        }

        [Test]
        public async Task InvokeAsync_ShouldCallNext_WhenTokenIsValid()
        {
            var nextCalled = false;
            var context = new DefaultHttpContext();

            // Environment variable'ı set et
            Environment.SetEnvironmentVariable("Jwt__Secret", "bu-cok-gizli-bir-anahtar-en-az-32-karakter");

            var key = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes("bu-cok-gizli-bir-anahtar-en-az-32-karakter"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            ));

            context.Request.Headers["Authorization"] = $"Bearer {token}";

            var config = new ConfigurationBuilder().Build();
            var middleware = new AuthMiddleware(
                _ => { nextCalled = true; return Task.CompletedTask; },
                config);

            await middleware.InvokeAsync(context);
            Assert.That(nextCalled, Is.True);
        }

        [Test]
        public async Task InvokeAsync_ShouldNotCallNext_WhenTokenIsMissing()
        {
            var nextCalled = false;
            var context = new DefaultHttpContext();

            var config = new ConfigurationBuilder()
                  .AddInMemoryCollection(new Dictionary<string, string?>
                  {
                       { "Jwt__Secret", "bu-cok-gizli-bir-anahtar-en-az-32-karakter" }
                  })
                  .Build();
            var middleware = new AuthMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, config);

            await middleware.InvokeAsync(context);

            Assert.That(nextCalled, Is.False);
        }
    }
}