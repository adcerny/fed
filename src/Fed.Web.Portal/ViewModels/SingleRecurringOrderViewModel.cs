using Fed.Core.Entities;
using System.Collections.Generic;

namespace Fed.Web.Portal.ViewModels
{
    public class SingleRecurringOrderViewModel
    {
        public SingleRecurringOrderViewModel(
            IList<Timeslot> timeslots,
            IList<Product> products,
            IList<SkipDate> skipDates,
            Customer customer,
            Contact contact,
            RecurringOrder recurringOrder)
        {
            Timeslots = timeslots;
            Products = products;
            SkipDates = skipDates;
            Customer = customer;
            Contact = contact;
            RecurringOrder = recurringOrder;
        }

        public IList<Timeslot> Timeslots { get; }
        public IList<Product> Products { get; }
        public IList<SkipDate> SkipDates { get; }

        public Customer Customer { get; }
        public Contact Contact { get; }
        public RecurringOrder RecurringOrder { get; }

        public string ContactFullName => Contact.Title is null
            ? $"{Contact.FirstName} {Contact.LastName}"
            : $"{Contact.Title} {Contact.FirstName} {Contact.LastName}";
    }
}
