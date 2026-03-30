using Dispatcher.API.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Dispatcher.Tests
{
    public class TestLogger<T> : ILogger<T>
    {
        public List<string> Logs { get; } = new();

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Logs.Add(formatter(state, exception));
        }

        private class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();
            public void Dispose() { }
        }
    }

    [TestFixture]
    public class RequestLoggingMiddlewareTests
    {
        [Test]
        public async Task InvokeAsync_ShouldWriteLog_WhenRequestCompletesSuccessfully()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/documents";
            context.Request.Method = "GET";
            context.Items["TargetService"] = "document-service";

            var logger = new TestLogger<RequestLoggingMiddleware>();

            var middleware = new RequestLoggingMiddleware(
                async ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    await Task.CompletedTask;
                },
                logger);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.That(logger.Logs.Count, Is.EqualTo(1));

            var log = logger.Logs[0];
            Assert.That(log, Does.Contain("/api/documents"));
            Assert.That(log, Does.Contain("GET"));
            Assert.That(log, Does.Contain("200"));
            Assert.That(log, Does.Contain("document-service"));
        }

        [Test]
        public async Task InvokeAsync_ShouldWriteLog_WhenRequestReturns404()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/unknown";
            context.Request.Method = "GET";
            context.Items["TargetService"] = "unknown";

            var logger = new TestLogger<RequestLoggingMiddleware>();

            var middleware = new RequestLoggingMiddleware(
                async ctx =>
                {
                    ctx.Response.StatusCode = 404;
                    await Task.CompletedTask;
                },
                logger);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.That(logger.Logs.Count, Is.EqualTo(1));

            var log = logger.Logs[0];
            Assert.That(log, Does.Contain("/api/unknown"));
            Assert.That(log, Does.Contain("GET"));
            Assert.That(log, Does.Contain("404"));
            Assert.That(log, Does.Contain("unknown"));
        }

        [Test]
        public async Task InvokeAsync_ShouldWriteUsername_WhenUsernameExistsInContextItems()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/documents";
            context.Request.Method = "GET";
            context.Response.StatusCode = 200;
            context.Items["TargetService"] = "document-service";
            context.Items["Username"] = "sudenaz";

            var logger = new TestLogger<RequestLoggingMiddleware>();

            var middleware = new RequestLoggingMiddleware(
                async ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    await Task.CompletedTask;
                },
                logger);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.That(logger.Logs.Count, Is.EqualTo(1));

            var log = logger.Logs[0];
            Assert.That(log, Does.Contain("sudenaz"));
        }
    }
}