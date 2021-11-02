using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNatural.Web.Services.Services;
using WinkNaturals.Models;


namespace WinkNatural.Web.Services.Interfaces
{
    public interface IPaymentService
    {
        BankCardTransaction ProcessPayment(WinkPaymentRequest winkPaymentRequest);

        Task<AddCardResponse> CreateCustomerProfile(GetPaymentRequest model);

        ProcessPaymentMethodTransactionResponse ProcessPaymentMethod(GetPaymentRequest getPaymentProPayModel);

        Task<AddCardResponse> PaymentUsingAuthorizeNet(AddPaymentModel model);


    }
}
