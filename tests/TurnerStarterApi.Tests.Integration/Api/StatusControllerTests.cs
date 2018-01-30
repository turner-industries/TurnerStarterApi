using System.Threading.Tasks;
using NUnit.Framework;
using Turner.Infrastructure.Mediator;

namespace TurnerStarterApi.Tests.Integration.Api
{
    [TestFixture]
    public class StatusControllerTests : ApiTest
    {
        [Test]
        public async Task GetAsync_ReturnsUpAndRunning()
        {
            // Arrange
            var expected = new Response<string>
            {
                Data = "We're up and running!"
            };

            // Act
            var actual = await ApiClient.GetAsync("/api/v1/status");

            // Assert
            await AssertResponseAsync(actual, expected);
        }
    }
}
