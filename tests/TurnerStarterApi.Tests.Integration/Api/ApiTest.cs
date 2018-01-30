using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Turner.Infrastructure.Mediator;
using TurnerStarterApi.Tests.Integration.Extensions;
using TurnerStarterApi.Tests.Integration.Helpers;

namespace TurnerStarterApi.Tests.Integration.Api
{
    [TestFixture]
    public class ApiTest
    {
        private Scope _scope;
        protected HttpClient ApiClient => AssemblySetup.ApiClient;
        protected Container Container => AssemblySetup.Container;
        protected DbContext DataContext => Container.GetService<DbContext>();

        [SetUp]
        public void Initialize()
        {
            _scope = AsyncScopedLifestyle.BeginScope(Container);
        }

        public async Task AssertResponseAsync(HttpResponseMessage actual, object expected)
        {
            await actual.EnsureOkStatusCode();

            var expectedJson = JsonConvert.SerializeObject(expected);
            var actualJson = await actual.Content.ReadAsStringAsync();
            Assert.AreEqual(actualJson, expectedJson);
        }

        public async Task<Response<T>> GetResponseAsync<T>(HttpResponseMessage actual)
        {
            var actualJson = await actual.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Response<T>>(actualJson);
        }

        [TearDown]
        public void CleanUp()
        {
            var config = Container.GetService<IConfiguration>();
            var connectionString = config.GetConnectionString("DataContext");
            new DatabaseDeleter(connectionString).DeleteAllData();

            _scope.Dispose();
        }
    }
}