using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TurnerStarterApi.Api.Configuration
{
    public class CorsConfiguration
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
        }

        public static void Configure(IApplicationBuilder app, IConfiguration configuration)
        {
            var origins = configuration["CorsOrigins"].Split(",");
            app.UseCors(builder =>
            {
                builder
                    .WithOrigins(origins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        }
    }
}
