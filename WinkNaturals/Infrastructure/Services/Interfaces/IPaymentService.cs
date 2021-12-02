using RestSharp;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNatural.Web.Services.Services;
using WinkNaturals.Infrastructure.Services.Token;

namespace WinkNatural.Web.Services.Interfaces
{
    public interface IPaymentService
    {
        BankCardTransaction ProcessPayment(WinkPaymentRequest winkPaymentRequest);

        Task<AddCardResponse> CreateCustomerProfile(GetPaymentRequest model);

        ProcessPaymentMethodTransactionResponse ProcessPaymentMethod(GetPaymentRequest getPaymentProPayModel);

        AddCardResponse PaymentUsingAuthorizeNet(AddPaymentModel model);

        GenerateTokenResponse GenerateCreditCardToken(string cardNumber);


    }
}
