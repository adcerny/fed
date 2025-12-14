using System;
using System.Collections.Generic;

namespace Fed.Web.Portal.ViewModels
{
    public class PicksheetViewModel
    {
        public IList<Fed.Core.Entities.Delivery> Deliveries { get; set; }
        public List<String> ProductOrder { get; set; }
    }
}
