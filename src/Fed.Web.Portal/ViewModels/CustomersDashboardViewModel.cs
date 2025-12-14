using Fed.Core.Entities;
using Fed.Web.Portal.Models.Reports;
using System.Collections.Generic;

namespace Fed.Web.Portal.ViewModels
{
    public class CustomersDashboardViewModel : BaseViewModel
    {
        public IList<Customer> Customers { get; set; }
        public ReportModel ReportModel { get; set; }
        public string LifecycleStatus { get; set; }
        public int? AccountTypeId { get; set; }
    }
}