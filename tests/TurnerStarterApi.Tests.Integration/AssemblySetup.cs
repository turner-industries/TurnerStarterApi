using System;
using System.IO;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using TurnerStarterApi.Api;
using TurnerStarterApi.Core.Features.Users;
using TurnerStarterApi.Tests.Core.Builders;

namespace TurnerStarterApi.Tests.Integration
{
    [SetUpFixture]
    public class AssemblySetup
    {
        public static TestServer ApiServer { get; private set; }
        public static HttpClient ApiClient { get; private set; }
        public static Container Container { get; private set; }

        [OneTimeSetUp]
        public static async Task Init()
        {
            InitConnectionString();
            InitApi();
            await AddCurrentUser();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        private static void InitConnectionString()
        {
            var appsettings = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var content = File.ReadAllText(appsettings);
            content = content.Replace("{database}", "TurnerStarterApi.Integration." + Guid.NewGuid());
            File.WriteAllText(appsettings, content);
        }

        private static void InitApi()
        {
            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>();
            ApiServer = new TestServer(builder);
            ApiClient = ApiServer.CreateClient();
            Container = (Container)ApiServer.Host.Services.GetService(typeof(Container));
        }

        private static async Task AddCurrentUser()
        {
            using (AsyncScopedLifestyle.BeginScope(Container))
            {
                var dbContext = Container.GetService<DbContext>();
                await UserBuilder.Instance().AsCurrentUser().PersistAndBuild(dbContext);
            }
        }

        [OneTimeTearDown]
        public static void AssemblyCleanup()
        {
            using (AsyncScopedLifestyle.BeginScope(Container))
            {
                var dbContext = Container.GetService<DbContext>();
                dbContext.Database.EnsureDeleted();
            }
        }
    }
}