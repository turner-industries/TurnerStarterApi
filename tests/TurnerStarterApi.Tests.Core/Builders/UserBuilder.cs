using System;
using System.Security.Principal;
using TurnerStarterApi.Core.Features.Users;

namespace TurnerStarterApi.Tests.Core.Builders
{
    public class UserBuilder : Builder<UserBuilder, User>
    {
        public UserBuilder()
        {
            Entity = new User
            {
                FirstName = Guid.NewGuid().ToString(),
                LastName = Guid.NewGuid().ToString(),
                Role = Roles.User
            };
        }

        public UserBuilder AsCurrentUser()
        {
            var currentUser = WindowsIdentity.GetCurrent();

            Entity = new User
            {
                FirstName = "Test",
                LastName = "Admin",
                Role = Roles.Admin,
                Username = currentUser.Name.Split('\\')?[1]
            };

            return this;
        }

        public UserBuilder WithName(string firstName, string lastName)
        {
            Entity.FirstName = firstName;
            Entity.LastName = lastName;

            return this;
        }
    }
}
