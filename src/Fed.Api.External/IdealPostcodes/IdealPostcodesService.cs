using Fed.Core.Entities;
using Fed.Core.Services.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fed.Api.External.IdealPostcodesService
{
    public class IdealPostcodesService : IPostcodeLocationService, IPostcodeAddressesService
    {
        private readonly string _apiKey;
        private readonly IPostcodeLocationService _freeService;
        private readonly IPostcodeHubService _hubService;

        public IdealPostcodesService(string apiKey, IPostcodeLocationService freeService, IPostcodeHubService hubService)
        {
            _apiKey = apiKey;
            _freeService = freeService;
            _hubService = hubService;
        }

        public async Task<PostcodeLocation> GetPostcodeLocation(string postcode)
        {
            postcode = postcode.Trim();

            PostcodeLocation postcodeLocation = await _freeService.GetPostcodeLocation(postcode);

            if (postcodeLocation != null)
                return postcodeLocation;

            HttpResponseMessage response = await CallApi(postcode);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                var jsonObject = JObject.Parse(responseBody);

                if (!double.TryParse(
                        jsonObject.SelectToken("result[0].latitude").ToString(),
                        out double latitude))
                    return null;

                if (!double.TryParse(
                        jsonObject.SelectToken("result[0].longitude").ToString(),
                        out double longitude))
                    return null;

                return PostcodeLocation.Create(postcode, latitude, longitude);
            }
            else
                return null;
        }

        public async Task<List<DeliverableAddress>> GetDeliverableAddresses(string postcode)
        {
            postcode = postcode.Trim();

            if (string.IsNullOrWhiteSpace(postcode))
                throw new ArgumentException("Postcode cannot be null or empty.", nameof(postcode));
            if (!PostcodeLocation.IsPostcodeValid(postcode))
                throw new ArgumentException("Postcode is invalid.", nameof(postcode));


            Guid hubId = await _hubService.GetHubIdForPostcode(postcode);
            if (hubId == Guid.Empty)
                return null;

            var addresses = await GetAddresses(postcode);

            List<DeliverableAddress> deliverableAddresses = addresses.Select(a => new DeliverableAddress(
                                                                                        a.CompanyName,
                                                                                        a.AddressLine1,
                                                                                        a.AddressLine2,
                                                                                        a.Town,
                                                                                        a.Postcode,
                                                                                        hubId
                                                                                        )).ToList();



            return deliverableAddresses;
        }

        public async Task<List<Address>> GetAddresses(string postcode)
        {
            postcode = postcode.Trim();

            if (string.IsNullOrWhiteSpace(postcode))
                throw new ArgumentException("Postcode cannot be null or empty.", nameof(postcode));
            if (!PostcodeLocation.IsPostcodeValid(postcode))
                throw new ArgumentException("Postcode is invalid.", nameof(postcode));


            List<Address> addresses = new List<Address>();

            HttpResponseMessage response = await CallApi(postcode);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                var jsonObject = JObject.Parse(responseBody);

                var results = jsonObject.SelectToken("result");

                TextInfo textInfo = new CultureInfo("en-GB", false).TextInfo;

                foreach (var result in results)
                {
                    var address = new Address(
                        result.SelectToken("organisation_name").ToString(),
                        result.SelectToken("line_1").ToString(),
                        result.SelectToken("line_2").ToString(),
                        textInfo.ToTitleCase(result.SelectToken("post_town").ToString().ToLower()),
                        result.SelectToken("postcode").ToString()
                       );

                    addresses.Add(address);
                }

                return addresses;
            }
            else
                return null;
        }

        private async Task<HttpResponseMessage> CallApi(string postcode)
        {
            var client = new HttpClient();
            var response = await client.GetAsync($"https://api.ideal-postcodes.co.uk/v1/postcodes/{postcode}?api_key={_apiKey}");
            return response;
        }
    }
}