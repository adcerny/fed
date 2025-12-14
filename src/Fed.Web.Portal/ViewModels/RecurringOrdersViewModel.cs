using Fed.Core.Entities;
using Fed.Core.ValueTypes;
using System.Collections.Generic;

namespace Fed.Web.Portal.ViewModels
{
    public class RecurringOrdersViewModel
    {
        public RecurringOrdersViewModel(
            IList<Timeslot> timeslots,
            IList<Customer> customers,
            IList<Product> products,
            IList<RecurringOrder> recurringOrders,
            Date selectedDate)
        {
            Timeslots = timeslots;
            Customers = customers;
            Products = products;
            RecurringOrders = recurringOrders;
            SelectedDate = selectedDate;
        }

        public IList<Timeslot> Timeslots { get; }
        public IList<Customer> Customers { get; }
        public IList<Product> Products { get; }
        public IList<RecurringOrder> RecurringOrders { get; }
        public Date SelectedDate { get; }
    }
}