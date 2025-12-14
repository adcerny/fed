using System;
using System.Collections.Generic;
using System.Text;

namespace Fed.Api.External.FreshSalesService.Entities
{
    public class Company
    {
        public string id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zipcode { get; set; }
        public string country { get; set; }
        public int? number_of_employees { get; set; }
        public string annual_revenue { get; set; }
        public string website { get; set; }
        public string phone { get; set; }
        public string industry_type_id { get; set; }
        public string business_type_id { get; set; }
    }
}
