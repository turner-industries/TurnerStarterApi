using TurnerStarterApi.Core.Features.Security;
using TurnerStarterApi.Core.Features.Users;

namespace TurnerStarterApi.Tests.Unit.Security
{
    public class MockIdentityContext : IIdentityContext
    {
        public User RequestingUser { get; set; }
    }
}
