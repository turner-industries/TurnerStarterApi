using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TurnerStarterApi.Core.Features.Security;
using TurnerStarterApi.Core.Features.Users;

namespace TurnerStarterApi.Api.Security
{
    public class MacHttpIdentityContext : IIdentityContext
    {
        private readonly DbContext _dataContext;
        private User _cachedUser;

        public MacHttpIdentityContext(DbContext dataContext)
        {
            _dataContext = dataContext;
        }

        public User RequestingUser
        {
            get
            {
                var username = "system";
                if (_cachedUser != null && _cachedUser.Username == username)
                {
                    return _cachedUser;
                }

                var user = _dataContext.Set<User>()
                    .FirstOrDefault(x => !x.IsDeleted && x.Username == username);

                _cachedUser = user;

                return user;
            }
        }
    }

    public class MacHttpIdentityContextProxy : IIdentityContext
    {
        private readonly Lazy<MacHttpIdentityContext> _wrapped;

        public MacHttpIdentityContextProxy(Lazy<MacHttpIdentityContext> wrapped)
        {
            _wrapped = wrapped;
        }

        public User RequestingUser => _wrapped.Value.RequestingUser;
    }
}
