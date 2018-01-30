using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TurnerStarterApi.Api.Configuration
{
    public static class LoggingConfiguration
    {
        public static void Configure(IApplicationBuilder app, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
        }
    }
}
