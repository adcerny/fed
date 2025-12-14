using Fed.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Fed.Web.SupplierPortal.Middleware
{
    public class GlobalErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public GlobalErrorHandlerMiddleware(
            RequestDelegate next,
            ILogger logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            try
            {
                try
                {
                    await _next(ctx);
                }
                catch (FedException fedEx)
                {
                    var json = $"{{ \"errorCode\": {(int)fedEx.ErrorCode}, \"errorMessage\": \"{fedEx.Message}\" }}";
                    ctx.Response.StatusCode = 400;
                    ctx.Response.GetTypedHeaders().ContentType = new MediaTypeHeaderValue("application/json");
                    await ctx.Response.WriteAsync(json, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.LogError(ex.Message, ex);

                ctx.Response.StatusCode = 500;

                try
                {
                    var json = JsonConvert.SerializeObject(ex);
                    ctx.Response.GetTypedHeaders().ContentType = new MediaTypeHeaderValue("application/json");
                    await ctx.Response.WriteAsync(json, Encoding.UTF8);
                }
                catch (Exception ex2)
                {
                    await ctx.Response.WriteAsync(ex2.Message, Encoding.UTF8);
                }
            }
        }
    }
}
