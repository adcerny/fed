using Fed.Web.Service.Client;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Fed.Tests.Integration
{
    public class TimeslotsTests
    {
        [Fact]
        public async Task GetTimeslotsForHubWhichDoesntExist()
        {
            var fedClient = FedWebClient.Create(new NullLogger());
            var ex = await Assert.ThrowsAsync<HttpRequestException>(async () => await fedClient.GetTimeslotsAsync(Guid.NewGuid()));
            Assert.Contains("404 (Not Found)", ex.Message);
        }

        [Fact]
        public async Task GetTimeslots()
        {
            var fedClient = FedWebClient.Create(new NullLogger());

            var hubs = await fedClient.GetHubsAsync();
            var hubId = hubs[0].Id;
            var timeslots = await fedClient.GetTimeslotsAsync(hubId);

            Assert.Equal(3 * 5, timeslots.Where(t => t.IsActive).Count());
        }
    }
}
