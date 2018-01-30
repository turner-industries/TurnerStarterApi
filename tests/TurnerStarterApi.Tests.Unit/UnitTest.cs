using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Turner.Infrastructure.Mediator;
using TurnerStarterApi.Core.Features.Security;
using TurnerStarterApi.Tests.Core.Builders;
using TurnerStarterApi.Tests.Unit.Security;

namespace TurnerStarterApi.Tests.Unit
{
    public class UnitTest
    {
        private Scope _scope;
        protected Container Container => AssemblySetup.Container;
        protected IMediator Mediator => Container.GetInstance<IMediator>();
        protected DbContext DataContext => Container.GetService<DbContext>();
        protected int RequestingUserId;

        [SetUp]
        public void Initialize()
        {
            _scope = AsyncScopedLifestyle.BeginScope(Container);

            var identityContext = (MockIdentityContext)Container
                .GetInstance<IIdentityContext>();

            var requestingUser = UserBuilder.Instance()
                .AsCurrentUser()
                .PersistAndBuild(DataContext)
                .Result;
            identityContext.RequestingUser = requestingUser;
            RequestingUserId = identityContext.RequestingUser.Id;
        }

        [TearDown]
        public void CleanUp()
        {
            _scope.Dispose();
        }
    }
}
