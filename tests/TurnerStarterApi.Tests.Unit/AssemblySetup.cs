using System;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Turner.Infrastructure.Mediator;
using TurnerStarterApi.Core.Configuration;
using TurnerStarterApi.Core.Data;
using TurnerStarterApi.Core.Features.Security;
using TurnerStarterApi.Tests.Unit.Security;

namespace TurnerStarterApi.Tests.Unit
{
    [SetUpFixture]
    public class AssemblySetup
    {
        public static Container Container { get; private set; }

        [OneTimeSetUp]
        public void Init()
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            var mockIdentityContext = new MockIdentityContext();
            container.RegisterSingleton<IIdentityContext>(mockIdentityContext);

            container.Register<DbContext>(() =>
            {
                var options = new DbContextOptionsBuilder<DbContext>();
                options.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                options.UseInMemoryDatabase(Guid.NewGuid().ToString());
                return new DataContext(mockIdentityContext, options.Options);
            }, Lifestyle.Scoped);

            container.ConfigureCore(null);

            Container = container;

            container.Register(typeof(IValidator<>), new[] { typeof(AssemblySetup).Assembly });
            container.Register(typeof(IRequestHandler<,>), new[] { typeof(AssemblySetup).Assembly });

            AutoMapperConfiguration.Configure(typeof(AssemblySetup).Assembly);
            FluentValidationConfiguration.Configure();
        }
    }
}