using Fed.Web.Portal.Models.Reports;

namespace Fed.Web.Portal.ViewModels
{
    public class ReportingViewModel
    {
        public ReportingViewModel(
            string title,
            ReportModel reportModel)
        {
            Title = title;
            ReportModel = reportModel;
        }

        public string Title { get; }
        public ReportModel ReportModel { get; }
    }
}