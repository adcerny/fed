using Fed.Api.External.AbelAndColeService;
using Fed.Api.External.MerchelloService;
using Fed.Api.External.MicrosoftTeams;
using Fed.Api.External.SendGridService;
using Fed.AzureFunctions.Entities;
using Fed.Core.Services;
using Fed.Infrastructure.Data.SqlServer;
using Fed.Infrastructure.Data.SqlServer.Handlers;
using Fed.Web.Service.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Core
{
    public static class FunctionRunner
    {
        private static async Task<TResult> RunAsync<TArg, TResult>(
            string functionName,
            ILogger logger,
            TArg arg,
            Func<TArg, ServicesBag, Task<TResult>> funcWithArgAndResult = null,
            Func<TArg, ServicesBag, Task> funcWithArg = null,
            Func<ServicesBag, Task> funcWithoutArgAndWithoutResult = null,
            Func<Exception, TResult> errorHandler = null)
        {
            try
            {
                logger.LogInformation($"Function '{functionName}' started at: {DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}");

                logger.LogInformation("Loading configuration from environment variables...");

                var config = Config.LoadFromEnvironment();

                logger.LogInformation("Initialising services...");

                var fedBot = FedBot.Create(logger, config.TeamsWebhookUrl);

                var fedWebClient = new FedWebClient(
                    logger,
                    new HttpClient(),
                    config.FedWebServiceUrl);

                var acOrderClient = new OrderProductsService(
                    logger,
                    new HttpClient(),
                    config.AbelAndColeOrderApiUrl);

                var merchelloClient = new MerchelloAPIClient(
                    new HttpClient(),
                    config.MerchelloApiUrl,
                    config.MerchelloSecretToken,
                    logger);

                var sendGridService = new SendGridService(
                    config.SendGridApiKey,
                    config.SendGridMarketingSettings,
                    new HttpClient(),
                    logger);

                var sqlConfig = new SqlServerConfig(config.ConnectionString);

                var bakeryMinimumOrderService = new MinimumOrderService(
                    new ProductsHandler(sqlConfig));

                var suppliersService = new SuppliersService(
                    logger,
                    new SuppliersHandler(sqlConfig),
                    new ReportingHandler(sqlConfig),
                    bakeryMinimumOrderService);

                var bag =
                    new ServicesBag
                    {
                        Logger = logger,
                        Config = config,
                        SqlConfig = sqlConfig,
                        FedClient = fedWebClient,
                        MerchelloClient = merchelloClient,
                        FedBot = fedBot,
                        AbelAndColeClient = acOrderClient,
                        SendGridService = sendGridService,
                        SuppliersService = suppliersService,
                        MinimumOrderService = bakeryMinimumOrderService
                    };

                try
                {
                    logger.LogInformation("Executing custom function code...");

                    if (funcWithArgAndResult != null)
                    {
                        return await funcWithArgAndResult(arg, bag);
                    }
                    else if (funcWithArg != null)
                    {
                        await funcWithArg(arg, bag);
                    }
                    else if (funcWithoutArgAndWithoutResult != null)
                    {
                        await funcWithoutArgAndWithoutResult(bag);
                    }

                    return default(TResult);
                }
                catch (Exception ex)
                {
                    logger.LogError("An exception has been thrown: {ex}.", ex);
                    await bag.FedBot.SendMessage(TeamsCard.CreateError(ex, functionName));

                    return
                        errorHandler == null
                        ? default(TResult)
                        : errorHandler(ex);
                }
            }
            catch (Exception ex)
            {
                logger.LogError("An exception has been thrown initialising the function: {ex}.", ex);

                return
                    errorHandler == null
                    ? default(TResult)
                    : errorHandler(ex);
            }
            finally
            {
                logger.LogInformation($"Function '{functionName}' finished at: {DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}");
            }
        }

        public static Task RunAsync(ILogger logger, string functionName, Func<ServicesBag, Task> function)
            => RunAsync<object, object>(
                functionName,
                logger,
                null,
                funcWithoutArgAndWithoutResult: function);

        public static Task<object> RunWithArgAsync<TArg>(
            ILogger logger,
            string functionName,
            TArg arg,
            Func<TArg, ServicesBag, Task> function,
            Func<Exception, object> errorHandler = null)
            => RunAsync(
                functionName,
                logger,
                arg,
                funcWithArg: function,
                errorHandler: errorHandler);

        public static Task<HttpResponseMessage> RunWebAsync(
            ILogger logger,
            string functionName,
            HttpRequestMessage request,
            Func<HttpRequestMessage, ServicesBag, Task<HttpResponseMessage>> function)
            => RunAsync(
                functionName,
                logger,
                request,
                funcWithArgAndResult: function,
                errorHandler: (ex) => request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));

        public static Task<HttpResponseMessage> RunWebWithArgsAsync<TArg>(
            ILogger logger,
            string functionName,
            HttpRequestMessage request,
            TArg arg,
            Func<(HttpRequestMessage, TArg), ServicesBag, Task<HttpResponseMessage>> function)
            => RunAsync(
                functionName,
                logger,
                (request, arg),
                funcWithArgAndResult: function,
                errorHandler: (ex) => request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
    }
}