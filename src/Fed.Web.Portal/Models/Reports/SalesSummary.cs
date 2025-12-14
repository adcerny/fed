using System.Collections.Generic;

namespace Fed.Web.Portal.Models.Reports
{
    public class SalesSummary
    {
        public string Week1Name { get; set; }
        public string Week2Name { get; set; }
        public string Week3Name { get; set; }
        public string Week4Name { get; set; }
        public IList<AccountTypeStat> SalesStats { get; set; }
    }
}