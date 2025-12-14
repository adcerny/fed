using System;

namespace Fed.Web.Portal.ViewModels
{
    public class PublicHolidayViewModel
    {
        public int Id { get; }
        public string Name { get; set; }
        public string Date { get; set; }
        public DateTime DateAdded { get; }
    }
}
