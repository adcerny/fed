using Fed.Web.Portal.Config;
using Fed.Web.Portal.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fed.Web.Portal
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddFed(Configuration);
            //https://docs.microsoft.com/en-us/aspnet/core/migration/22-to-30?view=aspnetcore-2.2&tabs=visual-studio#jsonnet-support
            services.AddMvc().AddNewtonsoftJson();
            services.AddControllersWithViews(options =>
            {
                options.ModelBinderProviders.Insert(0, new DateBinderProvider());
            }).AddRazorRuntimeCompilation();
            services.AddServerSideBlazor();
            services.AddRazorPages();
            services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseGlobalErrorHandler()
                .When(!env.IsDevelopment(), app.UseHsts)
                .UseHttpsRedirection()
                .UseStaticFiles()
                .UseDefaultFiles()
                .UseRouting()
                .UseHttpsRedirection()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(
                      name: "default",
                      pattern: "{controller=Home}/{action=Index}/{id?}");
                    endpoints.MapRazorPages();
                    endpoints.MapBlazorHub();
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
