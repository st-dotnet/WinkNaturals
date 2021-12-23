
using Exigo.Api.Client;
using System.Threading.Tasks;
using WinkNaturals.Infrastructure.Services.DTO;
using WinkNaturals.Infrastructure.Services.ExigoService.CreditCard;
using static WinkNaturals.Helpers.Constant;

namespace WinkNatural.Web.Services.Interfaces
{
    public interface ICustomerService
    {
        //Get customer
        Task<GetCustomersResponse> GetCustomer(int customerId);
        Task<string> GetImage();

        Task<UpdateCustomerResponse> UpdateCustomer(UpdateCustomerRequest request);

        Task<bool> SendEmailVerification(int customerId, string email);
        //Task<TransactionalResponse> ManageAutoOrder(ManageAutoOrderViewModel autoOrderViewModel, int id);
        //object SaveNewCustomerCreditCard(int customerID, CreditCard card);
        //CreditCard SetCustomerCreditCard(int customerID, CreditCard card);

       // Task DeleteCustomerCreditCard(int customerID, CreditCardType type);
    }
}
