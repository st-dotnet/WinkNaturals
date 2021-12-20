using AutoMapper;
using Exigo.Api.Client;
using Microsoft.AspNetCore.Mvc;
using WinkNatural.Web.Services.Interfaces;
using WinkNaturals.Infrastructure.Services.Interfaces;

namespace WinkNaturals.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PartyController : BaseController
    {
        //  private readonly ExigoApiClient exigoApiClient = new ExigoApiClient(ExigoConfig.Instance.CompanyKey, ExigoConfig.Instance.LoginName, ExigoConfig.Instance.Password);

        private readonly IPartyService _partyService;
        private readonly IMapper _mapper;
        private readonly IAccountService _accountService;

        public PartyController(IPartyService partyService, IMapper mapper, IAccountService accountService)
        {
            _partyService = partyService;
            _mapper = mapper;
            _accountService = accountService;
        }
        // To Create Party

        [HttpPost("CreateParty")]
        public IActionResult CreateParty(CreatePartyRequest createPartyRequest)
        {
            return Ok(_partyService.CreateParty(createPartyRequest));
        }

        // To update party
        [HttpPost("UpdateParty")]
        public IActionResult UpdateParty(UpdatePartyRequest updatePartyRequest)
        {
            return Ok(_partyService.UpdateParty(updatePartyRequest));
        }

        // To Get Parties
        [HttpPost("GetParties")]
        public IActionResult GetParties(GetPartiesRequest getPartiesRequest)
        {
            return Ok(_partyService.GetParties(getPartiesRequest));
        }

        // To Get Party Guests
        [HttpPost("GetPartyGuests")]
        public IActionResult GetPartyGuests(GetPartyGuestsRequest getPartyGuestsRequest)
        {
            return Ok(_partyService.GetPartyGuests(getPartyGuestsRequest));
        }

        // To Create Guest
        [HttpPost("CreateGuest")]
        public IActionResult CreateGuest(CreateGuestRequest createGuestRequest)
        {
            return Ok(_partyService.CreateGuest(createGuestRequest));
        }

        // To update Guest
        [HttpPost("UpdateGuest")]
        public IActionResult UpdateGuest(UpdateGuestRequest updateGuestRequest)
        {
            return Ok(_partyService.UpdateGuest(updateGuestRequest));

        }
        //[HttpPost("SaveAddress")]
        //public IActionResult SaveAddress(int customerId, Address address)
        //{
        //    return Ok((_accountService.SaveAddress(customerId, address)));
        //}
    }
}
