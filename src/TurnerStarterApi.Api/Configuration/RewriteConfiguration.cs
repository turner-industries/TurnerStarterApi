using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;

namespace TurnerStarterApi.Api.Configuration
{
    public class RewriteConfiguration
    {
        public static void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var rewriteOptions = new RewriteOptions();

            if (env.IsDevelopment())
            {
                rewriteOptions
                    .AddRedirect("^$", "docs");
            }
            else
            {
                rewriteOptions
                    .AddRewrite(@"^/((?!\.).)*$", "index.html", true);
            }

            app.UseRewriter(rewriteOptions);
        }
    }
}
