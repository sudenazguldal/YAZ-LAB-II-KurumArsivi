using NUnit.Framework;

namespace Dispatcher.Tests
{
    [TestFixture]
    public class AuthTests
    {
        [Test]
        public void Routing_ShouldReturn401Unauthorized_WhenTokenIsMissing()
        {


            // Act
            int actualStatusCode = 200;

            // Assert
            Assert.That(actualStatusCode, Is.EqualTo(401), "ERROR: Dispatcher is allowing requests without a token! Expected 401 Unauthorized.");
        }
    }
}