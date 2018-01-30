using FluentValidation;

namespace TurnerStarterApi.Core.Configuration
{
    public static class FluentValidationConfiguration
    {
        public static void Configure()
        {
            ValidatorOptions.CascadeMode = CascadeMode.StopOnFirstFailure;
        }
    }
}
