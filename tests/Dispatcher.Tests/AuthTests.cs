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
            var context = new DefaultHttpContext();
            var middleware = new AuthMiddleware(innerHttpContext => Task.CompletedTask);

        }
    }
}