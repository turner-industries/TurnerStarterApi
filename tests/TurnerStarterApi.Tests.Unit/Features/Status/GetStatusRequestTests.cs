using System.Threading.Tasks;
using NUnit.Framework;
using TurnerStarterApi.Core.Features.Status;

namespace TurnerStarterApi.Tests.Unit.Features.Status
{
    [TestFixture]
    public class GetStatusRequestTests : UnitTest
    {
        [Test]
        public async Task ValidRequest_ReturnsStatusMessage()
        {
            // Act
            var result = await Mediator.HandleAsync(new GetStatusRequest());

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(result.Data, "We're up and running!");
        }
    }
}
