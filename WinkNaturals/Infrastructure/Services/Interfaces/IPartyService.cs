using Exigo.Api.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNaturals.Models;


namespace WinkNatural.Web.Services.Interfaces
{
    public interface IPartyService
    {
        Task<CreatePartyResponse> CreateParty(CreatePartyRequest createPartyRequest);
        Task<UpdatePartyResponse> UpdateParty(UpdatePartyRequest updatePartyRequest);
        Task<GetPartiesResponse> GetParties(GetPartiesRequest getPartiesRequest);
        Task<GetPartyGuestsResponse> GetPartyGuests(GetPartyGuestsRequest getPartyGuestsRequest);
        Task<CreateGuestResponse> CreateGuest(CreateGuestRequest createGuestRequest);
        Task<UpdateGuestResponse> UpdateGuest(UpdateGuestRequest updateGuestRequest);

    }
}
