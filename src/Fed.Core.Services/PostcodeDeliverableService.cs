using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Entities;
using Fed.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Services
{
    public class PostcodeDeliverableService : IPostcodeHubService
    {
        IDeliveryBoundaryService _deliveryBoundaryService;
        IPostcodeLocationService _postcodeLocationPaidService;
        IPostcodeLocationHandler _postcodeLocationHandler;

        public PostcodeDeliverableService(
            IDeliveryBoundaryService deliveryBoundaryService,
            IPostcodeLocationService postcodeLocationPaidService,
            IPostcodeLocationHandler postcodeLocationHandler)
        {
            _deliveryBoundaryService = deliveryBoundaryService ?? throw new ArgumentNullException(nameof(deliveryBoundaryService));
            _postcodeLocationPaidService = postcodeLocationPaidService ?? throw new ArgumentNullException(nameof(postcodeLocationPaidService));
            _postcodeLocationHandler = postcodeLocationHandler ?? throw new ArgumentNullException(nameof(postcodeLocationHandler));
        }

        public async Task<Guid> GetHubIdForPostcode(string postcode)
        {
            if (string.IsNullOrWhiteSpace(postcode))
                throw new ArgumentException("Postcode cannot be null or empty.", nameof(postcode));

            postcode = postcode.Trim();

            if (!PostcodeLocation.IsPostcodeValid(postcode))
                throw new ArgumentException("Postcode is invalid.", nameof(postcode));

            postcode = Address.NormalisePostcode(postcode);

            var p = await _postcodeLocationHandler.ExecuteAsync(new Data.Queries.GetPostcodeLocationQuery(postcode));

            if (p == null)
            {
                p = await _postcodeLocationPaidService.GetPostcodeLocation(postcode);

                if (p == null)
                {
                    await _postcodeLocationHandler.ExecuteAsync(
                        new CreateCommand<PostcodeQuery>(
                            new PostcodeQuery
                            {
                                Postcode = postcode,
                                Deliverable = false,
                                QueryDate = DateTime.UtcNow
                            }));
                    throw new KeyNotFoundException($"Postcode {postcode} not found");
                }

                await _postcodeLocationHandler.ExecuteAsync(new CreateCommand<PostcodeLocation>(p));
            }

            var boundaries = await _deliveryBoundaryService.GetDeliveryBoundaryAsync();

            foreach (var b in boundaries)
            {
                if (b.IsInside(p.Coordinate))
                {
                    await _postcodeLocationHandler.ExecuteAsync(
                        new CreateCommand<PostcodeQuery>(
                            new PostcodeQuery
                            {
                                Postcode = postcode,
                                Deliverable = true,
                                QueryDate = DateTime.UtcNow
                            }));
                    return b.HubId;
                }
            }

            var postcodeQuery = new PostcodeQuery { Postcode = postcode, Deliverable = false, QueryDate = DateTime.UtcNow };
            await _postcodeLocationHandler.ExecuteAsync(new CreateCommand<PostcodeQuery>(postcodeQuery));
            return Guid.Empty;
        }
    }
}
