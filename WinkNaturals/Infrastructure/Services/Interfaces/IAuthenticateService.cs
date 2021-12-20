using Exigo.Api.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO.Customer;
using WinkNaturals.Infrastructure.Services.ExigoService;
using WinkNaturals.Models;
using VerifyAddressResponse = WinkNaturals.Infrastructure.Services.ExigoService.VerifyAddressResponse;

namespace WinkNatural.Web.Services.Interfaces
{
    public interface IAuthenticateService
    {
        //create customer
        Task<CreateCustomerResponse> CreateCustomer(CreateCustomerRequest request);
        //Signin customer
        Task<CustomerCreateResponse> LoginCustomer(AuthenticateCustomerRequest request);

        //Update customer password
        Task<CustomerUpdateResponse> UpdateCustomerPassword(CustomerUpdateRequest request);

        //Send forgot password email
        Task<CustomerUpdateResponse> SendForgotPasswordEmail(CustomerUpdateRequest request);

        //Check if email/username is exists or not
        Task<bool> IsEmailOrUsernameExists(CustomerValidationRequest request);

        CustomerCreateResponse Authenticate(AuthenticateCustomerRequest model);
        IEnumerable<CustomerCreateModel> GetAll();
        CustomerCreateModel GetById(int id);
        Task<Address> SaveNewCustomerAddress(int customerId, Address address);

       Task<IAddress> ValidateAddress(IAddress address);
       

    }
}
