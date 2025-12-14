using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Fed.Web.Service.Controllers
{
    [ApiController]
    public class MarketingController : ControllerBase
    {
        private readonly IContactsHandler _contactsHandler;

        public MarketingController(IContactsHandler contactsHandler)
        {
            _contactsHandler = contactsHandler ?? throw new ArgumentNullException(nameof(contactsHandler));
        }

        [HttpPut("/marketingConsent/{emailAddress}")]
        public async Task<ActionResult<bool>> SetMarketingConsent(string emailAddress, bool isMarketingConsented)
        {
            await _contactsHandler.ExecuteAsync(new UpdateMarketingConsentCommand(emailAddress, isMarketingConsented));

            return true;
        }
    }
}