using TurnerStarterApi.Core.Features.Users;

namespace TurnerStarterApi.Core.Features.Security
{
    public interface IIdentityContext
    {
        User RequestingUser { get; }
    }
}
