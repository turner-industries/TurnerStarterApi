using NUnit.Framework;
using TurnerStarterApi.Core.Features.Security;

namespace TurnerStarterApi.Tests.Unit.Security
{
    [TestFixture]
    public class TokenGeneratorTests
    {
        [Test]
        public void Generate_ReturnsToken()
        {
            var token = TokenGenerator.Generate();
            Assert.IsNotNull(token);
        }
    }
}
