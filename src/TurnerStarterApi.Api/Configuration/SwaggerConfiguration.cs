using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TurnerStarterApi.Api.Configuration
{
    public static class SwaggerConfiguration
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Title = "Starter API",
                    Version = "v1",
                    Contact = new Contact
                    {
                        Name = "Turner Industries Group, LLC"
                    }
                });

                options.OperationFilter<HideIdFilter>();
            });
        }

        public static void Configure(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "docs";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Starter API v1");
            });
        }

        public class HideIdFilter : IOperationFilter
        {
            public void Apply(Operation operation, OperationFilterContext context)
            {
                if (operation?.Parameters == null)
                {
                    return;
                }

                foreach (var operationParameter in operation.Parameters.ToList())
                {
                    var bodyParameter = operationParameter as BodyParameter;
                    if (bodyParameter == null)
                    {
                        continue;
                    }

                    var parameterName = bodyParameter.Schema.Ref.Replace("#/definitions/", string.Empty);

                    var schema = context.SchemaRegistry.Definitions[parameterName];
                    foreach (var schemaProperty in schema.Properties.ToList())
                    {
                        if (schemaProperty.Key == "id")
                        {
                            schema.Properties.Remove(schemaProperty);
                        }
                    }
                }
            }
        }
    }
}
