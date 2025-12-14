using Fed.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Web.Portal.ViewModels
{
    public class LabelViewModel
    {
        public Timeslot CurrentTimeslot { get; set; }
        public List<Timeslot> Timeslots { get; set; }
        public List<Delivery> Deliveries { get; set; }
        public int NumberOfLabels { get; set; }
    }
}
