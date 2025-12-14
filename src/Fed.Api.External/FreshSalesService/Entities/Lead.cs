using System;
using System.Collections.Generic;
using System.Text;

namespace Fed.Api.External.FreshSalesService.Entities
{
    public class Lead
    {
        public string id { get; set; }
        public string job_title { get; set; }
        public string department { get; set; }
        public string email { get; set; }
        public string work_number { get; set; }
        public string mobile_number { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zipcode { get; set; }
        public string country { get; set; }
        public string time_zone { get; set; }
        public bool do_not_disturb { get; set; }
        public string display_name { get; set; }
        public string avatar { get; set; }
        public string keyword { get; set; }
        public string medium { get; set; }
        public string last_seen { get; set; }
        public string last_contacted { get; set; }
        public int lead_score { get; set; }
        public DateTime stage_updated_time { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public Company company { get; set; }
        public string deal { get; set; }
        public Links links { get; set; }
        public DateTime updated_at { get; set; }
        public bool has_authority { get; set; }
        public string facebook { get; set; }
        public string twitter { get; set; }
        public string linkedin { get; set; }
        public CustomField custom_field { get; set; }
    }
}
