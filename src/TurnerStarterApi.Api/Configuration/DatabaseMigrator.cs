using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TurnerStarterApi.Core.Data;

namespace TurnerStarterApi.Api.Configuration
{
    public class DatabaseConfiguration
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DataContext>(options => options
                .UseSqlServer(configuration.GetConnectionString("DataContext")));
        }

        public static void Configure(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var dataContext = scope.ServiceProvider.GetService<DataContext>();
                dataContext.Database.Migrate();
                dataContext.Seed();
            }
        }
    }
}
