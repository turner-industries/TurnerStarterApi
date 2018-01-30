using System;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Turner.Infrastructure.Mediator.BackgroundJobs;
using TurnerStarterApi.Core.Configuration;
using TurnerStarterApi.Core.Features.Security;
using TurnerStarterApi.Core.Features.Users;

namespace TurnerStarterApi.Api.Configuration
{
    public static class HangfireConfiguration
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddHangfire(x => x.UseSqlServerStorage(configuration.GetConnectionString("DataContext")));
        }

        public static void Configure(IApplicationBuilder app, IConfiguration configuration)
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.ConfigureCore(configuration);
            container.Register<IIdentityContext>(() => new NullIdentityContext());

            container.Options.AllowOverridingRegistrations = true;
            container.Register(() => new BackgroundJobContext(true));
            container.Options.AllowOverridingRegistrations = false;

            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                Activator = new SimpleInjectorJobActivator(container)
            });
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            });
        }

        private class SimpleInjectorJobActivator : JobActivator
        {
            private readonly Container _container;

            public SimpleInjectorJobActivator(Container container)
            {
                _container = container ?? throw new ArgumentNullException(nameof(container));
            }

            public override JobActivatorScope BeginScope(JobActivatorContext context)
            {
                var scope = AsyncScopedLifestyle.BeginScope(_container);
                return new SimpleInjectorScope(_container, scope);
            }

            public override object ActivateJob(Type jobType)
            {
                return _container.GetInstance(jobType);
            }
        }

        private class SimpleInjectorScope : JobActivatorScope
        {
            private readonly Container _container;
            private readonly Scope _scope;

            public SimpleInjectorScope(Container container, Scope scope)
            {
                _container = container;
                _scope = scope;
            }

            public override object Resolve(Type type)
            {
                return _container.GetInstance(type);
            }

            public override void DisposeScope()
            {
                _scope?.Dispose();
            }
        }

        private class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
        {
            public bool Authorize(DashboardContext context)
            {
                var httpContext = context.GetHttpContext();
                return httpContext.User.IsInRole(Roles.Admin);
            }
        }
    }

    public class NullIdentityContext : IIdentityContext
    {
        public User RequestingUser => null;
    }
}
