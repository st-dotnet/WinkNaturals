using AutoMapper;
using Exigo.Api.Client;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNatural.Web.Services.Interfaces;
using WinkNaturals.Infrastructure.Services.DTO;
using WinkNaturals.Infrastructure.Services.ExigoService.CreditCard;
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
        public PartyController(IPartyService partyService, IMapper mapper)
        {
            _partyService = partyService;
            _mapper = mapper;
        }
        // To Create Party

        [HttpPost("CreateParty")]
        public async Task<IActionResult> CreateParty(CreatePartyRequest createPartyRequest)
        {
            return Ok(await _partyService.CreateParty(createPartyRequest));
        }

        // To update party
        [HttpPost("UpdateParty")]
        public async Task<IActionResult> UpdateParty(UpdatePartyRequest updatePartyRequest)
        {
            return Ok(await _partyService.UpdateParty(updatePartyRequest));
        }

        // To Get Parties
        [HttpPost("GetParties")]
        public async Task<IActionResult> GetParties(GetPartiesRequest getPartiesRequest)
        {
            return Ok(await _partyService.GetParties(getPartiesRequest));
        }

        // To Get Party Guests
        [HttpPost("GetPartyGuests")]
        public async Task<IActionResult> GetPartyGuests(GetPartyGuestsRequest getPartyGuestsRequest)
        {
            return Ok(await _partyService.GetPartyGuests(getPartyGuestsRequest));
        }
        // To Create Guest
        [HttpPost("CreateGuest")]
        public async Task<IActionResult> CreateGuest(CreateGuestRequest createGuestRequest)
        {
            return Ok(await _partyService.CreateGuest(createGuestRequest));
        }

        // To update Guest
        [HttpPost("UpdateGuest")]
        public async Task<IActionResult> UpdateGuest(UpdateGuestRequest updateGuestRequest)
        {
            return Ok(await _partyService.UpdateGuest(updateGuestRequest));

        }

        //// To ManageAutoOrder
        //[HttpPost("ManageAutoOrder/{autoOrderViewModel}/{autoOrderId:int}")]
        //public async Task<IActionResult> ManageAutoOrder(ManageAutoOrderViewModel autoOrderViewModel, int autoOrderId)
        //{
        //    return Ok(await _partyService.ManageAutoOrder(autoOrderViewModel, autoOrderId));
        //}
        /// <summary>
        /// ManageAutoOrder
        /// </summary>
        /// <returns></returns>
        [HttpPost("ManageAutoOrder")]
        public async Task<IActionResult> ManageAutoOrder(TransactionalRequestModel transactionRequests,int AutoOrderId)
        {
            return Ok(await _partyService.ManageAutoOrder(transactionRequests, Identity.CustomerID, Identity.Email, AutoOrderId));
        }
    }
}
