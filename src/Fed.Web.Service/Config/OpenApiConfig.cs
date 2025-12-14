using Fed.Web.Service.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.IO;
using System.Reflection;

namespace Fed.Web.Service.Config
{
    public static class OpenApiConfig
    {
        private static string Name => "Fed by Abel and Cole";
        private static string Version => "v1";
        private static string Endpoint => $"/swagger/{Version}/swagger.json";
        private static string UIEndpoint => "";

        public static void SwaggerUIConfig(SwaggerUIOptions config)
        {
            config.RoutePrefix = UIEndpoint;
            config.SwaggerEndpoint(Endpoint, Name);
        }

        public static void SwaggerGenConfig(SwaggerGenOptions config)
        {
            config.SwaggerDoc(
                Version,
                new OpenApiInfo { Version = Version, Title = Name });

            config.SchemaFilter<FluentValidationFilter>();

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            config.IncludeXmlComments(xmlPath);
            config.CustomOperationIds(apiDesc =>
            {
                return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
            });
        }
    }
}