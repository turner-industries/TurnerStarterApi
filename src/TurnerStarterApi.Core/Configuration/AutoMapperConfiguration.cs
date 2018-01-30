using System.Reflection;
using AutoMapper;

namespace TurnerStarterApi.Core.Configuration
{
    public static class AutoMapperConfiguration
    {
        public static void Configure(params Assembly[] additionalAssemblies)
        {
            Mapper.Initialize(config =>
            {
                config.AddProfiles(typeof(AutoMapperConfiguration).GetTypeInfo().Assembly);
                config.AddProfiles(additionalAssemblies);
            });
        }
    }
}
