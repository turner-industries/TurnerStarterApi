using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TurnerStarterApi.Tests.Integration.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static Task EnsureOkStatusCode(this HttpResponseMessage message)
        {
            return EnsureStatusCodeAsync(message, HttpStatusCode.OK);
        }

        public static Task EnsureCreatedStatusCodeAsync(this HttpResponseMessage message)
        {
            return EnsureStatusCodeAsync(message, HttpStatusCode.Created);
        }

        private static async Task EnsureStatusCodeAsync(HttpResponseMessage message, HttpStatusCode expectedStatusCode)
        {
            var response = await message.Content.ReadAsStringAsync();
            Assert.AreEqual(expectedStatusCode, message.StatusCode, $"Request failed with response: {response}");
        }
    }
}
