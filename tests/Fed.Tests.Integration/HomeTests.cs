using Fed.Web.Service.Client;
using System.Threading.Tasks;
using Xunit;

namespace Fed.Tests.Integration
{
    public class HomeTests
    {
        [Fact]
        public async Task Ping()
        {
            var fedClient = FedWebClient.Create(new NullLogger());

            var response = await fedClient.PingAsync();

            Assert.Equal("pong", response);
        }
    }
}