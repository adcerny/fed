using Fed.Api.External.FreshSalesService.Entities;
using Fed.Core.Entities;
using Fed.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Fed.Api.External.FreshSalesService
{
    public class FreshSalesService : ICRMService
    {

        private readonly string _apiKey;
        private readonly string _baseUri = "https://fedteam.freshsales.io";
        private readonly HttpClient _httpClient;

        public FreshSalesService(string apiKey)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUri);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", $"token={_apiKey}");
        }

        public async Task<bool> AddLead(Customer customer)
        {
            Lead lead = GetLeadFromCustomer(customer);

            var existingLead = await GetLeadFromFreshSales(lead.email);

            if (existingLead != null)
                return await UpdateLead(lead, existingLead.id);
            else
                return await CreateLead(lead);

        }

        private static Lead GetLeadFromCustomer(Customer customer)
        {
            var contact = customer.PrimaryContact;
            var deliveryAddress = contact.DeliveryAddresses?.Where(d => d.IsPrimary).FirstOrDefault();

            var lead = new Lead
            {
                email = contact.Email,
                work_number = contact.Phone,
                address = deliveryAddress?.AddressLine1,
                city = deliveryAddress?.Town,
                zipcode = deliveryAddress?.Postcode,
                first_name = contact.FirstName,
                last_name = contact.LastName,
                custom_field = new CustomField 
                { 
                    cf_prospect_notes = customer.Notes 
                },
                company = new Company
                {
                    name = customer.CompanyName,
                    address = deliveryAddress?.AddressLine1,
                    city = deliveryAddress?.Town,
                    zipcode = deliveryAddress?.Postcode,
                    website = customer.Website,
                    number_of_employees = customer.OfficeSizeMax ?? customer.OfficeSizeMin,
                    phone = customer.PrimaryContact.Phone
                }
                
            };
            return lead;
        }

        private async Task<Lead> GetLeadFromFreshSales(string email)
        {
            var response = await _httpClient.GetAsync($"/api/lookup?q={email}&f=email&entities=lead");
            string result = response.Content.ReadAsStringAsync().Result;
            JObject root = JObject.Parse(result);
            var leads = root["leads"]["leads"].ToList();
            return leads.FirstOrDefault()?.ToObject<Lead>();
        }

        private async Task<bool> CreateLead(Lead lead)
        {
            using (var content = new StringContent(JsonConvert.SerializeObject(lead), System.Text.Encoding.UTF8, "application/json"))
            {
                var response = await _httpClient.PostAsync("/api/leads", content);
                string result = response.Content.ReadAsStringAsync().Result;
                return response.StatusCode == HttpStatusCode.OK;
            }
        }

        private async Task<bool> UpdateLead(Lead lead, string id)
        {
            using (var content = new StringContent(JsonConvert.SerializeObject(lead), System.Text.Encoding.UTF8, "application/json"))
            {
                var response = await _httpClient.PutAsync($"/api/leads/{id}", content);
                string result = response.Content.ReadAsStringAsync().Result;
                return response.StatusCode == HttpStatusCode.OK;
            }
        }
    }
}
