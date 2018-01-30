using System.Collections.Generic;
using TurnerStarterApi.Core.Features.Security;

namespace TurnerStarterApi.Core.Features.Common
{
    public class ProjectionParameters : Dictionary<string, object>
    {
        public ProjectionParameters(IIdentityContext identityContext)
        {
            Add("currentUserId", identityContext.RequestingUser?.Id);
        }
    }
}
