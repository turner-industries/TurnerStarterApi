using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Integration.AspNetCore.Mvc;
using SimpleInjector.Lifestyles;
using Turner.Infrastructure.Mediator;
using TurnerStarterApi.Api.Controllers;
using TurnerStarterApi.Api.Security;
using TurnerStarterApi.Core.Configuration;
using TurnerStarterApi.Core.Data;
using TurnerStarterApi.Core.Features.Security;

namespace TurnerStarterApi.Api.Configuration
{
    public static class SimpleInjectorConfiguration
    {
        private static Container _container;

        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            _container = new Container();

            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            _container.Register<HttpIdentityContext>(Lifestyle.Scoped);
            _container.Register<IIdentityContext>(() =>
                new HttpIdentityContextProxy(new Lazy<HttpIdentityContext>(_container.GetInstance<HttpIdentityContext>))
            );

            _container.Register<DbContext>(() =>
            {
                var identityContext = _container.GetInstance<IIdentityContext>();

                var options = new DbContextOptionsBuilder<DataContext>();
                options.UseSqlServer(configuration.GetConnectionString("DataContext"));

                return new DataContext(identityContext, options.Options);
            }, Lifestyle.Scoped);

            _container.ConfigureCore(configuration);

            _container.RegisterInitializer<BaseApiController>(controller =>
            {
                controller.Mediator = _container.GetInstance<IMediator>();
            });

            services.AddSingleton(_container);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IControllerActivator>(new SimpleInjectorControllerActivator(_container));
            services.UseSimpleInjectorAspNetRequestScoping(_container);
            services.EnableSimpleInjectorCrossWiring(_container);
        }

        public static void Configure(IApplicationBuilder app)
        {
            _container.CrossWire<IHttpContextAccessor>(app);
            _container.Verify();
        }
    }
}
