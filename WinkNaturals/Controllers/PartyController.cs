using Microsoft.AspNetCore.Mvc;
using Resources;
using System;
using WinkNatural.Web.Services.Interfaces;
using WinkNatural.Web.Services.Utilities;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNaturals.Models;
using Exigo.Api.Client;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using WinkNatural.Web.Common;
using WinkNatural.Web.Services.DTO;
using WinkNatural.Web.Common.Utils;

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
    }
}
