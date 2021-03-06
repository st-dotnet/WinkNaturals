using Exigo.Api.Client;
using System.Threading.Tasks;
using WinkNaturals.Infrastructure.Services.DTO;

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
        Task<TransactionalResponse> ManageAutoOrder(ManageAutoOrderViewModel autoOrderViewModel, int id);
    }
}
