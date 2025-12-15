using Fed.Core.Services.Validators;
using Fed.Web.Service.Config;
using Fed.Web.Service.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fed.Web.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostingEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddFed(Configuration, HostingEnvironment);

            services.AddControllersWithViews()
                .AddNewtonsoftJson(JsonConfig.MvcConfig);

            services.AddFluentValidationAutoValidation()
                    .AddFluentValidationClientsideAdapters();
            services.AddValidatorsFromAssemblyContaining<CustomerValidator>();

            services.AddSwaggerGen(OpenApiConfig.SwaggerGenConfig);
            services.AddSwaggerGenNewtonsoftSupport();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseGlobalErrorHandler()
                .When(!env.IsDevelopment(), app.UseHsts)
                .UseHttpsRedirection()
                .UseStaticFiles()
                .UseDefaultFiles()
                .UseRouting()
                .UseSwagger()
                .UseSwaggerUI(OpenApiConfig.SwaggerUIConfig)
                .UseCors("DefaultCorsPolicy")
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(
                      name: "default",
                      pattern: "{controller=Home}/{action=Index}/{id?}");
                    endpoints.MapRazorPages();
                })
                .Run(NotFoundHandler);
        }

        private readonly RequestDelegate NotFoundHandler =
            async ctx =>
            {
                ctx.Response.StatusCode = 404;
                await ctx.Response.WriteAsync("The requested resource does not exist or could not be found.");
            };
    }
}