using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using Turner.Infrastructure.Mediator;

namespace TurnerStarterApi.Tests.Unit.Extensions
{
    public static class ResponseExtensions
    {
        public static void HasNoErrors(this Response response)
        {
            Assert.IsFalse(response.HasErrors, "Expected no errors but got: " + JsonConvert.SerializeObject(response.Errors));
        }

        public static void HasError(this Response response, string errorMessage, string propertyName = "")
        {
            Assert.IsTrue(response.HasErrors);

            var propertyErrors = response.Errors.Where(x => x.PropertyName == propertyName).ToList();
            if (propertyErrors.Count > 1)
            {
                Assert.Fail($"Expected 1 error for {propertyName} but found {propertyErrors.Count}. {JsonConvert.SerializeObject(propertyErrors)}");
            }

            var match = propertyErrors.FirstOrDefault(x => x.ErrorMessage == errorMessage);
            if (match != null)
            {
                return;
            }

            Assert.Fail(string.IsNullOrEmpty(propertyName)
                ? $"Error not found: {errorMessage}"
                : $"Error not found for {propertyName}: {errorMessage}", JsonConvert.SerializeObject(response));
        }
    }
}
