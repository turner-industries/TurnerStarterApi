using System.Linq;
using Microsoft.EntityFrameworkCore;
using TurnerStarterApi.Core.Features.Users;

namespace TurnerStarterApi.Core.Data
{
    public static class DataSeeder
    {
        public static void Seed(this DbContext context)
        {
            SeedUsers(context);
        }

        private static void SeedUsers(DbContext context)
        {
            var users = context.Set<User>();
            if (users.Any())
                return;

            users.Add(new User
            {
                FirstName = "System",
                LastName = "User",
                Username = "system",
                Role = Roles.Admin
            });
            context.SaveChanges();
        }
    }
}
