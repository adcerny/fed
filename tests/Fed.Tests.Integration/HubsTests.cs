using Fed.Core.Entities;
using Fed.Web.Service.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Fed.Tests.Integration
{
    public class HubsTests
    {
        [Fact]
        public async Task UpdateHubWhichDoesntExist()
        {
            var fedClient = FedWebClient.Create(new NullLogger());
            var hub = new Hub(Guid.NewGuid(), "foobar", "foobar", "foobar", "foobar", TimeSpan.FromHours(18), DateTime.UtcNow);
            var ex = await Assert.ThrowsAsync<HttpRequestException>(async () => await fedClient.UpdateHubAsync(hub));
            Assert.Contains("404 (Not Found)", ex.Message);
        }

        [Fact]
        public async Task GetAndUpdateHub()
        {
            var fedClient = FedWebClient.Create(new NullLogger());

            var hubs = await fedClient.GetHubsAsync();
            var hub = hubs[0];

            var randomId = Guid.NewGuid().ToString("N").Substring(0, 6);
            var name = $"Test Hub {randomId}";
            var orderDeadline = TimeSpan.FromHours(19);
            var addressLine1 = "London Road 123";
            var addressLine2 = "City of London XYZ";
            var postcode = "EC1 XYZ";

            hub.Name = name;
            hub.OrderDeadline = orderDeadline;
            hub.AddressLine1 = addressLine1;
            hub.AddressLine2 = addressLine2;
            hub.Postcode = postcode;

            var updatedHub = await fedClient.UpdateHubAsync(hub);

            Assert.Equal(hub.Id, updatedHub.Id);
            Assert.Equal(name, updatedHub.Name);
            Assert.Equal(addressLine1, updatedHub.AddressLine1);
            Assert.Equal(addressLine2, updatedHub.AddressLine2);
            Assert.Equal(postcode, updatedHub.Postcode);
            Assert.Equal(orderDeadline, updatedHub.OrderDeadline);

            var updatedHubs = await fedClient.GetHubsAsync();
            var firstHub = updatedHubs[0];

            Assert.Equal(hub.Id, firstHub.Id);
            Assert.Equal(name, firstHub.Name);
            Assert.Equal(addressLine1, firstHub.AddressLine1);
            Assert.Equal(addressLine2, firstHub.AddressLine2);
            Assert.Equal(postcode, firstHub.Postcode);
            Assert.Equal(orderDeadline, firstHub.OrderDeadline);
        }
    }
}
