using Fed.Core.Entities;
using Fed.Web.Portal.Models.Reports;
using System.Collections.Generic;

namespace Fed.Web.Portal.ViewModels
{
    public class HomeDashboardViewModel : BaseViewModel
    {
        public ReportModel NewCustomerSummary { get; set; }
        public ReportModel AccountsCreatedReport { get; set; }
        public IList<AccountsCreatedViewModel> AccountsCreated { get; set; }
        public IList<CustomerMarketingAttribute> CustomerMarketingAttributes { get; set; }
    }
}