
using Exigo.Api.Client;
using System.Threading.Tasks;
using WinkNaturals.Infrastructure.Services.ExigoService;
using VerifyAddressResponse = WinkNaturals.Infrastructure.Services.ExigoService.VerifyAddressResponse;

namespace WinkNatural.Web.Services.Interfaces
{
    public interface ICustomerService
    {
        //Get customer
        Task<GetCustomersResponse> GetCustomer(int customerId);
        Task<string> GetImage();

        Task<UpdateCustomerResponse> UpdateCustomer(UpdateCustomerRequest request);

        Task<bool> SendEmailVerification(int customerId, string email);

        Task<VerifyAddressResponse> VerifyAddress(Address address);

    }
}
