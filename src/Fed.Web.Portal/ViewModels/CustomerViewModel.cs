using Fed.Api.External.ActivityLogs;
using Fed.Core.Entities;
using Fed.Core.ValueTypes;
using System.Collections.Generic;

namespace Fed.Web.Portal.ViewModels
{
    public class CustomerViewModel
    {
        public FullCustomerInfo Customer { get; set; }
        public IDictionary<Date, IList<RecurringOrder>> UpcomingOrders { get; set; }
        public IList<Product> Products { get; set; }
        public IList<CustomerActivity> LogonHistory { get; set; }
        public IList<Discount> Discounts { get; set; }
    }
}