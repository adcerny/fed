using Fed.Core.Entities;
using System.Collections.Generic;

namespace Fed.Web.Portal.ViewModels
{
    public class SystemDashboardViewModel : BaseViewModel
    {
        public AppInfo ServiceInfo { get; set; }
        public AppInfo PortalInfo { get; set; }
        public string ProductSyncUrl { get; set; }
        public string BrainTreeMigrationUrl { get; set; }
        public IList<CustomerAgent> CustomerAgents { get; set; }
        public IList<CustomerMarketingAttribute> CustomerMarketingAttributes { get; set; }
        public IList<ProductCategory> ProductCategories { get; set; }
    }
}