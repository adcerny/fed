using Fed.Api.External.AbelAndColeService;
using Fed.Api.External.MerchelloService;
using Fed.Api.External.MicrosoftTeams;
using Fed.Api.External.SendGridService;
using Fed.Core.Services;
using Fed.Infrastructure.Data.SqlServer;
using Fed.Web.Service.Client;
using Microsoft.Extensions.Logging;

namespace Fed.AzureFunctions.Entities
{
    public class ServicesBag
    {
        public ILogger Logger { get; set; }
        public Config Config { get; set; }
        public SqlServerConfig SqlConfig { get; set; }
        public FedBot FedBot { get; set; }
        public FedWebClient FedClient { get; set; }
        public MerchelloAPIClient MerchelloClient { get; set; }
        public OrderProductsService AbelAndColeClient { get; set; }
        public SendGridService SendGridService { get; set; }
        public SuppliersService SuppliersService { get; set; }
        public MinimumOrderService MinimumOrderService { get; set; }
    }
}