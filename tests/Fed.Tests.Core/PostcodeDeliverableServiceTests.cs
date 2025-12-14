using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Services;
using Fed.Core.Services.Interfaces;
using Fed.Core.ValueTypes;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Fed.Tests.Core
{
    public class PostcodeDeliverableServiceTests
    {
        [Fact]
        public void Constructor_DeliveryBoudrayServiceIsNull_ThrowsException()
        {
            var postcodeLocationPaidService = Substitute.For<IPostcodeLocationService>();
            var postcodeLocationHandler = Substitute.For<IPostcodeLocationHandler>();

            Assert.Throws<ArgumentNullException>(() =>
                new PostcodeDeliverableService(
                    null,
                    postcodeLocationPaidService,
                    postcodeLocationHandler));
        }

        [Fact]
        public void Constructor_PostcodeLocationPaidServiceIsNull_ThrowsException()
        {
            var deliveryBoundaryService = Substitute.For<IDeliveryBoundaryService>();
            var postcodeLocationHandler = Substitute.For<IPostcodeLocationHandler>();

            Assert.Throws<ArgumentNullException>(() =>
                new PostcodeDeliverableService(
                    deliveryBoundaryService,
                    null,
                    postcodeLocationHandler));
        }

        [Fact]
        public void Constructor_PostcodeLocationRepositoryIsNull_ThrowsException()
        {
            var deliveryBoundaryService = Substitute.For<IDeliveryBoundaryService>();
            var postcodeLocationPaidService = Substitute.For<IPostcodeLocationService>();

            Assert.Throws<ArgumentNullException>(() =>
                new PostcodeDeliverableService(
                    deliveryBoundaryService,
                    postcodeLocationPaidService,
                    null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public async Task IsPostcodeDeliverable_PostcodeHasNotBeenProvided_ThrowsException(string postcode)
        {
            var deliveryBoundaryService = Substitute.For<IDeliveryBoundaryService>();
            var postcodeLocationPaidService = Substitute.For<IPostcodeLocationService>();
            var postcodeLocationHandler = Substitute.For<IPostcodeLocationHandler>();

            var postcodeDeliverableService =
                new PostcodeDeliverableService(
                    deliveryBoundaryService,
                    postcodeLocationPaidService,
                    postcodeLocationHandler);

            await Assert.ThrowsAsync<ArgumentException>(() => postcodeDeliverableService.GetHubIdForPostcode(postcode));
        }

        [Fact]
        public async Task IsPostcodeDeliverable_PostcodeHasBeenProvided_GetsPostcodeLocationFromRepositoryForCorrectdPostcode()
        {
            const string postcode = "EC2Y 8DS";

            var deliveryBoundaryService = Substitute.For<IDeliveryBoundaryService>();
            var postcodeLocationPaidService = Substitute.For<IPostcodeLocationService>();
            var postcodeLocationHandler = Substitute.For<IPostcodeLocationHandler>();

            var boundaries = new List<DeliveryBoundary>
            {
                new DeliveryBoundary {
                    HubId = Guid.NewGuid(),
                    Name = "Test boundary",
                    MapCoordinates = new List<MapCoordinate>
                    {
                        new MapCoordinate(0.0, 1.0),
                        new MapCoordinate(2.0, 3.0)
                    }
                }
            };

            deliveryBoundaryService.GetDeliveryBoundaryAsync().Returns(boundaries);

            postcodeLocationHandler.ExecuteAsync(Arg.Any<GetPostcodeLocationQuery>()).Returns(
                Task.FromResult(PostcodeLocation.Create(postcode, 1.0, 2.0)));

            var postcodeDeliverableService =
                new PostcodeDeliverableService(
                    deliveryBoundaryService,
                    postcodeLocationPaidService,
                    postcodeLocationHandler);

            await postcodeDeliverableService.GetHubIdForPostcode(postcode);

            await postcodeLocationHandler.Received().ExecuteAsync(Arg.Is<GetPostcodeLocationQuery>(v => v.Postcode.Equals(postcode)));
        }

        [Fact]
        public async Task IsPostcodeDeliverable_PostcodeHasBeenProvided_GetsPostcodeLocationFromRepositoryOnlyOnce()
        {
            const string postcode = "EC2Y 8DS";

            var deliveryBoundaryService = Substitute.For<IDeliveryBoundaryService>();
            var postcodeLocationPaidService = Substitute.For<IPostcodeLocationService>();
            var postcodeLocationHandler = Substitute.For<IPostcodeLocationHandler>();

            var boundaries = new List<DeliveryBoundary>
            {
                new DeliveryBoundary {
                    HubId = Guid.NewGuid(),
                    Name = "Test boundary",
                    MapCoordinates = new List<MapCoordinate>
                    {
                        new MapCoordinate(0.0, 1.0),
                        new MapCoordinate(2.0, 3.0)
                    }
                }
            };

            deliveryBoundaryService.GetDeliveryBoundaryAsync().Returns(boundaries);

            postcodeLocationHandler.ExecuteAsync(Arg.Any<GetPostcodeLocationQuery>()).Returns(
                Task.FromResult(PostcodeLocation.Create(postcode, 1.0, 2.0)));

            var postcodeDeliverableService = new PostcodeDeliverableService(
                deliveryBoundaryService,
                postcodeLocationPaidService,
                postcodeLocationHandler);

            await postcodeDeliverableService.GetHubIdForPostcode(postcode);

            await postcodeLocationHandler.Received(1).ExecuteAsync(Arg.Any<GetPostcodeLocationQuery>());
        }

        [Fact]
        public async Task IsPostcodeDeliverable_PostcodeLocationIsNotNull_GetsDeliveryBoundaryOnlyOnce()
        {
            const string postcode = "EC2Y 8DS";

            var deliveryBoundaryService = Substitute.For<IDeliveryBoundaryService>();
            var postcodeLocationPaidService = Substitute.For<IPostcodeLocationService>();
            var postcodeLocationHandler = Substitute.For<IPostcodeLocationHandler>();

            var boundaries = new List<DeliveryBoundary>
            {
                new DeliveryBoundary {
                    HubId = Guid.NewGuid(),
                    Name = "Test boundary",
                    MapCoordinates = new List<MapCoordinate>
                    {
                        new MapCoordinate(0.0, 1.0),
                        new MapCoordinate(2.0, 3.0)
                    }
                }
            };

            deliveryBoundaryService.GetDeliveryBoundaryAsync().Returns(boundaries);

            postcodeLocationHandler.ExecuteAsync(Arg.Any<GetPostcodeLocationQuery>()).Returns(
                Task.FromResult(PostcodeLocation.Create(postcode, 1.0, 2.0)));

            var postcodeDeliverableService = new PostcodeDeliverableService(
                deliveryBoundaryService,
                postcodeLocationPaidService,
                postcodeLocationHandler);

            await postcodeDeliverableService.GetHubIdForPostcode(postcode);

            await deliveryBoundaryService.Received(1).GetDeliveryBoundaryAsync();
        }

        [Theory]
        [InlineData("SE1 9SG", 51.504962, -0.087651, "10fe857b-d311-46b8-a41d-f41a34597f1c")]
        [InlineData("WC1X 8PH", 51.518545, -0.111816, "10fe857b-d311-46b8-a41d-f41a34597f1c")]
        [InlineData("E1 7NF", 51.518179, -0.075465, "10fe857b-d311-46b8-a41d-f41a34597f1c")]
        [InlineData("EC3N 2ET", 51.513081, -0.076475, "10fe857b-d311-46b8-a41d-f41a34597f1c")]
        [InlineData("SE1 8TG", 51.50398, -0.108643, "10fe857b-d311-46b8-a41d-f41a34597f1c")]
        [InlineData("SE1 8SW", 51.502706, -0.113277, "00000000-0000-0000-0000-000000000000")]
        [InlineData("SE1 6LW", 51.495853, -0.1009, "00000000-0000-0000-0000-000000000000")]
        [InlineData("SE1 2UP", 51.503378, -0.076579, "00000000-0000-0000-0000-000000000000")]
        [InlineData("EC3N 2ET", 51.509741, -0.076325, "00000000-0000-0000-0000-000000000000")]
        [InlineData("EC1M 4DF", 51.520991, -0.101235, "00000000-0000-0000-0000-000000000000")]
        public async Task IsPostcodeDeliverable_WhenValidatedAgainstBoundary_ReturnsCorrectResult(
            string postcode,
            double longditude,
            double latitude,
            string expectedResult)
        {
            var deliveryBoundaryService = Substitute.For<IDeliveryBoundaryService>();
            var postcodeLocationPaidService = Substitute.For<IPostcodeLocationService>();
            var postcodeLocationHandler = Substitute.For<IPostcodeLocationHandler>();

            var coordinates = new MapCoordinate(longditude, latitude);

            var hubId = Guid.Parse(expectedResult);

            var boundary = new DeliveryBoundary
            {

                HubId = Guid.Parse(expectedResult),
                Name = "Test boundary",
                MapCoordinates = new List<MapCoordinate>
                {
                    new MapCoordinate(51.4951625,-0.0938215),
                    new MapCoordinate(51.5015754,-0.0794856),
                    new MapCoordinate(51.5051288,-0.0804299),
                    new MapCoordinate(51.5099108,-0.0777258),
                    new MapCoordinate(51.5186453,-0.0733478),
                    new MapCoordinate(51.5212199,-0.0782191),
                    new MapCoordinate(51.5212733,-0.0986499),
                    new MapCoordinate(51.5188696,-0.1130716),
                    new MapCoordinate(51.5074904,-0.1153036),
                    new MapCoordinate(51.4962152,-0.1039722),
                    new MapCoordinate(51.4951625,-0.0938215)
                }
            };

            var boundaries = new List<DeliveryBoundary> { boundary };
            deliveryBoundaryService.GetDeliveryBoundaryAsync().Returns(boundaries);

            postcodeLocationHandler.ExecuteAsync(Arg.Any<GetPostcodeLocationQuery>()).Returns(
                Task.FromResult(PostcodeLocation.Create(postcode, coordinates.Latitude, coordinates.Longitude)));

            var postcodeDeliverableService = new PostcodeDeliverableService(
                deliveryBoundaryService,
                postcodeLocationPaidService,
                postcodeLocationHandler);

            var result = await postcodeDeliverableService.GetHubIdForPostcode(postcode);

            Assert.Equal(hubId, result);
        }

        [Fact]
        public async Task IsPostcodeDeliverable_PostcodeLocationIsNull_TriesToRetrievePostcodeLocationFromPostcodeLocationServiceOnlyOnce()
        {
            const string postcode = "EC2Y 8DS";

            var deliveryBoundaryService = Substitute.For<IDeliveryBoundaryService>();
            var postcodeLocationPaidService = Substitute.For<IPostcodeLocationService>();
            var postcodeLocationHandler = Substitute.For<IPostcodeLocationHandler>();

            var boundaries = new List<DeliveryBoundary>
            {
                new DeliveryBoundary {
                    HubId = Guid.NewGuid(),
                    Name = "Test boundary",
                    MapCoordinates = new List<MapCoordinate>
                    {
                        new MapCoordinate(0.0, 1.0),
                        new MapCoordinate(2.0, 3.0)
                    }
                }
            };

            deliveryBoundaryService.GetDeliveryBoundaryAsync().Returns(boundaries);

            postcodeLocationHandler.ExecuteAsync(Arg.Any<GetPostcodeLocationQuery>()).Returns(Task.FromResult((PostcodeLocation)null));
            postcodeLocationPaidService.GetPostcodeLocation(Arg.Any<string>()).Returns(Task.FromResult(PostcodeLocation.Create(postcode, 1.0, 2.0)));

            var postcodeDeliverableService = new PostcodeDeliverableService(
                deliveryBoundaryService,
                postcodeLocationPaidService,
                postcodeLocationHandler);

            await postcodeDeliverableService.GetHubIdForPostcode(postcode);

            await postcodeLocationPaidService.Received(1).GetPostcodeLocation(Arg.Any<string>());
        }

        [Fact]
        public async Task IsPostcodeDeliverable_BothPostcodeLocationServicesReturnNull_ThrowsException()
        {
            const string postcode = "EC2Y 8DS";

            var deliveryBoundaryService = Substitute.For<IDeliveryBoundaryService>();
            var postcodeLocationPaidService = Substitute.For<IPostcodeLocationService>();
            var postcodeLocationHandler = Substitute.For<IPostcodeLocationHandler>();

            var boundaries = new List<DeliveryBoundary>
            {
                new DeliveryBoundary {
                    HubId = Guid.NewGuid(),
                    Name = "Test boundary",
                    MapCoordinates = new List<MapCoordinate>
                    {
                        new MapCoordinate(0.0, 1.0),
                        new MapCoordinate(2.0, 3.0)
                    }
                }
            };

            deliveryBoundaryService.GetDeliveryBoundaryAsync().Returns(boundaries);

            postcodeLocationHandler.ExecuteAsync(Arg.Any<GetPostcodeLocationQuery>()).Returns(Task.FromResult((PostcodeLocation)null));
            postcodeLocationPaidService.GetPostcodeLocation(Arg.Any<string>()).Returns(Task.FromResult((PostcodeLocation)null));

            var postcodeDeliverableService = new PostcodeDeliverableService(
                deliveryBoundaryService,
                postcodeLocationPaidService,
                postcodeLocationHandler);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => postcodeDeliverableService.GetHubIdForPostcode(postcode));
        }

        [Fact]
        public async Task IsPostcodeDeliverable_WhenPostcodeReturnedFromService_UpdatesRepositoryOnlyOnce()
        {
            const string postcode = "EC2Y 8DS";

            var deliveryBoundaryService = Substitute.For<IDeliveryBoundaryService>();
            var postcodeLocationPaidService = Substitute.For<IPostcodeLocationService>();
            var postcodeLocationHandler = Substitute.For<IPostcodeLocationHandler>();

            var boundaries = new List<DeliveryBoundary>
            {
                new DeliveryBoundary {
                    HubId = Guid.NewGuid(),
                    Name = "Test boundary",
                    MapCoordinates = new List<MapCoordinate>
                    {
                        new MapCoordinate(0.0, 1.0),
                        new MapCoordinate(2.0, 3.0)
                    }
                }
            };

            deliveryBoundaryService.GetDeliveryBoundaryAsync().Returns(boundaries);

            postcodeLocationHandler.ExecuteAsync(Arg.Any<GetPostcodeLocationQuery>()).Returns(Task.FromResult((PostcodeLocation)null));
            postcodeLocationPaidService.GetPostcodeLocation(Arg.Any<string>()).Returns(Task.FromResult(PostcodeLocation.Create(postcode, 1.0, 2.0)));

            var postcodeDeliverableService = new PostcodeDeliverableService(
                deliveryBoundaryService,
                postcodeLocationPaidService,
                postcodeLocationHandler);

            await postcodeDeliverableService.GetHubIdForPostcode(postcode);

            await postcodeLocationHandler.Received(1).ExecuteAsync(Arg.Any<CreateCommand<PostcodeLocation>>());
        }

        [Fact]
        public async Task IsPostcodeDeliverable_WhenPostcodeReturnedFromService_UpdatesRepositoryWithCorrectPostcode()
        {
            const string postcode = "EC2Y 8DS";

            var deliveryBoundaryService = Substitute.For<IDeliveryBoundaryService>();
            var postcodeLocationPaidService = Substitute.For<IPostcodeLocationService>();
            var postcodeLocationHandler = Substitute.For<IPostcodeLocationHandler>();

            var postcodeLocation = PostcodeLocation.Create(postcode, 1.0, 2.0);

            var boundaries = new List<DeliveryBoundary>
            {
                new DeliveryBoundary {
                    HubId = Guid.NewGuid(),
                    Name = "Test boundary",
                    MapCoordinates = new List<MapCoordinate>
                    {
                        new MapCoordinate(0.0, 1.0),
                        new MapCoordinate(2.0, 3.0)
                    }
                }
            };

            deliveryBoundaryService.GetDeliveryBoundaryAsync().Returns(boundaries);

            postcodeLocationHandler.ExecuteAsync(Arg.Any<GetPostcodeLocationQuery>()).Returns(Task.FromResult((PostcodeLocation)null));
            postcodeLocationPaidService.GetPostcodeLocation(Arg.Any<string>()).Returns(Task.FromResult(postcodeLocation));

            var postcodeDeliverableService = new PostcodeDeliverableService(
                deliveryBoundaryService,
                postcodeLocationPaidService,
                postcodeLocationHandler);

            await postcodeDeliverableService.GetHubIdForPostcode(postcode);

            await postcodeLocationHandler.Received().ExecuteAsync(Arg.Is<CreateCommand<PostcodeLocation>>(v => v.Object == postcodeLocation));
        }

        [Fact]
        public async Task IsPostcodeDeliverable_PostcodeLocationIsNull_TriesToRetrievePostcodeLocationFromPostcodeLocationServiceForCorrectPostcode()
        {
            const string postcode = "EC2Y 8DS";

            var deliveryBoundaryService = Substitute.For<IDeliveryBoundaryService>();
            var postcodeLocationPaidService = Substitute.For<IPostcodeLocationService>();
            var postcodeLocationHandler = Substitute.For<IPostcodeLocationHandler>();

            var boundary = new DeliveryBoundary(
                new List<MapCoordinate>
                {
                    new MapCoordinate(0.0, 1.0),
                    new MapCoordinate(2.0, 3.0)
                });

            var boundaries = new List<DeliveryBoundary>
            {
                new DeliveryBoundary {
                    HubId = Guid.NewGuid(),
                    Name = "Test boundary",
                    MapCoordinates = new List<MapCoordinate>
                    {
                        new MapCoordinate(0.0, 1.0),
                        new MapCoordinate(2.0, 3.0)
                    }
                }
            };

            deliveryBoundaryService.GetDeliveryBoundaryAsync().Returns(boundaries);

            postcodeLocationHandler.ExecuteAsync(Arg.Any<GetPostcodeLocationQuery>()).Returns(Task.FromResult((PostcodeLocation)null));
            postcodeLocationPaidService.GetPostcodeLocation(Arg.Any<string>()).Returns(Task.FromResult(PostcodeLocation.Create(postcode, 1.0, 2.0)));

            var postcodeDeliverableService = new PostcodeDeliverableService(
                deliveryBoundaryService,
                postcodeLocationPaidService,
                postcodeLocationHandler);

            await postcodeDeliverableService.GetHubIdForPostcode(postcode);

            await postcodeLocationPaidService.Received().GetPostcodeLocation(postcode);
        }
    }
}
