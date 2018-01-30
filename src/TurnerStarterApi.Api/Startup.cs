using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TurnerStarterApi.Api.Configuration;
using TurnerStarterApi.Core.Configuration;

namespace TurnerStarterApi.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            SimpleInjectorConfiguration.ConfigureServices(services, _configuration);
            CorsConfiguration.ConfigureServices(services);
            MvcConfiguration.ConfigureServices(services);
            SwaggerConfiguration.ConfigureServices(services);
            HangfireConfiguration.ConfigureServices(services, _configuration);
            DatabaseConfiguration.ConfigureServices(services, _configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            DatabaseConfiguration.Configure(app);
            LoggingConfiguration.Configure(app, _configuration, loggerFactory);
            RewriteConfiguration.Configure(app, env);
            SimpleInjectorConfiguration.Configure(app);
            CorsConfiguration.Configure(app, _configuration);
            MvcConfiguration.Configure(app, env);
            SwaggerConfiguration.Configure(app);
            AutoMapperConfiguration.Configure();
            FluentValidationConfiguration.Configure();
            HangfireConfiguration.Configure(app, _configuration);
        }
    }
}
