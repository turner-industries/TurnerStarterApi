using System;
using System.Linq;
using System.Security.Principal;
using Microsoft.EntityFrameworkCore;
using TurnerStarterApi.Core.Features.Security;
using TurnerStarterApi.Core.Features.Users;

namespace TurnerStarterApi.Api.Security
{
    public class HttpIdentityContext : IIdentityContext
    {
        private readonly DbContext _dataContext;
        private User _cachedUser;

        public HttpIdentityContext(DbContext dataContext)
        {
            _dataContext = dataContext;
        }

        public User RequestingUser
        {
            get
            {
                var currentUser = WindowsIdentity.GetCurrent();
                if (!currentUser.IsAuthenticated)
                {
                    return null;
                }

                var username = currentUser.Name.Split('\\')?[1];
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

    public class HttpIdentityContextProxy : IIdentityContext
    {
        private readonly Lazy<HttpIdentityContext> _wrapped;

        public HttpIdentityContextProxy(Lazy<HttpIdentityContext> wrapped)
        {
            _wrapped = wrapped;
        }

        public User RequestingUser => _wrapped.Value.RequestingUser;
    }
}
