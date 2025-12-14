using Fed.Core.Entities;
using Fed.Core.Services.Interfaces;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fed.Api.External.PostcodesIOService
{
    public class PostcodesIOService : IPostcodeLocationService
    {
        public async Task<PostcodeLocation> GetPostcodeLocation(string postcode)
        {

            postcode = postcode.Trim();

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync($"https://api.postcodes.io/postcodes/{postcode}");

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();

                var jsonObject = JObject.Parse(responseBody);

                double latitude;
                double longitude;
                if (!double.TryParse(jsonObject.SelectToken("result.latitude").ToString(), out latitude))
                    return null;
                if (!double.TryParse(jsonObject.SelectToken("result.longitude").ToString(), out longitude))
                    return null;

                return PostcodeLocation.Create(postcode, latitude, longitude);
            }
            else
            {
                return null;
            }

        }
    }
}
