using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using SimpleInjector;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.BackgroundJobs;
using Turner.Infrastructure.Mediator.Configuration;
using TurnerStarterApi.Core.Features.Common;

namespace TurnerStarterApi.Core.Configuration
{
    public static class SimpleInjectorCoreConfiguration
    {
        public static void ConfigureCore(this Container container, IConfiguration configuration)
        {
            var coreAssembly = typeof(SimpleInjectorCoreConfiguration).GetTypeInfo().Assembly;

            container.Register<IBackgroundJobMediator, BackgroundJobMediator>();
            container.Register(() => new BackgroundJobContext(false));
            container.Register(() => configuration);
            container.Register<ProjectionParameters>();

            container.ConfigureMediator(new[] { coreAssembly });

            container.Register(typeof(IValidator<>), new[] { typeof(SimpleInjectorCoreConfiguration).GetTypeInfo().Assembly });

            container.Register(typeof(IRequestHandler<,>), typeof(GetAllRequestHandler<,>));
            container.Register(typeof(IRequestHandler<,>), typeof(GetAllAdvancedRequestHandler<,>));
            container.Register(typeof(IRequestHandler<,>), typeof(GetByIdRequestHandler<,>));
            container.Register(typeof(IRequestHandler<>), typeof(DeleteByIdRequestHandler<>));
        }
    }
}
