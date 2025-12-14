using Fed.Web.Portal.Models.Reports;
using System;

namespace Fed.Web.Portal.ViewModels
{
    public class SalesDashboardViewModel : BaseViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public ReportModel ReportModel { get; set; }
        public SalesSummary SalesSummary { get; set; }
    }
}