using Fed.Core.Entities;
using Fed.Core.ValueTypes;
using System.Collections.Generic;

namespace Fed.Web.Portal.ViewModels
{
    public class DeliveriesViewModel
    {
        public DeliveriesViewModel(Date selectedDate)
        {
            SelectedDate = selectedDate;
        }

        public Date SelectedDate { get; }
        public IList<Delivery> Deliveries { get; set; }
        public string SortBy { get; set; }
        public bool Asc { get; set; }
    }
}