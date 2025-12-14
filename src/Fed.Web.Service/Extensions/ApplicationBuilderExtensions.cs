using Fed.Web.Service.Middleware;
using Microsoft.AspNetCore.Builder;
using System;

namespace Fed.Web.Service.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseGlobalErrorHandler(this IApplicationBuilder builder) =>
            builder.UseMiddleware<GlobalErrorHandlerMiddleware>();

        public static IApplicationBuilder When(
            this IApplicationBuilder builder,
            bool predicate,
            Func<IApplicationBuilder> compose) => predicate ? compose() : builder;
    }
}